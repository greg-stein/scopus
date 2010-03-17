using System.Collections.Generic;

namespace ParserEngine.LexicalAnalysis.RegExp
{
    public abstract class RegExp
    {
        public RegExp()
        {
            IsNullableCalculated = false;
            IsFirstPosCalculated = false;
            IsLastPosCalculated = false;
        }

        public static RegExp Sequence(params RegExp[] regExps)
        {
            return new SequenceRegExp(regExps);
        }

        public static RegExp Choice(params RegExp[] regExps)
        {
            return new AlternationRegExp(regExps);
        }

        public static RegExp Optional(RegExp regExp)
        {
            return new OptionalRegExp(regExp);
        }

        public static RegExp AnyNumberOf(RegExp regExp)
        {
            return new RepetitionRegExp(regExp);
        }

        public static RegExp AtLeastOneOf(RegExp regExp)
        {
            return new RepetitionRegExp(regExp, true);
        }

        public static RegExp Term(char terminal)
        {
            return new LiteralRegExp(terminal);
        }

        protected abstract RegExp[] SubExpressions { get; }
        internal abstract bool CalculateNullable();
        internal abstract HashSet<int> CalculateFirstPos();
        internal abstract HashSet<int> CalculateLastPos();
        
        protected bool IsNullableCalculated { get; set; }
        protected bool IsFirstPosCalculated { get; set; }
        protected bool IsLastPosCalculated { get; set; }

        private bool nullable;
        private HashSet<int> firstPos;
        private HashSet<int> lastPos;

        internal bool Nullable
        {
            get
            {
                if (!IsNullableCalculated)
                {
                    nullable = CalculateNullable();
                }

                return nullable;
            }
        }

        internal HashSet<int> FirstPos
        {
            get
            {
                if (!IsFirstPosCalculated)
                {
                    firstPos = CalculateFirstPos();
                }

                return firstPos;
            }
        }

        internal HashSet<int> LastPos
        {
            get
            {
                if (!IsLastPosCalculated)
                {
                    lastPos = CalculateLastPos();
                }

                return lastPos;
            }
        }
    }
}