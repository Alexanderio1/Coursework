namespace GUI.Expressions
{
    public class PolizBuilder
    {
        private PolizGenerationResult _result;

        public PolizGenerationResult Build(ExpressionNode root)
        {
            _result = new PolizGenerationResult();

            if (root == null)
            {
                return _result;
            }

            if (ContainsIdentifier(root))
            {
                _result.ContainsIdentifiers = true;
                return _result;
            }

            BuildForNode(root);
            return _result;
        }

        private bool ContainsIdentifier(ExpressionNode node)
        {
            var operand = node as OperandExpressionNode;

            if (operand != null)
            {
                return !operand.IsNumber;
            }

            var binary = node as BinaryExpressionNode;

            if (binary != null)
            {
                return ContainsIdentifier(binary.Left) ||
                       ContainsIdentifier(binary.Right);
            }

            return false;
        }

        private void BuildForNode(ExpressionNode node)
        {
            var operand = node as OperandExpressionNode;

            if (operand != null)
            {
                _result.Items.Add(operand.Value);
                return;
            }

            var binary = node as BinaryExpressionNode;

            if (binary != null)
            {
                BuildForNode(binary.Left);
                BuildForNode(binary.Right);
                _result.Items.Add(binary.Operator);
            }
        }
    }
}