namespace GUI.Expressions
{
    public class ExpressionToken
    {
        public ExpressionTokenCode Code { get; private set; }
        public string Text { get; private set; }

        public int Position { get; private set; }
        public int Length { get; private set; }

        public int Line { get; private set; }
        public int Column { get; private set; }

        public ExpressionToken(
            ExpressionTokenCode code,
            string text,
            int position,
            int length,
            int line,
            int column)
        {
            Code = code;
            Text = text;
            Position = position;
            Length = length;
            Line = line;
            Column = column;
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", Code, Text);
        }
    }
}