using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
		public void SortMat()
		{
			Sort(delegate (CEmo e1, CEmo e2)
			{
				return e2.mat - e1.mat;
			});
		}
	}
}
