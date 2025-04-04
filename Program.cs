﻿using NSUci;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using RapLog;

namespace NSProgram
{
	class Program
	{
		public static int added = 0;
		public static int updated = 0;
		public static int deleted = 0;
		/// <summary>
		/// Moves added to book per game.
		/// </summary>
		public static int bookLimitAdd = 8;
		/// <summary>
		/// Limit ply to wrtie.
		/// </summary>
		public static int bookLimitW = 8;
		/// <summary>
		/// Limit ply to read.
		/// </summary>
		public static int bookLimitR = 8;
		public static bool isIv = false;
		public static CRapLog log = new CRapLog(false);
		public static CBook book = new CBook();

		static void Main(string[] args)
		{
			bool bookWrite = false;
			bool isInfo = false;
			/// <summary>
			/// Book can update moves.
			/// </summary>
			bool isW = false;
			/// <summary>
			/// Random moves factor.
			/// </summary>
			int bookRandom = 100;
			string lastFen = String.Empty;
			string lastMoves = String.Empty;
			CUci uci = new CUci();
			object locker = new object();
			string ax = "-bn";
			List<string> listBn = new List<string>();
			List<string> listEf = new List<string>();
			List<string> listEa = new List<string>();
			List<string> listTf = new List<string>();
			for (int n = 0; n < args.Length; n++)
			{
				string ac = args[n];
				switch (ac)
				{
					case "-add"://moves add to book
					case "-bn"://book name
					case "-ef"://engine file
					case "-ea"://engine arguments
					case "-rnd"://random moves
					case "-lr"://limit read in half moves
					case "-lw"://limit write in half moves
					case "-tf"://teacher file
						ax = ac;
						break;
					case "-log"://add log
						ax = ac;
						log.enabled = true;
						break;
					case "-w"://writable
						ax = ac;
						isW = true;
						break;
					case "-info":
						ax = ac;
						isInfo = true;
						break;
					case "-iv":
						ax = ac;
						isIv = true;
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
							case "-tf":
								listTf.Add(ac);
								break;
							case "-ea":
								listEa.Add(ac);
								break;
							case "-w":
								ac = ac.Replace("K", "000").Replace("M", "000000");
								book.maxRecords = int.TryParse(ac, out int m) ? m : 0;
								break;
							case "-add":
								bookLimitAdd = int.TryParse(ac, out int a) ? a : bookLimitAdd;
								break;
							case "-rnd":
								bookRandom = int.TryParse(ac, out int r) ? r : 0;
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
			string bookFile = String.Join(" ", listBn);
			string engineFile = String.Join(" ", listEf);
			string engineArguments = String.Join(" ", listEa);
            Console.WriteLine($"idbook name {CHeader.name}");
            Console.WriteLine($"idbook version {CHeader.version}");
            bool bookLoaded = SetBookFile(bookFile);
			Process engineProcess = null;
			if (File.Exists(engineFile))
			{
				engineProcess = new Process();
				engineProcess.StartInfo.FileName = engineFile;
				engineProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(engineFile);
				engineProcess.StartInfo.UseShellExecute = false;
				engineProcess.StartInfo.RedirectStandardInput = true;
				engineProcess.StartInfo.Arguments = engineArguments;
				engineProcess.Start();
				Console.WriteLine($"info string engine on");
			}
			else if (engineFile != String.Empty)
					Console.WriteLine($"info string missing engine  [{engineFile}]");
			if (bookLoaded && isW)
			{
				bookLimitR = 0;
				bookLimitW = 0;
			}
			do
			{
				lock (locker)
				{
					string msg = Console.ReadLine().Trim();
					if (String.IsNullOrEmpty(msg) || (msg == "help") || (msg == "book"))
					{
						Console.WriteLine("book load [filename].[mem|pgn|uci|fen] - clear and add moves from file");
						Console.WriteLine("book save [filename].[mem] - save book to the file");
						Console.WriteLine("book delete [number x] - delete x moves from the book");
						Console.WriteLine("book addfile [filename].[mem|png|uci|fen] - add moves to the book from file");
						Console.WriteLine("book adduci [uci] - add moves in uci format to the book");
						Console.WriteLine("book clear - clear all moves from the book");
						Console.WriteLine("book moves [uci] - make sequence of moves in uci format and shows possible continuations");
						continue;
					}
					uci.SetMsg(msg);
					int count = book.recList.Count;
					if (uci.command == "book")
					{
						switch (uci.tokens[1])
						{
							case "addfen":
								if (book.AddFen(uci.GetValue("addfen")))
									Console.WriteLine("Fen have been added");
								else
									Console.WriteLine("Wrong fen");
								break;
							case "addfile":
								string fn = uci.GetValue("addfile");
								if (File.Exists(fn))
								{
									book.AddFile(fn);
									book.ShowMoves(true);
								}
								else Console.WriteLine("File not found");
								break;
							case "adduci":
								book.AddUci(uci.GetValue("adduci"),out _);
								Console.WriteLine($"{book.recList.Count - count:N0} moves have been added");
								break;
							case "clear":
								book.Clear();
								Console.WriteLine("Book is empty");
								break;
							case "delete":
								int c = book.Delete(uci.GetInt("delete"));
								Console.WriteLine($"{c:N0} moves was deleted");
								break;
							case "load":
								book.LoadFromFile(uci.GetValue("load"));
								book.ShowMoves(true);
								break;
							case "moves":
								book.InfoMoves(uci.GetValue("moves"));
								break;
							case "save":
								if (book.SaveToFile(uci.GetValue("save")))
									Console.WriteLine("The book has been saved");
								else
									Console.WriteLine("Writing to the file has failed");
								break;
                            case "info":
                                book.ShowInfo();
                                break;
                            case "getoption":
								Console.WriteLine($"option name book_file type string default book{CBook.defExt}");
								Console.WriteLine($"option name write type check default false");
								Console.WriteLine($"option name log type check default false");
								Console.WriteLine($"option name limit_add_moves type spin default {bookLimitAdd} min 0 max 100");
								Console.WriteLine($"option name limit_ply_read type spin default {bookLimitR} min 0 max 100");
								Console.WriteLine($"option name limit_ply_write type spin default {bookLimitW} min 0 max 100");
								Console.WriteLine($"option name random type spin default {bookRandom} min 0 max 201");
								Console.WriteLine("optionend");
								break;
							case "setoption":
								switch (uci.GetValue("name", "value").ToLower())
								{
									case "book_file":
                                        SetBookFile(uci.GetValue("value"));
                                        break;
									case "write":
										isW = uci.GetValue("value") == "true";
										break;
									case "log":
										log.enabled = uci.GetValue("value") == "true";
										break;
									case "limit_add_moves":
										bookLimitAdd = uci.GetInt("value");
										break;
									case "limit_ply_read":
										bookLimitR = uci.GetInt("value");
										break;
									case "limit_ply_write":
										bookLimitW = uci.GetInt("value");
										break;
									case "random":
										bookRandom = uci.GetInt("value");
										break;
								}
								break;
							default:
								Console.WriteLine($"Unknown command [{uci.tokens[1]}]");
								break;
						}
						continue;
					}
					if ((uci.command != "go") && (engineProcess != null))
						engineProcess.StandardInput.WriteLine(msg);
					switch (uci.command)
					{
						case "position":
							lastFen = uci.GetValue("fen", "moves");
							lastMoves = uci.GetValue("moves", "fen");
							book.chess.SetFen(lastFen);
							book.chess.MakeMoves(lastMoves);
							if (String.IsNullOrEmpty(lastFen))
							{
								if (book.chess.halfMove < 2)
								{
									bookWrite = isW;
									added = 0;
									updated = 0;
									deleted = 0;
								}
								if (bookLoaded && bookWrite && book.chess.Is2ToEnd(out string myMove, out string enMove))
								{
									string[] am = lastMoves.Split(' ');
									List<string> movesUci = new List<string>();
									foreach (string m in am)
										movesUci.Add(m);
									movesUci.Add(myMove);
									movesUci.Add(enMove);
									added += book.AddUciMate(movesUci);
									book.SaveToFile();
								}
							}
							break;
						case "go":
							string move = String.Empty;
							if ((bookLimitR == 0) || (bookLimitR > book.chess.halfMove))
								move = book.GetMove(lastFen, lastMoves, bookRandom,ref bookWrite);
							if (move != String.Empty)
								Console.WriteLine($"bestmove {move}");
							else
								if (engineProcess == null)
									Console.WriteLine("enginemove");
								else
									engineProcess.StandardInput.WriteLine(msg);
							break;
					}
				}
			} while (uci.command != "quit");

			bool SetBookFile(string bn)
			{
				bookFile = bn;
				bookLoaded = book.LoadFromFile(bookFile);
				if (bookLoaded)
				{
					if ((book.recList.Count > 0) && File.Exists(book.path))
					{
						FileInfo fi = new FileInfo(book.path);
						long bpm = (fi.Length << 3) / book.recList.Count;
						Console.WriteLine($"info string book on {book.recList.Count:N0} moves {bpm} bpm");
					}
					if (isW)
						Console.WriteLine($"info string write on");
					if (isInfo)
						book.InfoMoves();
				}
				else
					isW = false;
				return bookLoaded;
			}
		}
	}
}
