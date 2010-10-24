using System;
using System.Collections.Generic;
using System.Text;
using Scopus.Auxiliary;
using Scopus.Exceptions;
using Scopus.LexicalAnalysis.Algorithms;
using Scopus.LexicalAnalysis.RegularExpressions;
using Scopus.SyntaxAnalysis;

namespace Scopus.LexicalAnalysis
{
    public class KeywordsTokenizer : ITokenizer
    {
        protected const int INITIAL_STATE = 0;
        protected const int WRONG_STATE = -1;

        private const int CHARS_COUNT = Byte.MaxValue + 1;
        internal const int INITIAL_STATES_COUNT = Byte.MaxValue + 1;

// ReSharper disable CharImplicitlyConvertedToNumeric
        protected const int ASCII_ZERO = '0';
        protected const int ASCII_NINE = '9';

        protected const int ASCII_A_SM = 'a';
        protected const int ASCII_Z_SM = 'z';
        protected const int ASCII_A_BG = 'A';
        protected const int ASCII_Z_BG = 'Z';

        protected const int ASCII_CR = '\r';
        protected const int ASCII_LF = '\n';

        protected const int ASCII_MINUS = '-';
        protected const int ASCII_PLUS = '+';
        protected const int ASCII_DOT = '.';
        protected const int ASCII_UNDERSCORE = '_';
// ReSharper restore CharImplicitlyConvertedToNumeric

        protected const char ASCII_SPACE = ' ';
        protected const char ASCII_TAB = '\t';


        protected HashSet<int> AcceptingStates = new HashSet<int>();

        protected IDProvider ClassIdProvider = new IDProvider();
        protected int EndlineStateValue;

        /*
		protected Terminal IntegerNumberValue;
		public Terminal IntegerNumber
    	{
    		get
    		{
    			InitIntegerNumberToken();
    			return IntegerNumberValue;
    		}
    	}
		protected int IntegerNumberStateValue;
		//public int IntegerNumberState
		//{
		//    get
		//    {
		//        InitIntegerNumberToken();
		//        return IntegerNumberStateValue;
		//    }
		//}
		
		protected Terminal FloatNumberValue;
		public Terminal FloatNumber
		{
			get
			{
				InitFloatNumberToken();
				return FloatNumberValue;
			}
		}
		protected int FloatNumberStateValue;
		//public int FloatNumberState
		//{
		//    get
		//    {
		//        InitFloatNumberToken();
		//        return FloatNumberStateValue;
		//    }
		//}
		*/
        protected Terminal EndlineValue;
        protected IDProvider StateIdProvider = new IDProvider();

        protected Dictionary<int, int> StateToTokenID = new Dictionary<int, int>();
                                       // associates state IDs with token IDs

        protected List<int[]> TransitionsTable = new List<int[]>(INITIAL_STATES_COUNT);

        public KeywordsTokenizer()
        {
            HiddenTokens = new HashSet<int>();

            CreateNewState(); // starting from 0

            ClassIdProvider.GetNext();
                // the ID == 0 is reserved for synthetic (generated at enumeration) END_MARK (<end>) token
        }

        public Terminal Endline
        {
            get
            {
                InitEndlineToken();
                return EndlineValue;
            }
        }

        public HashSet<int> HiddenTokens { get; set; }

        /*
		protected Terminal StringIdentifierValue;
		public Terminal StringIdentifier
		{
			get
			{
				InitStringIdentifierToken();
				return StringIdentifierValue;
			}
		}
		protected int StringIdentifierStateValue;
		//public int StringIdentifierState
		//{
		//    get
		//    {
		//        InitStringIdentifierToken();
		//        return StringIdentifierStateValue;
		//    }
		//}
		
		*/

        #region ITokenizer Members

        public int[] TokensIndices { get; set; }
        public int[] TokensClasses { get; set; }

        public void SetTransitionFunction(ITransitionFunction function)
        {
            throw new NotImplementedException();
        }

        public Terminal UseTerminal(RegExp terminal)
        {
            throw new NotImplementedException();
        }

        public void IgnoreTerminal(RegExp ignoree)
        {
            throw new NotImplementedException();
        }

        public Terminal UseEpsilon()
        {
            throw new NotImplementedException();
        }

        public int TotalTokensCount
        {
            get { return ClassIdProvider.GetCurrent() + 1; // plus one for predefined <end>
            }
        }

        public void SetEncoding(Encoding encoding)
        {
            // throw new NotImplementedException();
        }

        public void BuildTransitions()
        {
            //throw new NotImplementedException();
        }

        public int Tokenize(byte[] buffer, int offset, int length)
        {
            int tokensCount = 0;

            for (int i = offset; i < offset + length;)
            {
                TokensIndices[tokensCount] = i;
                int tokenClass;
                int tokenLength = MatchToken(buffer, i, length, out tokenClass);
                TokensClasses[tokensCount] = tokenClass;

                i += tokenLength;

                tokensCount++;
            }

            return tokensCount - 1; // returns index of last token
        }

        #endregion

        public Terminal AddToken(string tokenString)
        {
            return new Terminal(tokenString, AddToken(tokenString, -1).Value);
        }

        private KeyValuePair<int, int> AddToken(string tokenString, int desiredClass)
        {
            if (String.IsNullOrEmpty(tokenString)) throw new ArgumentNullException("tokenString");

            int state = IntegrateToken(Encoding.ASCII.GetBytes(tokenString));

            AcceptingStates.Add(state);
            int tokenClass = (desiredClass != -1) ? desiredClass : ClassIdProvider.GetNext();
            StateToTokenID[state] = tokenClass;

            return new KeyValuePair<int, int>(state, tokenClass);
        }

        private int IntegrateToken(byte[] tokenBytes)
        {
            int oldBranchCurState = INITIAL_STATE;
            int newBranchCurState = INITIAL_STATE;

            oldBranchCurState = TransitionsTable[oldBranchCurState][tokenBytes[0]];
            newBranchCurState = CreateAndInitNewState(newBranchCurState, tokenBytes[0]);

            for (int i = 1; i < tokenBytes.Length; i++)
            {
                if (oldBranchCurState != WRONG_STATE)
                {
                    TransitionsTable[newBranchCurState] = (int[]) TransitionsTable[oldBranchCurState].Clone();

                    if (AcceptingStates.Contains(oldBranchCurState))
                    {
                        AcceptingStates.Add(newBranchCurState);
                        StateToTokenID[newBranchCurState] = StateToTokenID[oldBranchCurState];
                    }
                    oldBranchCurState = TransitionsTable[oldBranchCurState][tokenBytes[i]];
                }
                newBranchCurState = CreateAndInitNewState(newBranchCurState, tokenBytes[i]);
            }

            if (oldBranchCurState != WRONG_STATE)
                TransitionsTable[newBranchCurState] = (int[]) TransitionsTable[oldBranchCurState].Clone();

            return newBranchCurState;
        }

        internal Terminal AddTokens(params string[] tokenStrings)
        {
            if (tokenStrings.Length == 0) return null;

            int tokenClass = AddToken(tokenStrings[0]).TokenClassID;
            for (int i = 1; i < tokenStrings.Length; i++)
                AddToken(tokenStrings[i], tokenClass);

            return new Terminal(tokenStrings[0], tokenClass);
        }

        /*
		public void AddCommentWithBordersToken(string startToken, string endToken)
		{
			if (String.IsNullOrEmpty(startToken)) throw new ArgumentNullException("startToken");
			if (String.IsNullOrEmpty(endToken)) throw new ArgumentNullException("endToken");

			int commentaryStateFinal = AddTokenWithBorders(startToken, endToken);

			AcceptingStates.Add(commentaryStateFinal);
			StateToTokenID[commentaryStateFinal] = ClassIdProvider.GetNext();
			HiddenTokens.Add(ClassIdProvider.GetCurrent());
		}
		public void AddCommentToEndlineToken(string startToken)
		{
			if (String.IsNullOrEmpty(startToken)) throw new ArgumentNullException("startToken");
			
			InitEndlineToken();

			int startTerminalState = AddToken(startToken, -1).Key;
			int commentStateFinal = CreateNewState();

			for (int i = 0; i < CHARS_COUNT; i++)
				TransitionsTable[startTerminalState][i] = startTerminalState;

			TransitionsTable[startTerminalState][ASCII_CR] = commentStateFinal;
			TransitionsTable[startTerminalState][ASCII_LF] = commentStateFinal;

			AcceptingStates.Add(commentStateFinal);
			StateToTokenID[commentStateFinal] = ClassIdProvider.GetNext();
			HiddenTokens.Add(ClassIdProvider.GetCurrent());
		}
		public Terminal AddStringConstantToken(string startToken, string endToken)
		{
			if (String.IsNullOrEmpty(startToken)) throw new ArgumentNullException("startToken");
			if (String.IsNullOrEmpty(endToken)) throw new ArgumentNullException("endToken");

			int stringConstantStateFinal = AddTokenWithBorders(startToken, endToken);

			AcceptingStates.Add(stringConstantStateFinal);
			StateToTokenID[stringConstantStateFinal] = ClassIdProvider.GetNext();
			
			return new Terminal(Lexer.STRING_CONSTANT_TOKEN_NAME, ClassIdProvider.GetCurrent());
		}
		public Terminal AddStringConstantToEndlineToken(string startToken)
		{
			if (String.IsNullOrEmpty(startToken)) throw new ArgumentNullException("startToken");

			InitEndlineToken();

			int startTerminalState = AddToken(startToken, -1).Key;
			int stringConstantStateFinal = CreateNewState();

			for (int i = 0; i < CHARS_COUNT; i++)
				TransitionsTable[startTerminalState][i] = startTerminalState;

			TransitionsTable[startTerminalState][ASCII_CR] = stringConstantStateFinal;
			TransitionsTable[startTerminalState][ASCII_LF] = stringConstantStateFinal;

			AcceptingStates.Add(stringConstantStateFinal);
			StateToTokenID[stringConstantStateFinal] = ClassIdProvider.GetNext();

			return new Terminal(Lexer.STRING_CONSTANT_TOKEN_NAME, ClassIdProvider.GetCurrent());
		}
		protected int AddTokenWithBorders(string startToken, string endToken)
		{
			int startTerminalState = AddToken(startToken, -1).Key;

			int curIntermediateState = startTerminalState;
			foreach (byte b in Encoding.ASCII.GetBytes(endToken))
			{
				for (int i = 0; i < CHARS_COUNT; i++)
					TransitionsTable[curIntermediateState][i] = startTerminalState;
				curIntermediateState = CreateAndInitNewState(curIntermediateState, b);
			}

			return curIntermediateState;
		}
		*/

        /// <summary>
        /// Hides token, preventing tokenizer to supply it to parser.
        /// </summary>
        /// <param name="term">Terminal symbol of previously added token.</param>
        public void HideToken(Terminal term)
        {
            HiddenTokens.Add(term.TokenClassID);
        }

/*		
		protected virtual void InitIntegerNumberToken()
		{
			if (IntegerNumberValue == null)
			{
				IntegerNumberStateValue = CheckCreateState(ASCII_ZERO, INITIAL_STATE);
				for (int i = ASCII_ZERO; i <= ASCII_NINE; i++)
				{
					TransitionsTable[INITIAL_STATE][i] = IntegerNumberStateValue;
					TransitionsTable[IntegerNumberStateValue][i] = IntegerNumberStateValue;
				}

				AcceptingStates.Add(IntegerNumberStateValue);
				StateToTokenID[IntegerNumberStateValue] = ClassIdProvider.GetNext();
				IntegerNumberValue = new Terminal(Lexer.INTEGER_NUMBER_TOKEN_NAME, ClassIdProvider.GetCurrent());
			}
		}
		protected virtual void InitFloatNumberToken()
		{
			if (FloatNumberValue == null)
			{
				InitIntegerNumberToken();

				FloatNumberStateValue = CheckCreateState(ASCII_DOT, IntegerNumberStateValue);
				for (int i = ASCII_ZERO; i <= ASCII_NINE; i++)
					TransitionsTable[FloatNumberStateValue][i] = FloatNumberStateValue;

				AcceptingStates.Add(FloatNumberStateValue);
				// this allows both floats and ints to be convertible to one another, which greatly simplifies grammar
				StateToTokenID[FloatNumberStateValue] = IntegerNumber.TokenClassID;
				FloatNumberValue = new Terminal(Lexer.FLOAT_NUMBER_TOKEN_NAME, IntegerNumber.TokenClassID);
			}
		}
 * */

        protected virtual void InitEndlineToken()
        {
            if (EndlineValue == null)
            {
                EndlineStateValue = CreateNewState();
                TransitionsTable[INITIAL_STATE][ASCII_LF] = EndlineStateValue;
                TransitionsTable[INITIAL_STATE][ASCII_CR] = EndlineStateValue;
                TransitionsTable[EndlineStateValue][ASCII_LF] = EndlineStateValue;
                TransitionsTable[EndlineStateValue][ASCII_CR] = EndlineStateValue;

                AcceptingStates.Add(EndlineStateValue);
                StateToTokenID[EndlineStateValue] = ClassIdProvider.GetNext();
                EndlineValue = new Terminal(Lexer.END_LINE_TOKEN_NAME, ClassIdProvider.GetCurrent());
            }
        }

        /*
		protected virtual void InitStringIdentifierToken()
		{
			if (StringIdentifierValue == null)
			{
				StringIdentifierStateValue = CreateNewState();
				
				for (int i = ASCII_A_SM; i <= ASCII_Z_SM; i++)
				{
					TransitionsTable[INITIAL_STATE][i] = StringIdentifierStateValue;
					TransitionsTable[StringIdentifierStateValue][i] = StringIdentifierStateValue;
				}
				for (int i = ASCII_A_BG; i <= ASCII_Z_BG; i++)
				{
					TransitionsTable[INITIAL_STATE][i] = StringIdentifierStateValue;
					TransitionsTable[StringIdentifierStateValue][i] = StringIdentifierStateValue;
				}
				for (int i = ASCII_ZERO; i <= ASCII_NINE; i++)
					TransitionsTable[StringIdentifierStateValue][i] = StringIdentifierStateValue;

				TransitionsTable[INITIAL_STATE][ASCII_UNDERSCORE] = StringIdentifierStateValue;
				TransitionsTable[StringIdentifierStateValue][ASCII_UNDERSCORE] = StringIdentifierStateValue;

				AcceptingStates.Add(StringIdentifierStateValue);
				StateToTokenID[StringIdentifierStateValue] = ClassIdProvider.GetNext();
				StringIdentifierValue = new Terminal(Lexer.STRING_IDENTIFIER_TOKEN_NAME, ClassIdProvider.GetCurrent());
			}
		}
        */

        private int MatchToken(byte[] buffer, int offset, int length, out int tokenClass)
        {
            int state = INITIAL_STATE;
            int i;
            tokenClass = -1;

            for (i = offset; i < length; i++)
            {
                int previousState = state;
                state = TransitionsTable[state][buffer[i]];

                if (state == WRONG_STATE)
                {
                    if (AcceptingStates.Contains(previousState))
                    {
                        tokenClass = StateToTokenID[previousState];
                        break;
                    }
                    throw new UnexpectedTokenException(new Token(buffer, offset, i - offset));
                }
            }

            // in case end of buffer was reached, checking whether it ends with a valid token
            if (i == length && AcceptingStates.Contains(state))
                tokenClass = StateToTokenID[state];

            return i - offset; // returns length of token
        }

        protected int CreateNewState()
        {
            var arr = new int[CHARS_COUNT];
            for (int b = 0; b < CHARS_COUNT; b++)
                arr[b] = WRONG_STATE;
            TransitionsTable.Add(arr);

            return StateIdProvider.GetNext();
        }

        protected int CreateAndInitNewState(int curState, int characterCode)
        {
            // this operation is dangerous to write in one line, because CreateNewState() changes the size of the table
            int newState = CreateNewState();
            TransitionsTable[curState][characterCode] = newState;

            return newState;
        }

        protected int CheckCreateState(int characterCode, int curState)
        {
            // automatically checks whether the state already exists, that accepts transition from curState by characterCode.
            int toState = TransitionsTable[curState][characterCode];
            if (toState != WRONG_STATE)
                return toState;

            return CreateAndInitNewState(curState, characterCode);
        }
    }
}