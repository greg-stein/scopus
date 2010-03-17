using System;
using System.Collections.Generic;
using ParserEngine.Exceptions;
using ParserEngine.LexicalAnalysis;
using ParserEngine.SyntaxAnalysis.ParsingTables;

namespace ParserEngine.SyntaxAnalysis
{
    public class LRParser : IParser
    {
        public ParsingTable ParsingTable { get; set; }
        public Grammar Grammar { get; set; }
        public ILexer Lexer { get; set; }

		public event SyntaxErrorEventHandler SyntaxError;
        public event EventHandler InputAccepted;

        private readonly TerminalValues mValues = new TerminalValues();
        private readonly Stack<int> mStatesStack = new Stack<int>();

        public LRParser()
        {
            SyntaxError += DefaultErrorRecoveryRoutine;
        }

        public void ParseInput()
        {
            mStatesStack.Push(0);

            var tokenEnumerator = Lexer.TokensStream.GetEnumerator();
            tokenEnumerator.MoveNext();

            while(true)
            {
                var s = mStatesStack.Peek();
                var actionEntry = ParsingTable.ActionTable[s, tokenEnumerator.Current.Class];

				// todo: TRACE option
				//Trace(actionEntry, tokenEnumerator.Current);

				if (actionEntry.Action == ParserAction.Shift)
                {
                    mStatesStack.Push(actionEntry.Destination);
                    mValues.PushToken((Token)tokenEnumerator.Current.Clone());

					if (!tokenEnumerator.MoveNext())
                        OnSyntaxError(new ParserContext(mStatesStack, Lexer, tokenEnumerator.Current));
                }
                else if (actionEntry.Action == ParserAction.Reduce)
                {
                    var production = Grammar.Productions[actionEntry.Destination];
                    for (int i = 0; i < production.Expression.Count; i++)
                        mStatesStack.Pop();

                    var state = mStatesStack.Peek();
					mStatesStack.Push(ParsingTable.GotoTable[state, production.Symbol.ID]);
                    mValues.SetPopableTokensCount(production.TerminalsCount);
                    production.PerformSemanticAction(mValues);
					mValues.RemoveUnusedTokens();
                }
                else if (actionEntry.Action == ParserAction.Accept)
                {
                    OnInputAccepted();
                    break;
                }
                else
                {
                    OnSyntaxError(new ParserContext(mStatesStack, Lexer, tokenEnumerator.Current));
                }
            }
        }

        private void Trace(ActionTableEntry action, Token current)
        {
            var actionStr = string.Empty;

            switch (action.Action)
            {
                case ParserAction.Reduce:
                    actionStr = "reduce by " + Grammar.Productions[action.Destination];
                    break;
                case ParserAction.Shift:
                    actionStr = "shift " + action.Destination;
                    break;
                case ParserAction.Error:
                    actionStr = "error";
                    break;
                case ParserAction.Accept:
                    actionStr = "accept";
                    break;
            }

            string stack = string.Empty;
            Array.ForEach(mStatesStack.ToArray(), i => stack = i + " " + stack);

            Console.WriteLine("{0,10}|{1,5}|{2}", stack, current, actionStr);
        }

        private void DefaultErrorRecoveryRoutine(IParser parser, ParserContext context)
        {
            throw new SyntaxErrorException(context);
        }

        private void OnSyntaxError(ParserContext context)
        {
            if (SyntaxError != null)
                SyntaxError(this, context);
        }

        private void OnInputAccepted()
        {
            if (InputAccepted != null)
                InputAccepted(this, new EventArgs());
        }
    }
}
