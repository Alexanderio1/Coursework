namespace GUI.Expressions
{
    public abstract class ExpressionNode
    {
    }

    public class OperandExpressionNode : ExpressionNode
    {
        public string Value { get; private set; }
        public bool IsNumber { get; private set; }

        public OperandExpressionNode(string value, bool isNumber)
        {
            Value = value;
            IsNumber = isNumber;
        }
    }

    public class BinaryExpressionNode : ExpressionNode
    {
        public string Operator { get; private set; }
        public ExpressionNode Left { get; private set; }
        public ExpressionNode Right { get; private set; }

        public BinaryExpressionNode(string operatorText, ExpressionNode left, ExpressionNode right)
        {
            Operator = operatorText;
            Left = left;
            Right = right;
        }
    }
}