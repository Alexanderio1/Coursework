namespace GUI.Expressions
{
    public class Quadruple
    {
        public int Number { get; private set; }

        public string Operator { get; private set; }
        public string Arg1 { get; private set; }
        public string Arg2 { get; private set; }
        public string Result { get; private set; }

        public Quadruple(int number, string operatorText, string arg1, string arg2, string result)
        {
            Number = number;
            Operator = operatorText;
            Arg1 = arg1;
            Arg2 = arg2;
            Result = result;
        }
    }
}