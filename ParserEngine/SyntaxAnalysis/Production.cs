using System;
using System.Collections.Generic;
using System.Linq;


namespace ParserEngine.SyntaxAnalysis
{
    public class Production : GrammarEntity, IEquatable<Production>
    {
		internal const string ARROW = "-->";

		internal int ID { get; set; }
		
		internal NonTerminal Symbol { get; private set; }
		internal List<GrammarEntity> Expression = new List<GrammarEntity>();
		internal Action<TerminalValues> SemanticAction;

		private bool mExpressionChanged = true;
		private int mTerminalsCount;
        internal int TerminalsCount
        {
            get
            {
                if (mExpressionChanged)
                {
					mTerminalsCount = Expression.Where(entity => entity is Terminal).Count();
                    mExpressionChanged = false;
                }

                return mTerminalsCount;
            }
        }


		public Production(NonTerminal symbol, GrammarEntity expression, Action<TerminalValues> action) : base(symbol.ToString())
		{
			Symbol = symbol;
			Expression = new List<GrammarEntity> { expression };
			SemanticAction = action;
		}

        public override bool Equals(object obj)
        {
            if (obj == null) return base.Equals(obj);

            if (obj is Production)
                return Equals(obj as Production);

            return false;
        }

        public bool Equals(Production other)
        {
            if (other.Symbol != Symbol) return false;
            if (other.Expression.Count != Expression.Count) return false;
            for (int i = 0; i < Expression.Count; i++)
                // !!! implement correctly throughout the grammar entities
				if (!other.Expression[i].Equals(Expression[i]))
					return false;

            return true;
        }

        public override int GetHashCode()
        {
            return Symbol.GetHashCode() ^ Expression.GetHashCode();
        }

        public override string ToString()
        {
            var value = Symbol + " " + ARROW;

            foreach (var entity in Expression)
            {
                value += string.Format(" {0}", entity);
            }

            return value;
        }

        public static Production operator &(Production production, GrammarEntity entity)
        {
			production.mExpressionChanged = true;
			production.Expression.Add(entity);
        	return production;
        }
		public static Production operator ^(Production p, Action<TerminalValues> action)
		{
			p.SemanticAction = action;
			return p;
		}

		public void PerformSemanticAction(TerminalValues values)
		{
			if (SemanticAction != null)
				SemanticAction(values);
		}
	}
}
