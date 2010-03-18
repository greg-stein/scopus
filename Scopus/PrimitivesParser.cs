using System.Text;
using Scopus.LexicalAnalysis;

namespace Scopus
{
    public static class PrimitivesParser
    {
        public static Encoding Encoding { get; set; }

        static PrimitivesParser()
        {
            Encoding = Encoding.ASCII;
        }

        // TODO: ParseDouble, ParseFloat, ParseBool, ParseChar, ParseByte

        public static string ParseString(Token token)
        {
            return Encoding.GetString(token.Buffer, token.Offset, token.Length);
        }

        public static int ParseInt(Token token)
        {
			int result = 0;
			int initialOffset = token.Offset;
			int signMultiplier = 1;
			
			if (token.Buffer[initialOffset] == '-')
			{
				initialOffset++;
				signMultiplier = -1;
			}
			else if (token.Buffer[initialOffset] == '+')
			{
				initialOffset++;
			}

			for (int i = initialOffset; i < token.Offset + token.Length; i++)
            {
				if (token.Buffer[i] == '.')
					break;
                result *= 10;
                result += (token.Buffer[i] & 0x0F);
            }

			return result * signMultiplier;
        }

		public static float ParseFloat(Token token)
		{
			// !!! todo: revise - maybe more optimal by perfomance
			float result = 0f;
			int initialOffset = token.Offset;
			int signMultiplier = 1;
			bool floatPointEncountered = false;

			if (token.Buffer[initialOffset] == '-')
			{
				initialOffset++;
				signMultiplier = -1;
			}
			else if (token.Buffer[initialOffset] == '+')
			{
				initialOffset++;
			}

			int intCounter;
			for (intCounter = initialOffset; intCounter < token.Offset + token.Length; intCounter++)
			{
				if (token.Buffer[intCounter] == '.')
				{
					floatPointEncountered = true;
					break;
				}
				result *= 10;
				result += (token.Buffer[intCounter] & 0x0F);
			}

			if (floatPointEncountered)
			{
				float frac = 0f;
				int mag = 1;
				for (int floatCounter = intCounter + 1; floatCounter < token.Offset + token.Length; floatCounter++)
				{
					mag *= 10;
					frac += (float)(token.Buffer[floatCounter] & 0x0F) / mag;
				}
				result += frac;
			}

			return result * signMultiplier;
		}
	}
}