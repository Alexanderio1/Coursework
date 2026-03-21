namespace GUI.Lexer
{
    public class LexerItem
    {
        public bool IsError { get; set; }
        public int? Code { get; set; }
        public string TypeName { get; set; }
        public string Lexeme { get; set; }
        public string Message { get; set; }
        public int Line { get; set; }
        public int StartColumn { get; set; }
        public int EndColumn { get; set; }
        public int AbsoluteIndex { get; set; }

        public string DisplayCode
        {
            get
            {
                if (IsError) return "ERR";
                return Code.HasValue ? Code.Value.ToString() : string.Empty;
            }
        }

        public string DisplayText
        {
            get
            {
                return IsError ? Message : Lexeme;
            }
        }

        public string LocationText
        {
            get
            {
                return string.Format("строка {0}, {1}-{2}", Line, StartColumn, EndColumn);
            }
        }
    }
}