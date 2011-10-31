using System;
using System.Text;

namespace Scopus.LexicalAnalysis.RegularExpressions
{
    internal abstract class RegExpParser
    {
        internal static RegExpParser GetParser(RegExpNotation notation)
        {
            switch (notation)
            {
                case RegExpNotation.POSIXNotation:
                    return new POSIXRegExpParser(Encoding.ASCII);
                case RegExpNotation.MicrosoftNotation:
                    break;
            }

            throw new InvalidOperationException("Not supported regular expression noation!");
        }

        internal abstract RegExp Parse(string regexp);
    }
}
