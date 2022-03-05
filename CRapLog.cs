using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;

namespace RapLog
{
	public static class CRapLog
	{

		public static void Add(string m)
		{
			List<string> list = new List<string>();
			string name = Assembly.GetExecutingAssembly().GetName().Name;
			string path = new FileInfo(name + ".log").FullName.ToString();
			if (File.Exists(path))
				list = File.ReadAllLines(path).ToList();
			list.Insert(0, $"{DateTime.Now} {m}");
			int count = list.Count - 100;
			if (count > 0)
				list.RemoveRange(100, count);
			File.WriteAllLines(path, list);
		}

	}
}
