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
			/// <summary>
			/// Limit ply to write.
			/// </summary>
			int bookLimitW = 0;
			/// <summary>
			/// Limit ply to read.
			/// </summary>
			int bookLimitR = 0;
			/// <summary>
			/// Random moves factor.
			/// </summary>
			int rnd = 0;
			CUci Uci = new CUci();
			CBookMem Book = new CBookMem();
			CChessExt Chess = CBookMem.Chess;
			string ax = "-bn";
			List<string> listBn = new List<string>();
			List<string> listEf = new List<string>();
			List<string> listEa = new List<string>();
			for (int n = 0; n < args.Length; n++)
			{
				string ac = args[n];
				switch (ac)
				{
					case "-bn"://book name
					case "-ef"://engine file
					case "-ea"://engine arguments
					case "-rnd"://random moves
					case "-lr"://limit read
					case "-lw"://limit write
						ax = ac;
						break;
					case "-w"://writable
						ax = ac;
						isWritable = true;
						break;
					case "-lba"://load before add
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
							case "-w":
								ac = ac.Replace("K", "000").Replace("M", "000000");
								Book.maxRecords = int.TryParse(ac, out int m) ? m : 0;
								break;
							case "-lr":
								bookLimitR = int.TryParse(ac, out int lr) ? lr : 0;
								break;
							case "-lw":
								bookLimitW = int.TryParse(ac, out int lw) ? lw : 0;
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

			if (!Book.LoadFromFile(bookName))
				Book.LoadFromFile($"{bookName}{CBookMem.defExt}");
			Console.WriteLine($"info string book {Book.recList.Count:N0} moves");
			while (true)
			{
				string msg = Console.ReadLine().Trim();
				if (String.IsNullOrEmpty(msg) || (msg == "help") || (msg == "book"))
				{
					Console.WriteLine("book load [filename].[mem|pgn|uci|fen] - clear and add moves from file");
					Console.WriteLine("book save [filename].[mem] - save book to the file");
					Console.WriteLine("book delete [number x] - delete x moves from the book");
					Console.WriteLine("book addfile [filename].[mem|png|uci|fen] - add moves to the book from file");
					Console.WriteLine("book adduci [uci] - add moves in uci format to the book");
					Console.WriteLine("book addfen [fen] - add position in fen format");
					Console.WriteLine("book clear - clear all moves from the book");
					Console.WriteLine("book moves [uci] - make sequence of moves in uci format and shows possible continuations");
					Console.WriteLine("book structure - show structure of current book");
					continue;
				}
				Uci.SetMsg(msg);
				int count = Book.recList.Count;
				if (Uci.command == "book")
				{
					switch (Uci.tokens[1])
					{
						case "addfen":
							if (Book.AddFen(Uci.GetValue(2, 0)))
								Console.WriteLine("Fen have been added");
							else
								Console.WriteLine("Wrong fen");
							break;
						case "addfile":
							if (!Book.AddFile(Uci.GetValue(2, 0)))
								Console.WriteLine("File not found");
							else
								Book.ShowMoves(true);
							break;
						case "adduci":
							Book.AddUci(Uci.GetValue(2, 0));
							Console.WriteLine($"{(Book.recList.Count - count):N0} moves have been added");
							break;
						case "clear":
							Book.Clear();
							Console.WriteLine("Book is empty");
							break;
						case "delete":
							int c = Book.Delete(Uci.GetInt(2));
							Console.WriteLine($"{c:N0} moves was deleted");
							break;
						case "load":
							if (!Book.LoadFromFile(Uci.GetValue(2, 0)))
								Console.WriteLine("File not found");
							else
								Book.ShowMoves(true);
							break;
						case "moves":
							Book.InfoMoves(Uci.GetValue(2, 0));
							break;
						case "structure":
							Book.InfoStructure();
							break;
						case "save":
							if (Book.SaveToFile(Uci.GetValue(2, 0)))
								Console.WriteLine("The book has been saved");
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
						string fen = Uci.GetValue("fen", "moves");
						string moves = Uci.GetValue("moves", "fen");
						Chess.SetFen(fen);
						Chess.MakeMoves(moves);
						if (isWritable && String.IsNullOrEmpty(fen) && Chess.Is2ToEnd(out string myMove, out string enMove))
						{
							string[] am = moves.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
							List<string> movesUci = new List<string>();
							foreach (string m in am)
								movesUci.Add(m);
							movesUci.Add(myMove);
							movesUci.Add(enMove);
							if (lba)
								Book.LoadFromFile();
							if (Book.AddUci(movesUci, bookLimitW) > 1)
								Book.Delete(1);
							Book.SaveToFile();
						}
						break;
					case "go":
						string move = String.Empty;
						if ((Chess.g_moveNumber < bookLimitR) || (bookLimitR == 0))
							move = Book.GetMove(rnd);
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
