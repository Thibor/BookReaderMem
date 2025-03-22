using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RapLog;

namespace NSProgram
{

    class CBook
    {
        public string path = String.Empty;
        public int errors = 0;
        public int maxRecords = 0;
        int lastCount = 0;
        public const string defExt = ".mem";
        public CChessExt chess = new CChessExt();
        public CRecList recList = new CRecList();
        readonly Stopwatch stopWatch = new Stopwatch();
        public readonly CHeader header = new CHeader();

        #region file mem

        bool AddFileTnt(string p)
        {
            path = p;
            if (!File.Exists(p))
                return true;
            string pt = p + ".tmp";
            try
            {
                if (!File.Exists(p) && File.Exists(pt))
                    File.Move(pt, p);
            }
            catch
            {
                return false;
            }
            try
            {
                using (FileStream fs = File.Open(p, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (BinaryReader reader = new BinaryReader(fs))
                {
                    string headerCur = reader.ReadString();
                    if (!Program.isIv && !header.FromStr(headerCur))
                        Console.WriteLine($"This program only supports version [{header.ToStr()}]");
                    else
                    {
                        while (reader.BaseStream.Position != reader.BaseStream.Length)
                        {
                            ulong m = ReadUInt64(reader);
                            ulong b = ReadUInt64(reader);
                            ulong w = ReadUInt64(reader);
                            byte win = ReadU8(reader);
                            byte loose = ReadU8(reader);
                            CRec rec = new CRec(MbwToTnt(m, b, w)) { win = win, lost = loose };
                            recList.Add(rec);
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
            lastCount = recList.Count;
            return true;
        }

        public bool SaveToTnt(string p)
        {
            string pt = p + ".tmp";
            try
            {
                using (FileStream fs = File.Open(pt, FileMode.Create, FileAccess.Write, FileShare.None))
                using (BinaryWriter writer = new BinaryWriter(fs))
                {
                    string lastTnt = String.Empty;
                    recList.SortTnt();
                    writer.Write(header.ToStr());
                    foreach (CRec rec in recList)
                    {
                        if ((rec.tnt == lastTnt) || ((rec.win < 1) && (rec.lost > 0xf)))
                        {
                            Program.deleted++;
                            continue;
                        }
                        TntToMbw(rec.tnt, out ulong m, out ulong b, out ulong w);
                        WriteUInt64(writer, m);
                        WriteUInt64(writer, b);
                        WriteUInt64(writer, w);
                        WriteU8(writer, rec.win);
                        WriteU8(writer, rec.lost);
                        lastTnt = rec.tnt;
                    }
                }
            }
            catch
            {
                return false;
            }
            try
            {
                if (File.Exists(p) && File.Exists(pt))
                    File.Delete(p);
            }
            catch
            {
                return false;
            }
            try
            {
                if (!File.Exists(p) && File.Exists(pt))
                    File.Move(pt, p);
            }
            catch
            {
                return false;
            }
            if (recList.Count / 100 > lastCount / 100)
                Program.log.Add($"book {recList.Count:N0} moves");
            lastCount = recList.Count;
            return true;
        }

        #endregion file mem

        #region file uci

        public bool SaveToUci(string p)
        {
            List<string> sl = GetGames();
            FileStream fs = File.Open(p, FileMode.Create, FileAccess.Write, FileShare.None);
            using (StreamWriter sw = new StreamWriter(fs))
            {
                foreach (String uci in sl)
                    sw.WriteLine(uci);
            }
            return true;
        }

        #endregion file uci

        #region file pgn

        public bool SaveToPgn(string p)
        {
            List<string> sl = GetGames();
            int line = 0;
            FileStream fs = File.Open(p, FileMode.Create, FileAccess.Write, FileShare.None);
            using (StreamWriter sw = new StreamWriter(fs))
            {
                foreach (String uci in sl)
                {
                    string[] arrMoves = uci.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    chess.SetFen();
                    string pgn = String.Empty;
                    foreach (string umo in arrMoves)
                    {
                        string san = chess.UmoToSan(umo);
                        if (san == String.Empty)
                            break;
                        if (chess.WhiteTurn)
                            pgn += $" {chess.MoveNumber}. {san}";
                        else
                            pgn += $" {san}";
                        int emo = chess.UmoToEmo(umo);
                        chess.MakeMove(emo);
                    }
                    sw.WriteLine();
                    sw.WriteLine("[White \"White\"]");
                    sw.WriteLine("[Black \"Black\"]");
                    sw.WriteLine();
                    sw.WriteLine(pgn.Trim());
                    Console.Write($"\rgames {++line}");
                }
            }
            Console.WriteLine();
            return true;
        }

        #endregion file pgn

        public void ShowMoves(bool last = false)
        {
            Console.Write($"\r{recList.Count} moves");
            if (last)
            {
                Console.WriteLine();
                if (errors > 0)
                    Console.WriteLine($"{errors} errors");
                errors = 0;
            }
        }

        public void Clear()
        {
            recList.Clear();
        }

        public bool AddFen(string fen)
        {
            if (chess.SetFen(fen))
            {
                CRec rec = new CRec(chess.GetTnt());
                recList.AddRec(rec);
                return true;
            }
            return false;
        }

        public int AddUci(string moves, out string uci, int limitLen = 0, int limitAdd = 0)
        {
            return AddUci(moves.Trim().Split(' '), out uci, limitLen, limitAdd);
        }

        public int AddUci(List<string> moves, out string uci, int limitLen = 0, int limitAdd = 0)
        {
            return AddUci(moves.ToArray(), out uci, limitLen, limitAdd);
        }

        public int AddUci(string[] moves, out string uci, int limitLen = 0, int limitAdd = 0)
        {
            uci = String.Empty;
            int ca = 0;
            if ((limitLen == 0) || (limitLen > moves.Length))
                limitLen = moves.Length;
            chess.SetFen();
            for (int n = 0; n < limitLen; n++)
            {
                bool iw = IsWinner(n, moves.Length);
                string m = moves[n];
                uci = n == 0 ? m : $"{uci} {m}";
                if (chess.MakeMove(m, out _))
                {
                    CRec rec = new CRec(chess.GetTnt());
                    if (iw)
                        rec.win++;
                    else
                        rec.lost++;
                    if ((rec.win == 0xff) || (rec.lost == 0xff))
                    {
                        rec.win >>= 1;
                        rec.lost >>= 1;
                    }
                    if (recList.AddRec(rec))
                        ca++;
                    if ((limitAdd > 0) && (ca >= limitAdd))
                        break;
                }
                else
                    break;
            }
            return ca;
        }

        public int AddUciMate(string moves)
        {
            return AddUciMate(moves.Trim().Split(' '));
        }

        public int AddUciMate(List<string> moves)
        {
            return AddUciMate(moves.ToArray());
        }

        public int AddUciMate(string[] moves)
        {
            int ca = 0;
            chess.SetFen();
            for (int n = 0; n < moves.Length; n++)
            {
                string m = moves[n];
                if (!chess.MakeMove(m, out _))
                    return ca;
                bool iw = IsWinner(n, moves.Length);
                string tnt = chess.GetTnt();
                CRec rec = new CRec(tnt);
                if (iw)
                    rec.win++;
                else
                    rec.lost++;
                if (recList.AddRec(rec))
                    ca++;
                if ((Program.bookLimitAdd > 0) && (ca >= Program.bookLimitAdd))
                    break;
            }
            return ca;
        }

        void WriteU8(BinaryWriter writer, byte v)
        {
            writer.Write(v);
        }

        byte ReadU8(BinaryReader reader)
        {
            return reader.ReadByte();
        }

        void WriteUInt16(BinaryWriter writer, ushort v)
        {
            byte[] bytes = BitConverter.GetBytes(v);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            writer.Write(bytes);
        }

        ushort ReadUInt16(BinaryReader reader)
        {
            ushort v = reader.ReadUInt16();
            if (BitConverter.IsLittleEndian)
            {
                byte[] bytes = BitConverter.GetBytes(v).Reverse().ToArray();
                return BitConverter.ToUInt16(bytes, 0);
            }
            return v;
        }

        void WriteUInt64(BinaryWriter writer, ulong v)
        {
            byte[] bytes = BitConverter.GetBytes(v);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            writer.Write(bytes);
        }

        ulong ReadUInt64(BinaryReader reader)
        {
            ulong v = reader.ReadUInt64();
            if (BitConverter.IsLittleEndian)
            {
                byte[] bytes = BitConverter.GetBytes(v).Reverse().ToArray();
                return BitConverter.ToUInt64(bytes, 0);
            }
            return v;
        }

        void TntToMbw(string tnt, out ulong m, out ulong b, out ulong w)
        {
            m = 0xFFFFFFFFFFFFFFFF;
            b = 0;
            w = 0;
            int z = 0;
            for (int n = 0; n < 64; n++)
            {
                ulong p = 0;
                switch (tnt[n])
                {
                    case '-':
                        m ^= 1ul << n;
                        break;
                    case 'a':
                        p = 1;
                        break;
                    case 'P':
                        p = 2;
                        break;
                    case 'p':
                        p = 3;
                        break;
                    case 'N':
                        p = 4;
                        break;
                    case 'n':
                        p = 5;
                        break;
                    case 'B':
                        p = 6;
                        break;
                    case 'b':
                        p = 7;
                        break;
                    case 'R':
                        p = 8;
                        break;
                    case 'r':
                        p = 9;
                        break;
                    case 'Q':
                        p = 10;
                        break;
                    case 'q':
                        p = 11;
                        break;
                    case 'K':
                        p = 12;
                        break;
                    case 'k':
                        p = 13;
                        break;
                    case 'T':
                        p = 14;
                        break;
                    case 't':
                        p = 15;
                        break;
                }
                if (p > 0)
                {
                    int s = (z & 0xf) << 2;
                    if (z++ < 16)
                        b |= p << s;
                    else
                        w |= p << s;
                }
            }
        }

        string MbwToTnt(ulong m, ulong b, ulong w)
        {
            string tnt = String.Empty;
            int z = 0;
            for (int n = 0; n < 64; n++)
            {
                if ((m & (1ul << n)) == 0)
                    tnt += "-";
                else
                {
                    int s = (z & 0xf) << 2;
                    ulong p = z++ < 16 ? (b >> s) & 0xf : (w >> s) & 0xf;
                    switch (p)
                    {
                        case 1:
                            tnt += "a";
                            break;
                        case 2:
                            tnt += "P";
                            break;
                        case 3:
                            tnt += "p";
                            break;
                        case 4:
                            tnt += "N";
                            break;
                        case 5:
                            tnt += "n";
                            break;
                        case 6:
                            tnt += "B";
                            break;
                        case 7:
                            tnt += "b";
                            break;
                        case 8:
                            tnt += "R";
                            break;
                        case 9:
                            tnt += "r";
                            break;
                        case 10:
                            tnt += "Q";
                            break;
                        case 11:
                            tnt += "q";
                            break;
                        case 12:
                            tnt += "K";
                            break;
                        case 13:
                            tnt += "k";
                            break;
                        case 14:
                            tnt += "T";
                            break;
                        case 15:
                            tnt += "t";
                            break;
                    }
                }
            }
            return tnt;
        }

        public int Delete(int c)
        {
            return recList.RecDelete(c);
        }

        public bool IsWinner(int index, int count)
        {
            return (index & 1) != (count & 1);
        }

        public CEmoList GetNotUsedList(CEmoList el)
        {
            if (el.Count == 0)
                return el;
            CEmoList emoList = new CEmoList();
            List<int> moves = chess.GenerateValidMoves(out _);
            foreach (int m in moves)
            {
                if (el.GetEmo(m) == null)
                {
                    CEmo emo = new CEmo(m);
                    emoList.Add(emo);
                }
            }
            if (emoList.Count > 0)
                return emoList;
            return el;
        }

        public CEmoList GetEmoList()
        {
            CEmoList emoList = new CEmoList();
            List<int> moves = chess.GenerateValidMoves(out _);
            foreach (int m in moves)
            {
                chess.MakeMove(m);
                string tnt = chess.GetTnt();
                CRec rec = recList.GetRec(tnt);
                if (rec != null)
                {
                    CEmo emo = new CEmo(m, rec);
                    emoList.Add(emo);
                }
                chess.UnmakeMove(m);
            }
            emoList.SortGames();
            return emoList;
        }

        public string GetMove(string fen, string moves, int rnd, ref bool bookWrite)
        {
            chess.SetFen(fen);
            chess.MakeMoves(moves);
            CEmoList emoList = GetEmoList();
            if (rnd > 200)
            {
                rnd = 100;
                emoList = GetNotUsedList(emoList);
            }
            if (emoList.Count == 0)
                return String.Empty;
            CEmo bst = emoList.GetRnd(rnd);
            chess.MakeMove(bst.emo);
            if (chess.IsRepetition())
            {
                bookWrite = false;
                return String.Empty;
            }
            string umo = chess.EmoToUmo(bst.emo);
            if (bst.rec != null)
                Console.WriteLine($"info score cp {bst.rec.GetScore()}");
            return umo;
        }

        public void InfoMoves(string moves = "")
        {
            chess.SetFen();
            if (!chess.MakeMoves(moves))
                Console.WriteLine("wrong moves");
            else
            {
                string frm = "{0,7} {1,7} {2,7} {3,7} {4,7}";
                CEmoList el = GetEmoList();
                if (el.Count == 0)
                    Console.WriteLine("no moves found");
                else
                {
                    Console.WriteLine();
                    Console.WriteLine(frm, "id", "move", "score", "win", "lost");
                    int i = 1;
                    foreach (CEmo e in el)
                    {
                        string umo = chess.EmoToUmo(e.emo);
                        Console.WriteLine(frm, i++, umo, e.rec.GetScore(), e.rec.win, e.rec.lost);
                    }
                }
            }
        }

        #region save

        public bool SaveToFile(string p = "")
        {
            if (string.IsNullOrEmpty(p))
                if (string.IsNullOrEmpty(path))
                    return false;
                else
                    SaveToFile(path);
            string ext = Path.GetExtension(p).ToLower();
            if (ext == defExt)
                return SaveToTnt(p);
            if (ext == ".uci")
                return SaveToUci(p);
            if (ext == ".pgn")
                return SaveToPgn(p);
            if (ext == ".txt")
                return SaveToTxt(p);
            return false;
        }

        public bool SaveToTxt(string p)
        {
            int line = 0;
            FileStream fs = File.Open(p, FileMode.Create, FileAccess.Write, FileShare.None);
            using (StreamWriter sw = new StreamWriter(fs))
            {
                foreach (CRec rec in recList)
                {
                    string l = $"{rec.tnt}{rec.Games():+#;-#;+0}";
                    sw.WriteLine(l);
                    Console.Write($"\rRecord {++line}");
                }
            }
            Console.WriteLine();
            return true;
        }

        List<string> GetGames()
        {
            List<string> sl = new List<string>();
            GetGames(string.Empty, 0, 1, ref sl);
            Console.WriteLine();
            Console.WriteLine("finish");
            Console.Beep();
            sl.Sort();
            return sl;
        }

        void GetGames(string moves, double proT, double proU, ref List<string> list)
        {
            bool add = true;
            chess.SetFen();
            chess.MakeMoves(moves);
            CEmoList el = GetEmoList();
            if (el.Count > 0)
            {
                proU /= el.Count;
                bool wt = chess.WhiteTurn;
                for (int n = 0; n < el.Count; n++)
                {
                    CEmo emo = el[n];
                    string umo = chess.EmoToUmo(emo.emo);
                    if (chess.MoveProgress(umo, wt) < 0)
                        continue;
                    double p = proT + n * proU;
                    add = false;
                    GetGames($"{moves} {chess.EmoToUmo(emo.emo)}".Trim(), p, proU, ref list);
                }
            }
            if (add)
            {
                list.Add(moves);
                double pro = (proT + proU) * 100.0;
                Console.Write($"\r{pro:N4} %");
            }
        }

        #endregion save

        #region load

        public bool LoadFromFile(string p = "")
        {
            if (String.IsNullOrEmpty(p))
                if (String.IsNullOrEmpty(path))
                    return false;
                else
                    return LoadFromFile(path);
            stopWatch.Restart();
            recList.Clear();
            bool result = AddFile(p);
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            Console.WriteLine($"info string Loaded in {ts.Seconds}.{ts.Milliseconds} seconds");
            return result;
        }

        public bool AddFile(string p)
        {
            string ext = Path.GetExtension(p).ToLower();
            if (ext == defExt)
                return AddFileTnt(p);
            else if (ext == ".uci")
                return AddFileUci(p);
            else if (ext == ".pgn")
                return AddFilePgn(p);
            Console.WriteLine($"info string moves {recList.Count:N0}");
            return false;
        }

        bool AddFileUci(string p)
        {
            string[] lines = File.ReadAllLines(p);
            foreach (string uci in lines)
                AddUci(uci, out _);
            return true;
        }

        bool AddFilePgn(string p)
        {
            List<string> listPgn = File.ReadAllLines(p).ToList();
            string movesUci = String.Empty;
            chess.SetFen();
            foreach (string m in listPgn)
            {
                string cm = m.Trim();
                if (String.IsNullOrEmpty(cm))
                    continue;
                if (cm[0] == '[')
                    continue;
                cm = Regex.Replace(cm, @"\.(?! |$)", ". ");
                if (cm.StartsWith("1. "))
                {
                    AddUci(movesUci, out _);
                    ShowMoves();
                    movesUci = String.Empty;
                    chess.SetFen();
                }
                string[] arrMoves = cm.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string san in arrMoves)
                {
                    if (Char.IsDigit(san[0]))
                        continue;
                    string umo = chess.SanToUmo(san);
                    if (umo == String.Empty)
                    {
                        errors++;
                        break;
                    }
                    movesUci += $" {umo}";
                    int emo = chess.UmoToEmo(umo);
                    chess.MakeMove(emo);
                }
            }
            AddUci(movesUci, out _);
            ShowMoves();
            return true;
        }

        #endregion load

        public void ShowInfo()
        {
            if (recList.Count == 0)
            {
                Console.WriteLine("no records");
                return;
            }
            InfoMoves();
        }

    }
}
