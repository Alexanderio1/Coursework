using System;

namespace GUI.Expressions
{
    public class ExpressionLexer
    {
        private string _text;
        private int _position;

        private int _line;
        private int _column;

        private ExpressionLexicalResult _result;

        public ExpressionLexicalResult Analyze(string text)
        {
            _text = text ?? string.Empty;
            _position = 0;
            _line = 1;
            _column = 1;

            _result = new ExpressionLexicalResult();

            while (!IsEnd())
            {
                char current = Current();

                if (char.IsWhiteSpace(current))
                {
                    ReadWhitespace();
                }
                else if (char.IsLetter(current))
                {
                    ReadIdentifier();
                }
                else if (char.IsDigit(current))
                {
                    ReadNumber();
                }
                else
                {
                    ReadOperatorOrUnknown();
                }
            }

            _result.Tokens.Add(new ExpressionToken(
                ExpressionTokenCode.EndOfInput,
                string.Empty,
                _position,
                0,
                _line,
                _column));

            return _result;
        }

        private void ReadIdentifier()
        {
            int startPosition = _position;
            int startLine = _line;
            int startColumn = _column;

            while (!IsEnd())
            {
                char c = Current();

                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    Advance();
                }
                else
                {
                    break;
                }
            }

            string text = _text.Substring(startPosition, _position - startPosition);

            _result.Tokens.Add(new ExpressionToken(
                ExpressionTokenCode.Identifier,
                text,
                startPosition,
                text.Length,
                startLine,
                startColumn));
        }

        private void ReadNumber()
        {
            int startPosition = _position;
            int startLine = _line;
            int startColumn = _column;

            while (!IsEnd() && char.IsDigit(Current()))
            {
                Advance();
            }

            string text = _text.Substring(startPosition, _position - startPosition);

            _result.Tokens.Add(new ExpressionToken(
                ExpressionTokenCode.Number,
                text,
                startPosition,
                text.Length,
                startLine,
                startColumn));
        }

        private void ReadOperatorOrUnknown()
        {
            int startPosition = _position;
            int startLine = _line;
            int startColumn = _column;
            char c = Current();

            ExpressionTokenCode code;

            switch (c)
            {
                case '+':
                    code = ExpressionTokenCode.Plus;
                    break;

                case '-':
                    code = ExpressionTokenCode.Minus;
                    break;

                case '*':
                    code = ExpressionTokenCode.Multiply;
                    break;

                case '/':
                    code = ExpressionTokenCode.Divide;
                    break;

                case '%':
                    code = ExpressionTokenCode.Modulo;
                    break;

                case '(':
                    code = ExpressionTokenCode.LeftParen;
                    break;

                case ')':
                    code = ExpressionTokenCode.RightParen;
                    break;

                default:
                    code = ExpressionTokenCode.Unknown;
                    break;
            }

            Advance();

            string text = _text.Substring(startPosition, 1);

            if (code == ExpressionTokenCode.Unknown)
            {
                _result.Errors.Add(new ExpressionLexicalError(
                    text,
                    "Недопустимый символ в арифметическом выражении",
                    startPosition,
                    1,
                    startLine,
                    startColumn));
            }

            _result.Tokens.Add(new ExpressionToken(
                code,
                text,
                startPosition,
                1,
                startLine,
                startColumn));
        }

        private void ReadWhitespace()
        {
            while (!IsEnd() && char.IsWhiteSpace(Current()))
            {
                Advance();
            }
        }

        private bool IsEnd()
        {
            return _position >= _text.Length;
        }

        private char Current()
        {
            return _text[_position];
        }

        private void Advance()
        {
            if (IsEnd())
            {
                return;
            }

            char c = _text[_position];
            _position++;

            if (c == '\n')
            {
                _line++;
                _column = 1;
            }
            else
            {
                _column++;
            }
        }
    }
}