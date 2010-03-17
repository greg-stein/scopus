using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace ParserEngine.SyntaxAnalysis.ParsingTables
{
	class ItemSet : IEnumerable<Item>
	{
		internal ItemSet(Grammar g, List<Item> kernel, int id)
		{
			KernelItemsCount = kernel.Count;
			ID = id;
			Closure = ParsingTable.Closure(g, kernel);
		}

		public IEnumerator<Item> GetEnumerator()
		{
			return Closure.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public override int GetHashCode()
		{
			string value = string.Empty;
			IEnumerator<Item> enu = Closure.GetEnumerator();
			for (int i = 0; i < KernelItemsCount; i++)
			{
				enu.MoveNext();
				value += enu.Current.ToString();
			}

			return value.GetHashCode();
		}
		public override bool Equals(object obj)
		{
			if (obj == null) return base.Equals(obj);

			if (obj is ItemSet)
				return Equals(obj as ItemSet);

			return false;
		}
		public bool Equals(ItemSet other)
		{
			// relying on fact that itemSet management is purely internal
			if (KernelItemsCount != other.KernelItemsCount)
				return false;

			IEnumerator<Item> enu = Closure.GetEnumerator();
			IEnumerator<Item> enuOther = other.Closure.GetEnumerator();
			for (int i = 0; i < KernelItemsCount; i++)
			{
				enu.MoveNext();
				enuOther.MoveNext();
				if (!enu.Current.Equals(enuOther.Current))
					return false;
			}

			return true;
		}
		public override string ToString()
		{
			string value = "< ||";
			IEnumerator<Item> enu = Closure.GetEnumerator();
			for (int i = 0; i < Closure.Count; i++)
			{
				enu.MoveNext();
				value += " " + enu.Current + " ||";
				if (i == KernelItemsCount-1)
					value += " >" + ((KernelItemsCount == Closure.Count) ? "" : " ||");
			}

			return value;
		}

		// !!! implement through dedicated enumerator
		internal IEnumerable<Item> GetKernel()
		{
			return Closure.Take(KernelItemsCount);
		}

		internal List<Item> Closure { get; private set; }
		//internal HashSet<Item> Closure { get; private set; }
		//internal List<Item> ClosureList { get; private set; }		// HashSet makes no guarantees to enumerate elements by order of addition
		internal int KernelItemsCount { get; private set; }
		internal int ID { get; private set; }
	}
}
