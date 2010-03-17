using System;
using System.Collections.Generic;

namespace ParserEngine.LexicalAnalysis.RegExp
{
    internal class SequenceRegExp : RegExp
    {
        internal RegExp RegExp1 { get; set; }
        internal RegExp RegExp2 { get; set; }

        internal SequenceRegExp(RegExp regExp1, RegExp regExp2)
        {
            RegExp1 = regExp1;
            RegExp2 = regExp2;
        }

        internal SequenceRegExp(params RegExp[] regExps)
        {
            if (regExps.Length < 2) throw new ArgumentException("regExps");

            RegExp1 = regExps[0];
            if (regExps.Length == 2)
            {
                RegExp2 = regExps[1];
            }
            else
            {
                RegExp2 = new SequenceRegExp(regExps[1], regExps[2]);
                for (int i = 3; i < regExps.Length; i++)
                {
                    RegExp2 = new SequenceRegExp(RegExp2, regExps[i]);
                }
            }
        }

        protected override RegExp[] SubExpressions
        {
            get { return new[] {RegExp1, RegExp2}; }
        }

        internal override bool CalculateNullable()
        {
            return RegExp1.Nullable && RegExp2.Nullable;
        }

        internal override HashSet<int> CalculateFirstPos()
        {
            if (RegExp1.Nullable)
            {
                var firstPos = new HashSet<int>(RegExp1.FirstPos);
                firstPos.UnionWith(RegExp2.FirstPos);

                return firstPos;
            }
            
            return RegExp1.FirstPos;
        }

        internal override HashSet<int> CalculateLastPos()
        {
            if (RegExp2.Nullable)
            {
                var lastPos = new HashSet<int>(RegExp2.LastPos);
                lastPos.UnionWith(RegExp1.LastPos);

                return lastPos;
            }

            return RegExp2.LastPos;
        }
    }
}
