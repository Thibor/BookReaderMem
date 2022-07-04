﻿using System;
using System.Collections.Generic;

namespace NSProgram
{
	class CRec
	{
		public bool used = false;
		public ulong hash = 0;
		public sbyte mat = 0;
		public byte age = 0;

		public double GetValue()
		{
			return mat == 0 ? 0 : 1.0 / mat;
		}
	}

	class CRecList : List<CRec>
	{
		readonly static Random rnd = new Random();

		public bool AddRec(CRec rec)
		{
			int index = FindHash(rec.hash);
			if (index == Count)
				Add(rec);
			else
			{
				CRec r = this[index];
				if (r.hash == rec.hash)
				{
					r.age = rec.age;
					r.mat = rec.mat;
					return false;
				}
				else
					Insert(index, rec);
			}
			return true;
		}

		public int RecDelete(int count)
		{
			if (count <= 0)
				return 0;
			int c = Count;
			if (count >= Count)
				Clear();
			else
			{
				Shuffle();
				SortAge();
				RemoveRange(Count - count, count);
				SortHash();
			}
			return c - Count;
		}

		public int DeleteNotUsed()
		{
			int del = 0;
			Shuffle();
			SortAge();
			for(int n = Count -1;n >= 0; n--)
			{
				CRec rec = this[n];
				if (rec.age < 0xff)
					break;
				if (rec.used)
					continue;
				RemoveAt(n);
				del++;
			}
			SortHash();
			return del;
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
				if (hash < rec.hash)
					last = middle;
				else if (hash > rec.hash)
					first = middle;
				else
					return middle;
			}
		}

		public CRec GetRec(ulong hash)
		{
			int index = FindHash(hash);
			if (index < Count)
				if (this[index].hash == hash)
					return this[index];
			return null;
		}

		public void DelHash(ulong hash)
		{
			if (IsHash(hash, out int index))
				RemoveAt(index);
		}

		public bool IsHash(ulong hash,out int index)
		{
			index = FindHash(hash);
			if (index < Count)
				return this[index].hash == hash;
			return false;
		}

		public void SetUsed(bool u)
		{
			foreach (CRec rec in this)
				rec.used = u;
		}

		public int GetUsed()
		{
			int used = 0;
			foreach (CRec rec in this)
				if (rec.used)
					used++;
			return used;
		}

		public void Shuffle()
		{
			int n = Count;
			while (n > 1)
			{
				int k = rnd.Next(n--);
				CRec value = this[k];
				this[k] = this[n];
				this[n] = value;
			}
		}

		public void SortHash()
		{
			Sort(delegate (CRec r1, CRec r2)
			{
				if (r1.hash > r2.hash)
					return 1;
				if (r1.hash < r2.hash)
					return -1;
				return 0;
			});
		}

		public void SortAge()
		{
			Sort(delegate (CRec r1, CRec r2)
			{
				return r1.age - r2.age;
			});
		}


	}
}
