using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using NSUci;

namespace NSProgram
{
	class Program
	{
		static void Main(string[] args)
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
			/// <summary>
			/// Book can write new moves.
			/// </summary>
			bool isWritable = false;
			/// <summary>
			/// Load Before Add new moves.
			/// </summary>
			bool lba = false;
			int rnd = 0;
			CUci Uci = new CUci();
			CBookMem book = new CBookMem();
			CChessExt chess = CBookMem.chess;
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
					case "-rnd":
						ax = ac;
						break;
					case "-w":
						ax = ac;
						isWritable = true;
						break;
					case "-lba":
						ax = ac;
						lba = true;
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
							case "-rnd":
								rnd = int.TryParse(ac, out int r) ? r : 0;
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
					Console.WriteLine($"info string missing engine  [{engineName}]");
				engineName = "";
			}

			if (!book.LoadFromFile(bookName))
				if (!book.LoadFromFile($"{bookName}{CBookMem.defExt}"))
					Console.WriteLine($"info string missing book [{bookName}]");
			while (true)
			{
				string msg = Console.ReadLine().Trim();
				if ((msg == "help") || (msg == "book"))
				{
					Console.WriteLine("book load [filename].[mem] - clear and add moves from file");
					Console.WriteLine("book save [filename].[mem] - save book to the file");
					Console.WriteLine("book delete [number x] - delete x number of moves from the book");
					Console.WriteLine("book addfile [filename].[mem] - add moves to the book from file");
					Console.WriteLine("book clear - clear all moves from the book");
					continue;
				}
				Uci.SetMsg(msg);
				if (Uci.command == "book")
				{
					switch (Uci.tokens[1])
					{
						case "addfen":
							if (!book.AddFen(Uci.GetValue(2, 0)))
								Console.WriteLine("Wrong fen");
							break;
						case "addfile":
							if (!book.AddFile(Uci.GetValue(2, 0)))
								Console.WriteLine("File not found");
							break;
						case "adduci":
							if (!book.AddUci(Uci.GetValue(2, 0)))
								Console.WriteLine("Wrong uci moves");
							break;
						case "clear":
							book.Clear();
							Console.WriteLine("Book is empty");
							break;
						case "delete":
							int c = book.Delete(Uci.GetInt(2));
							Console.WriteLine($"{c:N0} moves was deleted");
							break;
						case "load":
							if (!book.LoadFromFile(Uci.GetValue(2, 0)))
								Console.WriteLine("File not found");
							break;
						case "moves":
							book.InfoMoves(Uci.GetValue(2, 0));
							break;
						case "structure":
							book.InfoStructure();
							break;
						case "save":
							if (book.SaveToFile(Uci.GetValue(2, 0)))
								Console.WriteLine("Save was successful");
							else
								Console.WriteLine("Writing to the file has failed");
							break;
						default:
							Console.WriteLine($"Unknown command [{Uci.tokens[1]}]");
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
						string fen = Uci.GetValue("fen", "moves");
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
								chess.MakeMove(m, out _);
							}
						}
						if (isWritable && (fen == String.Empty) && chess.Is2ToEnd(out string myMove, out string enMove))
						{
							movesUci.Add(myMove);
							movesUci.Add(enMove);
							if (lba)
								book.LoadFromFile();
							book.AddUci(movesUci);
							book.SaveToFile();
						}
						break;
					case "go":
						string move = book.GetMove(rnd);
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
