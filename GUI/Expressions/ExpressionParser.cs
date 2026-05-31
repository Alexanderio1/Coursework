using System.Collections.Generic;

namespace GUI.Expressions
{
    public class ExpressionParser
    {
        private List<ExpressionToken> _tokens;
        private int _position;
        private ExpressionParseResult _result;

        public ExpressionParseResult Analyze(List<ExpressionToken> tokens)
        {
            _tokens = tokens ?? new List<ExpressionToken>();
            _position = 0;
            _result = new ExpressionParseResult();

            if (_tokens.Count == 0)
            {
                AddError(
                    string.Empty,
                    "Ожидалось арифметическое выражение",
                    0,
                    1,
                    1,
                    1);

                return _result;
            }

            ExpressionNode root = ParseE();
            _result.Root = root;

            while (!IsEnd())
            {
                ExpressionToken token = Current();

                if (token.Code == ExpressionTokenCode.RightParen)
                {
                    AddError(token, "Лишняя закрывающая скобка");
                    Advance();
                }
                else if (IsOperandStart(token))
                {
                    AddError(token, "Отсутствует оператор между операндами");
                    Advance();
                }
                else if (IsOperator(token))
                {
                    AddError(token, "Лишний оператор или отсутствует операнд");
                    Advance();
                }
                else
                {
                    AddError(token, "Неожиданный токен в выражении");
                    Advance();
                }
            }

            return _result;
        }

        // E → T A
        private ExpressionNode ParseE()
        {
            ExpressionNode left = ParseT();
            return ParseA(left);
        }

        // A → ε | + T A | - T A
        private ExpressionNode ParseA(ExpressionNode left)
        {
            if (Current().Code == ExpressionTokenCode.Plus ||
                Current().Code == ExpressionTokenCode.Minus)
            {
                ExpressionToken operation = Current();
                Advance();

                ExpressionNode right = ParseRequiredTAfterOperator(operation);

                if (left == null || right == null)
                {
                    return left;
                }

                ExpressionNode node = new BinaryExpressionNode(operation.Text, left, right);
                return ParseA(node);
            }

            return left;
        }

        // T → F B
        private ExpressionNode ParseT()
        {
            ExpressionNode left = ParseF();
            return ParseB(left);
        }

        // B → ε | * F B | / F B | % F B
        private ExpressionNode ParseB(ExpressionNode left)
        {
            if (Current().Code == ExpressionTokenCode.Multiply ||
                Current().Code == ExpressionTokenCode.Divide ||
                Current().Code == ExpressionTokenCode.Modulo)
            {
                ExpressionToken operation = Current();
                Advance();

                ExpressionNode right = ParseRequiredFAfterOperator(operation);

                if (left == null || right == null)
                {
                    return left;
                }

                ExpressionNode node = new BinaryExpressionNode(operation.Text, left, right);
                return ParseB(node);
            }

            return left;
        }

        // F → num | id | (E)
        private ExpressionNode ParseF()
        {
            ExpressionToken token = Current();

            if (token.Code == ExpressionTokenCode.Number)
            {
                Advance();
                return new OperandExpressionNode(token.Text, true);
            }

            if (token.Code == ExpressionTokenCode.Identifier)
            {
                Advance();
                return new OperandExpressionNode(token.Text, false);
            }

            if (token.Code == ExpressionTokenCode.LeftParen)
            {
                Advance();

                ExpressionNode node = ParseE();

                if (Current().Code == ExpressionTokenCode.RightParen)
                {
                    Advance();
                }
                else
                {
                    AddError(Current(), "Ожидалась закрывающая скобка ')'");
                }

                return node;
            }

            if (token.Code == ExpressionTokenCode.RightParen)
            {
                AddError(token, "Ожидался операнд перед закрывающей скобкой");
                return null;
            }

            if (token.Code == ExpressionTokenCode.EndOfInput)
            {
                AddError(token, "Ожидался операнд");
                return null;
            }

            if (IsOperator(token))
            {
                AddError(token, "Ожидался операнд перед оператором");
                Advance();
                return null;
            }

            AddError(token, "Ожидался операнд");
            Advance();
            return null;
        }

        private ExpressionToken Current()
        {
            if (_position >= _tokens.Count)
            {
                return _tokens[_tokens.Count - 1];
            }

            return _tokens[_position];
        }

        private void Advance()
        {
            if (!IsEnd())
            {
                _position++;
            }
        }

        private bool IsEnd()
        {
            return Current().Code == ExpressionTokenCode.EndOfInput;
        }

        private bool IsOperandStart(ExpressionToken token)
        {
            return token.Code == ExpressionTokenCode.Number ||
                   token.Code == ExpressionTokenCode.Identifier ||
                   token.Code == ExpressionTokenCode.LeftParen;
        }

        private bool IsOperator(ExpressionToken token)
        {
            return token.Code == ExpressionTokenCode.Plus ||
                   token.Code == ExpressionTokenCode.Minus ||
                   token.Code == ExpressionTokenCode.Multiply ||
                   token.Code == ExpressionTokenCode.Divide ||
                   token.Code == ExpressionTokenCode.Modulo;
        }

        private void AddError(ExpressionToken token, string message)
        {
            AddError(
                token.Text,
                message,
                token.Position,
                token.Length > 0 ? token.Length : 1,
                token.Line,
                token.Column);
        }

        private void AddError(
            string fragment,
            string message,
            int position,
            int length,
            int line,
            int column)
        {
            _result.Errors.Add(new ExpressionSyntaxError(
                fragment,
                message,
                position,
                length,
                line,
                column));
        }

        private ExpressionNode ParseRequiredTAfterOperator(ExpressionToken operation)
        {
            if (IsEnd() || Current().Code == ExpressionTokenCode.RightParen)
            {
                AddError(operation, "После оператора ожидается операнд");
                return null;
            }

            while (IsOperator(Current()))
            {
                AddError(Current(), "Лишний оператор: перед оператором ожидался операнд");
                Advance();

                if (IsEnd() || Current().Code == ExpressionTokenCode.RightParen)
                {
                    return null;
                }
            }

            if (!IsOperandStart(Current()))
            {
                AddError(Current(), "После оператора ожидается операнд");
                Advance();
                return null;
            }

            return ParseT();
        }

        private ExpressionNode ParseRequiredFAfterOperator(ExpressionToken operation)
        {
            if (IsEnd() || Current().Code == ExpressionTokenCode.RightParen)
            {
                AddError(operation, "После оператора ожидается операнд");
                return null;
            }

            while (IsOperator(Current()))
            {
                AddError(Current(), "Лишний оператор: перед оператором ожидался операнд");
                Advance();

                if (IsEnd() || Current().Code == ExpressionTokenCode.RightParen)
                {
                    return null;
                }
            }

            if (!IsOperandStart(Current()))
            {
                AddError(Current(), "После оператора ожидается операнд");
                Advance();
                return null;
            }

            return ParseF();
        }
    }
}