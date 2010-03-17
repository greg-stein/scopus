using System.Collections.Generic;
using ParserEngine.Auxiliary;

namespace ParserEngine.LexicalAnalysis.RegExp
{
    internal class LiteralRegExp : RegExp
    {
        // Is common for all literal expressions (leaves)
        private static IDProvider positionProvider = new IDProvider();

        internal char Terminal { get; private set; }
        internal int Position { get; private set; }

        static LiteralRegExp()
        {
            positionProvider.GetNext(); // start assigning positions from 1
        }

        internal LiteralRegExp(char terminal)
        {
            Terminal = terminal;
            Position = positionProvider.GetNext();
        }

        protected override RegExp[] SubExpressions
        {
            get { return new RegExp[0];}
        }

        internal override bool CalculateNullable()
        {
            return false;
        }

        internal override HashSet<int> CalculateFirstPos()
        {
            var firstPos = new HashSet<int> {this.Position};

            return firstPos;
        }

        internal override HashSet<int> CalculateLastPos()
        {
            var lastPos = new HashSet<int> { this.Position };

            return lastPos;
        }
    }
}
