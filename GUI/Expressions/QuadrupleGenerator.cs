using System.Collections.Generic;

namespace GUI.Expressions
{
    public class QuadrupleGenerator
    {
        private List<Quadruple> _quadruples;
        private int _tempCounter;

        public QuadrupleGenerationResult Generate(ExpressionNode root)
        {
            _quadruples = new List<Quadruple>();
            _tempCounter = 0;

            var result = new QuadrupleGenerationResult();

            if (root == null)
            {
                result.ResultName = string.Empty;
                return result;
            }

            string expressionResult = GenerateForNode(root);

            foreach (var quadruple in _quadruples)
            {
                result.Quadruples.Add(quadruple);
            }

            result.ResultName = expressionResult;

            return result;
        }

        private string GenerateForNode(ExpressionNode node)
        {
            var operand = node as OperandExpressionNode;
            if (operand != null)
            {
                return operand.Value;
            }

            var binary = node as BinaryExpressionNode;
            if (binary != null)
            {
                string left = GenerateForNode(binary.Left);
                string right = GenerateForNode(binary.Right);

                string temp = CreateTempName();

                var quadruple = new Quadruple(
                    _quadruples.Count + 1,
                    binary.Operator,
                    left,
                    right,
                    temp);

                _quadruples.Add(quadruple);

                return temp;
            }

            return string.Empty;
        }

        private string CreateTempName()
        {
            _tempCounter++;
            return "t" + _tempCounter;
        }
    }
}