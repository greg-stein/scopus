using System;
using System.Text;
using Scopus.Auxiliary;

namespace Scopus.LexicalAnalysis.RegularExpressions
{
    /// <summary>
    /// Represents literal regular expression for single character.
    /// </summary>
    internal class LiteralRegExp : RegExp
    {
        // TODO: Check if it is possible to move the Encoding - related logic to Lexer and leave RegExps dealing with bytes only.
        internal LiteralRegExp(byte literal)
        {
            Literals = new[] {literal};
        }

        internal LiteralRegExp(params byte[] literals)
        {
            Literals = (byte[]) literals.Clone();
        }

        internal LiteralRegExp(char literal)
        {
            Literals = Encoding.GetBytes(new[] {literal});
        }

        internal LiteralRegExp(string literals)
        {
            Literals = Encoding.GetBytes(literals);
        }

        internal LiteralRegExp(char literal, Encoding encoding)
        {
            Encoding = encoding;
            Literals = Encoding.GetBytes(new[] { literal });
        }

        internal LiteralRegExp(string literals, Encoding encoding)
        {
            Encoding = encoding;
            Literals = Encoding.GetBytes(literals);
        }

        private byte[] Literals { get; set; }

        protected internal override Encoding Encoding
        {
            get
            {
                return base.Encoding;
            }
            set
            {
                if (Literals != null)
                {
                    var literalsStr = Encoding.GetString(Literals); // Decode using old encoding
                    base.Encoding = value;
                    Literals = Encoding.GetBytes(literalsStr); // Encode using new encoding
                }
                else
                {
                    base.Encoding = value;
                }
            }
        }

        internal override FiniteAutomata AsNFA()
        {
            var nfa = new FiniteAutomata("LiteralRegExpNFA");
            State state = nfa.StartState;
            int i;

            for (i = 0; i < Literals.Length - 1; i++)
            {
                var newState = new State(Literals[i].ToString());
                state.AddTransitionTo(newState, InputChar.For(Literals[i]));
                state = newState;
            }

            state.AddTransitionTo(nfa.Terminator, InputChar.For(Literals[i])); // the last transition

            return nfa;
        }

        public override string ToString()
        {
            return Encoding.GetString(Literals);
        }

        public bool Equals(LiteralRegExp literalRegExp)
        {
            if (!base.Equals(literalRegExp)) return false;

            if (ByteArrayRoutines.AreEqual(literalRegExp.Literals, Literals))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return base.Equals(obj);

            var literalRegExp = obj as LiteralRegExp;
            if (literalRegExp != null)
                return Equals(literalRegExp);

            return false;
        }

     }
}