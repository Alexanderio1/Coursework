namespace GUI.ANTLR
{
    public sealed class AntlrSyntaxError
    {
        public string InvalidFragment { get; set; } = string.Empty;
        public int Line { get; set; }
        public int StartColumn { get; set; }
        public int EndColumn { get; set; }
        public int AbsoluteIndex { get; set; }
        public string Message { get; set; } = string.Empty;

        public string LocationText
        {
            get
            {
                if (StartColumn == EndColumn)
                    return $"строка {Line}, позиция {StartColumn}";
                return $"строка {Line}, позиции {StartColumn}-{EndColumn}";
            }
        }
    }
}