namespace NSProgram
{
    internal class CHeader
    {
        public const string name = "BookReaderMem";
        public const string version = "2024-12-11";

        public string ToStr()
        {
            return $"{name} {version}";
        }

        public bool FromStr(string s)
        {
            string[] a = s.Split();
            return (a[0] == name) && (a[1] == version);
        }
    }
}
