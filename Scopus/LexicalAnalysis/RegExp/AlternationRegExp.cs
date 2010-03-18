using System;
using System.Collections.Generic;

namespace Scopus.LexicalAnalysis.RegExp
{
    internal class AlternationRegExp : RegExp
    {
        internal RegExp Alternative1 { get; set; }
        internal RegExp Alternative2 { get; set; }

        internal AlternationRegExp(RegExp alternative1, RegExp alternative2)
        {
            Alternative1 = alternative1;
            Alternative2 = alternative2;
        }

        internal AlternationRegExp(params RegExp[] alternatives)
        {
            if (alternatives.Length < 2) throw new ArgumentException("alternatives");

            Alternative1 = alternatives[0];
            if (alternatives.Length == 2)
            {
                Alternative2 = alternatives[1];
            }
            else
            {
                Alternative2 = new AlternationRegExp(alternatives[1], alternatives[2]);
                for (int i = 3; i < alternatives.Length; i++)
                {
                    Alternative2 = new AlternationRegExp(Alternative2, alternatives[i]);
                }
            }
        }

        protected override RegExp[] SubExpressions
        {
            get { return new[] {Alternative1, Alternative2}; }
        }

        internal override bool CalculateNullable()
        {
            return (Alternative1.Nullable || Alternative2.Nullable);
        }

        internal override HashSet<int> CalculateFirstPos()
        {
            var firstPos = new HashSet<int>(Alternative1.FirstPos);
            firstPos.UnionWith(Alternative2.FirstPos);

            return firstPos;
        }

        internal override HashSet<int> CalculateLastPos()
        {
            var lastPos = new HashSet<int>(Alternative1.LastPos);
            lastPos.UnionWith(Alternative2.LastPos);

            return lastPos;
        }
    }
}
