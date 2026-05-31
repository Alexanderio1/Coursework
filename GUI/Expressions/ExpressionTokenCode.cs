namespace GUI.Expressions
{
    public enum ExpressionTokenCode
    {
        Identifier,
        Number,

        Plus,       // +
        Minus,      // -
        Multiply,   // *
        Divide,     // /
        Modulo,     // %

        LeftParen,  // (
        RightParen, // )

        EndOfInput,
        Unknown
    }
}