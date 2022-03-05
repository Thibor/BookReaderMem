﻿using System;
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

		public CEmo GetEmo(int emo)
		{
			foreach (CEmo e in this)
				if (e.emo == emo)
					return e;
			return null;
		}

		public CEmo GetRnd(int rnd = 0)
		{
			if (Count == 0)
				return null;
			if (rnd < 0)
				rnd = 0;
			if (rnd > 100)
			{
				Reverse();
				rnd = 200 - rnd;
			}
			CEmo bst = this[0];
			double bd = -200.0;
			double h = rnd / 100.0;
			foreach(CEmo e in this)
			{
				double cd = (e.mat + 128.0) * (1.0 - CChess.random.NextDouble() * h);
				if (bd < cd)
				{
					bd = cd;
					bst = e;
				}
			}
			return bst;
		}

		public void Shuffle()
		{
			int n = Count;
			while (n > 1)
			{
				int k = rnd.Next(n--);
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
