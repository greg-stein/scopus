namespace Scopus.SyntaxAnalysis
{
    public abstract class GrammarEntity
    {
        private readonly string mName;

		protected GrammarEntity(string name)
        {
            mName = name;
        }

        public override string ToString()
        {
            return mName;
        }

		// IMPLEMENTATION NOTICE:
		//
		//	The order of overloaded operators is this:
		//	1) --	(production part 1)
		//	2) >	(production part 2)
		//	3) &	(expression symbol concatenation)
		//	4) ^	(semantic action binding)
		// This guarantees that first of all, a new instance of production will be created,
		// (with initially leftmost symbol in expression, as the first element in Expression list),
		// and after that, additional symbols (if any) will be concatenated to this production object.
		// Last turn, the semantic action is binded to production object.
    }
}
