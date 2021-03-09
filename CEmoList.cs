using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSChess;

namespace NSProgram
{
	class CEmo
	{
		public int emo;
		public sbyte mat;
		public byte age;
	}

	class CEmoList:List<CEmo>
	{
		public CEmo GetRnd(int rnd)
		{
			if (Count == 0)
				return null;
			CEmo emo = this[0];
			if (Count == 1)
				return emo;
			for(int n = 1; n < Count; n++)
			{
				CEmo e = this[n];
				if (emo.mat - e.mat > rnd)
					return this[CChess.random.Next(n)];
			}
			return this[CChess.random.Next(Count)];
		}

		public void SortMat()
		{
			Sort(delegate (CEmo e1, CEmo e2)
			{
				return e2.mat - e1.mat;
			});
		}
	}
}
