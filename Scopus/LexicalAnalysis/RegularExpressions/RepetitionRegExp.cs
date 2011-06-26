namespace Scopus.LexicalAnalysis.RegularExpressions
{
    internal class RepetitionRegExp : RegExp
    {
        internal Greediness Greediness { get; set; }

        internal RepetitionRegExp(RegExp expression, Greediness greediness = Greediness.GreedyQuantification)
        {
            ExpressionToRepeat = expression;
            ChildExpressions = new[] {expression};
            Greediness = greediness;
        }

        internal RegExp ExpressionToRepeat { get; set; }

        internal override FiniteAutomata AsNFA()
        {
            var nfa = new FiniteAutomata("RepetitionRegExpNFA");
            FiniteAutomata innerExpNFA = ExpressionToRepeat.AsNFA();

            nfa.StartState.AddTransitionTo(innerExpNFA.StartState, InputChar.Epsilon());
            nfa.StartState.AddTransitionTo(nfa.Terminator, InputChar.Epsilon());
            innerExpNFA.Terminator.AddTransitionTo(nfa.Terminator, InputChar.Epsilon());
            innerExpNFA.Terminator.AddTransitionTo(innerExpNFA.StartState, InputChar.Epsilon());

            return nfa;
        }

        public override string ToString()
        {
            string qSign = (Greediness == Greediness.LazyQuantification) ? "?" : string.Empty;
            return "(" + ExpressionToRepeat + ")*" + qSign;
        }

        public bool Equals(RepetitionRegExp repetitionRegExp)
        {
            if (!base.Equals(repetitionRegExp)) return false;

            if (repetitionRegExp.ExpressionToRepeat.Equals(ExpressionToRepeat)) return false;
            if (repetitionRegExp.Greediness != Greediness) return false;

            return true;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return base.Equals(obj);

            var repetitionRegExp = obj as RepetitionRegExp;
            if (repetitionRegExp != null)
                return Equals(repetitionRegExp);

            return false;
        }
    }
}