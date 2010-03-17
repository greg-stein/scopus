using ParserEngine.SyntaxAnalysis;


namespace ParserEngine.LexicalAnalysis
{
	sealed class CADTokenizer : KeywordsTokenizer
	{
		public CADTokenizer()
		{
			InitIntegerNumberToken();
			InitFloatNumberToken();
			InitEndlineToken();
			InitStringIdentifierToken();

			HideToken(AddToken(ASCII_SPACE.ToString()));
			HideToken(AddToken(ASCII_TAB.ToString()));
		}
		
		protected override void InitIntegerNumberToken()
		{
			if (IntegerNumberValue == null)
			{
				var signedIntegerState = CheckCreateState(ASCII_PLUS, INITIAL_STATE);
				TransitionsTable[INITIAL_STATE][ASCII_MINUS] = signedIntegerState;

				IntegerNumberStateValue = CheckCreateState(ASCII_ZERO, INITIAL_STATE);
				for (int i = ASCII_ZERO; i <= ASCII_NINE; i++)
				{
					TransitionsTable[INITIAL_STATE][i] = IntegerNumberStateValue;
					TransitionsTable[signedIntegerState][i] = IntegerNumberStateValue;
					TransitionsTable[IntegerNumberStateValue][i] = IntegerNumberStateValue;
				}

				AcceptingStates.Add(IntegerNumberStateValue);
				StateToTokenID[IntegerNumberStateValue] = ClassIdProvider.GetNext();
				IntegerNumberValue = new Terminal(Lexer.INTEGER_NUMBER_TOKEN_NAME, ClassIdProvider.GetCurrent());
			}
		}

		protected override void InitStringIdentifierToken()
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
				TransitionsTable[StringIdentifierStateValue][ASCII_PLUS] = StringIdentifierStateValue;
				TransitionsTable[StringIdentifierStateValue][ASCII_MINUS] = StringIdentifierStateValue;
				TransitionsTable[StringIdentifierStateValue][ASCII_DOT] = StringIdentifierStateValue;

				AcceptingStates.Add(StringIdentifierStateValue);
				StateToTokenID[StringIdentifierStateValue] = ClassIdProvider.GetNext();
				StringIdentifierValue = new Terminal(Lexer.STRING_IDENTIFIER_TOKEN_NAME, ClassIdProvider.GetCurrent());
			}
		}
	}
}
