using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSProgram
{
	class CRec
	{
		public ulong key;
		public sbyte mat;
		public byte mem;

		public CRec()
		{
			key = 0;
			mat = 0;
			mem = 0xff;
		}
	}

	class CRecList : List<CRec>
	{

		public void AddRec(CRec rec)
		{
			int index = FindHash(rec.key);
			if (index == Count)
				Add(rec);
			else
			{
				CRec r = this[index];
				if (r.key == rec.key)
				{
					if (r.mat > rec.mat)
						r.mat--;
					if (r.mat < rec.mat)
						r.mat++;
					r.mem = 0xff;
				}
				else
					Insert(index, rec);
			}
		}

		public bool RecUpdate(CRec rec)
		{
			int index = FindHash(rec.key);
			if(index < Count)
			{
				CRec r = this[index];
				if (r.key == rec.key)
				{
					if (r.mat > rec.mat)
						r.mat--;
					if (r.mat < rec.mat)
						r.mat++;
					return true;
				}
			}
			return false;
		}

		public int FindHash(ulong hash)
		{
			int first = -1;
			int last = Count;
			while (true)
			{
				if (last - first == 1)
					return last;
				int middle = (first + last) >> 1;
				CRec rec = this[middle];
				if (hash < rec.key)
					last = middle;
				else if (hash > rec.key)
					first = middle;
				else
					return middle;
			}
		}

		public CRec GetRec(ulong hash)
		{
			int index = FindHash(hash);
			if (index < Count)
				if (this[index].key == hash)
					return this[index];
			return null;
		}


	}
}
