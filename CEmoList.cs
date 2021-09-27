using System.Collections.Generic;
using NSChess;

namespace NSProgram
{
	class CEmo
	{
		public int emo;
		public sbyte mat;
		public byte age;
	}

	class CEmoList : List<CEmo>
	{
		public CEmo GetRnd(int rnd = 0)
		{
			if (Count == 0)
				return null;
			CEmo emo = this[0];
			if ((Count == 1) || (rnd == 0))
				return emo;
			for (int n = 1; n < Count; n++)
			{
				CEmo e = this[n];
				if (emo.mat - e.mat >= rnd)
					return this[CChess.random.Next(n)];
			}
			return this[CChess.random.Next(Count)];
		}

		public void SortMat()
		{
			Sort(delegate (CEmo e1, CEmo e2)
			{
				int r = e2.mat - e1.mat;
				if (r != 0)
					return r;
				return e1.age - e2.age;
			});
		}
	}
}
