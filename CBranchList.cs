using System;
using System.Collections.Generic;
using System.Linq;

namespace NSProgram
{
	class CBranch
	{
		public int index = 0;
		public CEmoList emoList = new CEmoList();

		public void Fill()
		{
			index = 0;
			emoList = Program.book.GetEmoList();
			emoList.Shuffle();
		}

		public CEmo GetEmo()
		{
			if ((index >= 0) && (index < emoList.Count))
				return emoList[index];
			return null;
		}

		public double GetBit()
		{
			return 1.0 / emoList.Count;
		}

		public double GetProcent()
		{
			return (index * 1.0) / emoList.Count;
		}

	}

	internal class CBranchList : List<CBranch>
	{
		public void Start()
		{
			Clear();
			BlFill();
		}

		public void BlFill()
		{
			CBranch branch = new CBranch();
			branch.Fill();
			CEmo emo = branch.GetEmo();
			if (emo != null)
			{
				Add(branch);
				Program.book.chess.MakeMove(emo.emo);
				BlFill();
			}
		}

		public bool BlNext()
		{
			if (Count == 0)
				return false;
			CBranch lastBranch = this.Last();
			CEmo lastEmo = lastBranch.GetEmo();
			lastBranch.index++;
			CEmo newEmo = lastBranch.GetEmo();
			if (lastEmo != null)
				Program.book.chess.UnmakeMove(lastEmo.emo);
			if (newEmo != null)
			{
				Program.book.chess.MakeMove(newEmo.emo);
				return true;
			}
			else if (Count > 1)
			{
				RemoveAt(Count - 1);
				return BlNext();
			}
			return false;
		}

		public double GetProcent()
		{
			double b1 = Count > 0 ? this[0].GetBit() : 1.0;
			double b2 = Count > 1 ? this[1].GetBit() * b1 : b1;
			double b3 = Count > 1 ? this[2].GetBit() * b2 : b2;
			double p1 = Count > 0 ? this[0].GetProcent() : 1.0;
			double p2 = Count > 1 ? this[1].GetProcent() * b1 : b1;
			double p3 = Count > 2 ? this[2].GetProcent() * b2 : b2;
			return (p1 + p2 + p3 + b3) * 100.0;
		}

		public string GetUci()
		{
			string uci = String.Empty;
			foreach (CBranch branch in this)
			{
				CEmo emo = branch.GetEmo();
				if (emo != null)
				{
					string umo = Program.book.chess.EmoToUmo(emo.emo);
					uci = $"{uci} {umo}";
				}
			}
			return uci.Trim();
		}

		public void SetUsed(bool used = true)
		{
			foreach (CBranch b in this)
				b.emoList.SetUsed(used);
		}


	}

}
