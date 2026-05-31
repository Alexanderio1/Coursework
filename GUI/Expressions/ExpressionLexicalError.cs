namespace GUI.Expressions
{
    public class ExpressionLexicalError
    {
        public string Fragment { get; private set; }
        public string Message { get; private set; }

        public int Position { get; private set; }
        public int Length { get; private set; }

        public int Line { get; private set; }
        public int Column { get; private set; }

        public ExpressionLexicalError(
            string fragment,
            string message,
            int position,
            int length,
            int line,
            int column)
        {
            Fragment = fragment;
            Message = message;
            Position = position;
            Length = length;
            Line = line;
            Column = column;
        }
    }
}