using System;

namespace Scopus.SyntaxAnalysis.ParsingTables
{
    internal class Item : IEquatable<Item>
    {
        internal Item(Production p, int dotPosition)
        {
            Production = p;
            DotPosition = dotPosition;
        }

        internal Production Production { get; private set; }
        internal int DotPosition { get; private set; }

        internal GrammarEntity NextToDotSymbol
        {
            get
            {
                // In case the dot is at the end of production (S --> E·)
                if (DotPosition == Production.Expression.Count)
                    return null; // there is no symbol next to dot.

                return Production.Expression[DotPosition];
            }
        }

        #region IEquatable<Item> Members

        public bool Equals(Item other)
        {
            if (!other.Production.Equals(Production)) return false;
            if (other.DotPosition != DotPosition) return false;

            return true;
        }

        #endregion

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return base.Equals(obj);

            if (obj is Item)
                return Equals(obj as Item);

            return false;
        }

        public override string ToString()
        {
            string production = Production.ToString();
            // "A --> ".Length() :
            int dotIndex = Production.Symbol.ToString().Length + Production.ARROW.Length + 2; // two spaces

            for (int i = 0; i < DotPosition; i++)
            {
                GrammarEntity entity = Production.Expression[i];
                dotIndex += entity.ToString().Length + 1; // plus one space
            }

            return production.Insert(dotIndex - 1, "·");
        }
    }
}