using System;
using System.Collections.Generic;
using NSChess;

namespace NSProgram
{
	class CEmo
	{
		public int emo = 0;
		public CRec rec = null;

		public CEmo(int e)
		{
			emo = e;
		}

		public CEmo(int e, CRec r)
		{
			emo = e;
			rec = r;
		}

	}

	class CEmoList : List<CEmo>
	{
		public static Random rnd = new Random();

		public CEmo GetEmo(int emo)
		{
			foreach (CEmo e in this)
				if (e.emo == emo)
					return e;
			return null;
		}

		CEmo GetRnd100()
		{
			if (Count == 0)
				return null;
			int bst = 0;
			int total = 0;
			for(int n=0;n<Count;n++)
			{
				int games = this[n].rec.games;
				total += games;
				if (rnd.Next(total) < games)
					bst = n;
			}
			return this[bst];
		}

		public CEmo GetRnd(int random = 0)
		{
			if (Count == 0)
				return null;
			if (random < 0)
				random = 0;
			int i1 = 0;
			int i2 = Count;
			if (random == 100)
				return GetRnd100();
			else if (random < 100)
				i2 = (Count * random) / 100;
			else
				i1 = ((Count - 1) * (random - 100)) / 100;
			return this[CChess.rnd.Next(i1, i2)];
		}

		public void SortGames()
		{
			Sort(delegate (CEmo e1, CEmo e2)
			{
				return e2.rec.games - e1.rec.games;
			});
		}

	}
}
