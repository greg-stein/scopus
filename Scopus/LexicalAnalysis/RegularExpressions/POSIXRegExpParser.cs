using System;
using System.Collections.Generic;
using System.Text;
using Scopus.SyntaxAnalysis;

namespace Scopus.LexicalAnalysis.RegularExpressions
{
    // To be implemented using following grammars:
    // POSIX - http://www.opengroup.org/onlinepubs/009695399/basedefs/xbd_chap09.html
    // FLEX  - http://dinosaur.compilertools.net/flex/flex_7.html#SEC7

    // NOTE: Scopus itself is not used for the parsing of RegExp

    internal class POSIXRegExpParser : RegExpParser
    {
        private const char SQUARE_BRACE_OPEN = '[';
        private const char SQUARE_BRACE_CLOSE = ']';
        private const char ROUND_BRACE_OPEN = '(';
        private const char ROUND_BRACE_CLOSE = ')';
        private const char PLUS_SIGN = '+';
        private const char MINUS_SIGN = '-';
        private const char STAR_SIGN = '*';

        private Stack<char> stack = new Stack<char>();
        private Dictionary<char, Action> semanticActions = new Dictionary<char, Action>();
        private Encoding mEncoding;

        internal POSIXRegExpParser(Encoding encoding)
        {
            mEncoding = encoding;
        }

        private void ConstructParser()
        {
            semanticActions.Add(SQUARE_BRACE_CLOSE, ExtractCharClass);
        }

        internal void ExtractCharClass()
        {
            var charsList = new List<char>();
            var rangesList = new List<Tuple<char, char>>();

            var top = stack.Pop();
            while (top != SQUARE_BRACE_OPEN)
            {
                if (stack.Peek() == MINUS_SIGN)
                {
                    stack.Pop();
                    rangesList.Add(Tuple.Create(stack.Pop(), top));
                }
                else
                {
                    charsList.Add(top);
                }
                top = stack.Pop();
            }

            var regexp = RegExp.CharClass(charsList, rangesList, mEncoding);
        }

        internal override RegExp Parse(string regexp)
        {
            for (int i = 0; i < regexp.Length; i++)
            {
                Action action;
                if (semanticActions.TryGetValue(regexp[i], out action))
                {
                    action.Invoke();
                }
                else
                {
                    stack.Push(regexp[i]);
                }
            }
            return null;
        }
    }
}