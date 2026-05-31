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

            BuildForNode(root);

            return _result;
        }

        private void BuildForNode(ExpressionNode node)
        {
            var operand = node as OperandExpressionNode;
            if (operand != null)
            {
                _result.Items.Add(operand.Value);

                if (!operand.IsNumber)
                {
                    _result.ContainsIdentifiers = true;
                }

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