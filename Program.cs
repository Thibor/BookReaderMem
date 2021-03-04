using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using NSUci;
using NSChess;

namespace NSProgram
{
	class Program
	{
		static void Main(string[] args)
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
			CUci Uci = new CUci();
			CBookMem book = new CBookMem();
			CChess chess = CBookMem.chess;
			string ax = "-bn";
			List<string> listBn = new List<string>();
			List<string> listEf = new List<string>();
			List<string> listEa = new List<string>();
			for (int n = 0; n < args.Length; n++)
			{
				string ac = args[n];
				switch (ac)
				{
					case "-bn":
					case "-ef":
					case "-ea":
						ax = ac;
						break;
					default:
						switch (ax)
						{
							case "-bn":
								listBn.Add(ac);
								break;
							case "-ef":
								listEf.Add(ac);
								break;
							case "-ea":
								listEa.Add(ac);
								break;
						}
						break;
				}
			}
			string bookName = String.Join(" ", listBn);
			string engineName = String.Join(" ", listEf);
			string arguments = String.Join(" ", listEa);
			Process myProcess = new Process();
			if (File.Exists(engineName))
			{
				myProcess.StartInfo.FileName = engineName;
				myProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(engineName);
				myProcess.StartInfo.UseShellExecute = false;
				myProcess.StartInfo.RedirectStandardInput = true;
				myProcess.StartInfo.Arguments = arguments;
				myProcess.Start();
			}
			else
			{
				if (engineName != "")
					Console.WriteLine("info string missing engine");
				engineName = "";
			}

			if (!book.LoadFromFile(bookName))
				if (!book.LoadFromFile($"{bookName}{CBookMem.defExt}"))
							Console.WriteLine($"info string missing book {bookName}");
			while (true)
			{
				string msg = Console.ReadLine();
				Uci.SetMsg(msg);
				if (Uci.command == "book")
				{
					switch (Uci.tokens[1])
					{
						case "clear":
							book.Clear();
							break;
						case "info":
							book.Info();
							break;
						case "add":
							if(!book.FileAdd(Uci.GetValue(2, 0)))
								Console.WriteLine("File not found");
							break;
						case "save":
							book.SaveToFile(Uci.GetValue(2, 0));
							break;
					}
					continue;
				}
				if ((Uci.command != "go") && (engineName != ""))
					myProcess.StandardInput.WriteLine(msg);
				switch (Uci.command)
				{
					case "position":
						List<string> movesUci = new List<string>();
						string fen = Uci.GetValue("fen","moves");
						chess.SetFen(fen);
						int lo = Uci.GetIndex("moves", 0);
						if (lo++ > 0)
						{
							int hi = Uci.GetIndex("fen", Uci.tokens.Length);
							if (hi < lo)
								hi = Uci.tokens.Length;
							for (int n = lo; n < hi; n++)
							{
								string m = Uci.tokens[n];
								movesUci.Add(m);
								chess.MakeMove(m);
							}
						}
						if ((fen == String.Empty) && chess.Is2ToEnd(out string myMove, out string enMove))
						{
							movesUci.Add(myMove);
							movesUci.Add(enMove);
							book.Add(movesUci);
							book.SaveToFile();
						}
						break;
					case "go":
						string move = book.GetMove();
						if (move != String.Empty)
							Console.WriteLine($"bestmove {move}");
						else if (engineName == "")
							Console.WriteLine("enginemove");
						else
							myProcess.StandardInput.WriteLine(msg);
						break;
				}
			}
		}
	}
}
