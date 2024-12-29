using System;
using System.Collections.Generic;

namespace NSProgram
{
    class CRec
    {
        public byte win = 0;
        public byte lost = 0;
        public string tnt = String.Empty;

        public CRec(string tnt)
        {
            this.tnt = tnt;
        }

        public int Games()
        {
            return win + lost + 1;
        }

        public int Value()
        {
            return (win * 1000) / Games();
        }

    }

    class CRecList : List<CRec>
    {
        readonly static Random rnd = new Random();

        public bool ReplaceRec(CRec rec)
        {
            int index = FindTnt(rec.tnt);
            if (index == Count)
                Add(rec);
            else
            {
                CRec r = this[index];
                if (r.tnt == rec.tnt)
                {
                    r.win = rec.win;
                    r.lost = rec.lost;
                    return false;
                }
                else
                    Insert(index, rec);
            }
            return true;
        }

        public bool AddRec(CRec rec)
        {
            int index = FindTnt(rec.tnt);
            if (index == Count)
                Add(rec);
            else
            {
                CRec r = this[index];
                if (r.tnt == rec.tnt)
                {
                    r.win += rec.win;
                    r.lost += rec.lost;
                    if ((r.win == 0xff) || (r.lost == 0xff))
                    {
                        r.win >>= 1;
                        r.lost >>= 1;
                    }
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
                SortValue();
                RemoveRange(Count - count, count);
                SortTnt();
            }
            return c - Count;
        }

        public int FindTnt(string tnt)
        {
            int first = -1;
            int last = Count;
            while (true)
            {
                if (last - first == 1)
                    return last;
                int middle = (first + last) >> 1;
                CRec rec = this[middle];
                int c = String.Compare(tnt, rec.tnt, StringComparison.Ordinal);
                if (c <= 0)
                    last = middle;
                else
                    first = middle;
            }
        }

        public CRec GetRec()
        {
            int index = rnd.Next(Count);
            if (index < Count)
                return this[index];
            return null;
        }

        public CRec GetRec(string tnt)
        {
            int index = FindTnt(tnt);
            if (index < Count)
                if (this[index].tnt == tnt)
                    return this[index];
            return null;
        }

        public void DelTnt(string tnt)
        {
            if (IsTnt(tnt, out int index))
                RemoveAt(index);
        }

        public bool IsTnt(string tnt, out int index)
        {
            index = FindTnt(tnt);
            if (index < Count)
                return this[index].tnt == tnt;
            return false;
        }

        public void SortRnd()
        {
            int n = Count;
            while (n > 1)
            {
                int k = rnd.Next(n--);
                (this[n], this[k]) = (this[k], this[n]);
            }
        }

        public void SortTnt()
        {
            Sort(delegate (CRec r1, CRec r2)
            {
                return String.Compare(r1.tnt, r2.tnt, StringComparison.Ordinal);
            });
        }

        public void SortValue()
        {
            Sort(delegate (CRec r1, CRec r2)
            {
                return r2.Value() - r1.Value();
            });
        }

    }
}
