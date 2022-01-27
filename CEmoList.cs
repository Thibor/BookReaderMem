using System;
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
		readonly static Random rnd = new Random();

		public CEmo GetRnd(int rnd = 0)
		{
			if (Count == 0)
				return null;
			if (rnd > 100)
			{
				Reverse();
				rnd = 200 - rnd;
			}
			int n = (Count * rnd) / 100;
			return this[CChess.random.Next(n)];
		}

		public void Shuffle()
		{
			int n = Count;
			while (n > 1)
			{
				int k = rnd.Next(--n + 1);
				CEmo value = this[k];
				this[k] = this[n];
				this[n] = value;
			}
		}

		public void SortMat()
		{
			Shuffle();
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
