using System;

namespace GUI.Lexer
{
    public class LexicalAnalyzer
    {
        public LexerResult Analyze(string text)
        {
            var result = new LexerResult();

            if (string.IsNullOrEmpty(text))
                return result;

            int index = 0;
            int line = 1;
            int column = 1;

            while (index < text.Length)
            {
                char current = Peek(text, index);

                if (char.IsWhiteSpace(current))
                {
                    SkipWhitespace(text, ref index, ref line, ref column);
                    continue;
                }

                if (IsIdentifierStart(current))
                {
                    ReadIdentifierOrKeyword(text, result, ref index, ref line, ref column);
                    continue;
                }

                if (char.IsDigit(current))
                {
                    ReadNumber(text, result, ref index, ref line, ref column);
                    continue;
                }

                switch (current)
                {
                    case '=':
                        AddSingleCharToken(result, LexerTokenCode.Assign, "оператор присваивания", "=", ref index, ref line, ref column, text);
                        break;

                    case '(':
                        AddSingleCharToken(result, LexerTokenCode.LeftParen, "открывающая круглая скобка", "(", ref index, ref line, ref column, text);
                        break;

                    case ',':
                        AddSingleCharToken(result, LexerTokenCode.Comma, "разделитель (запятая)", ",", ref index, ref line, ref column, text);
                        break;

                    case ')':
                        AddSingleCharToken(result, LexerTokenCode.RightParen, "закрывающая круглая скобка", ")", ref index, ref line, ref column, text);
                        break;

                    case ';':
                        AddSingleCharToken(result, LexerTokenCode.Semicolon, "конец оператора", ";", ref index, ref line, ref column, text);
                        break;

                    case '+':
                        AddSingleCharToken(result, LexerTokenCode.Plus, "знак плюс", "+", ref index, ref line, ref column, text);
                        break;

                    case '-':
                        AddSingleCharToken(result, LexerTokenCode.Minus, "знак минус", "-", ref index, ref line, ref column, text);
                        break;

                    case '"':
                        ReadString(text, result, ref index, ref line, ref column);
                        break;

                    case '\'':
                        ReadChar(text, result, ref index, ref line, ref column);
                        break;

                    default:
                        AddError(
                            result,
                            string.Format("Недопустимый символ '{0}'", DescribeChar(current)),
                            current.ToString(),
                            index,
                            line,
                            column,
                            1);

                        Advance(text, ref index, ref line, ref column);
                        break;
                }
            }

            return result;
        }

        private void ReadIdentifierOrKeyword(string text, LexerResult result, ref int index, ref int line, ref int column)
        {
            int startIndex = index;
            int startLine = line;
            int startColumn = column;

            while (IsIdentifierPart(Peek(text, index)))
            {
                Advance(text, ref index, ref line, ref column);
            }

            string lexeme = text.Substring(startIndex, index - startIndex);

            LexerTokenCode code;
            string typeName;

            switch (lexeme)
            {
                case "val":
                    code = LexerTokenCode.Val;
                    typeName = "ключевое слово";
                    break;

                case "listOf":
                    code = LexerTokenCode.ListOf;
                    typeName = "специальная лексема";
                    break;

                case "true":
                    code = LexerTokenCode.True;
                    typeName = "булев литерал";
                    break;

                case "false":
                    code = LexerTokenCode.False;
                    typeName = "булев литерал";
                    break;

                default:
                    code = LexerTokenCode.Identifier;
                    typeName = "идентификатор";
                    break;
            }

            AddToken(result, code, typeName, lexeme, startIndex, startLine, startColumn);
        }

        private void ReadNumber(string text, LexerResult result, ref int index, ref int line, ref int column)
        {
            int startIndex = index;
            int startLine = line;
            int startColumn = column;

            while (char.IsDigit(Peek(text, index)))
            {
                Advance(text, ref index, ref line, ref column);
            }

            bool isDouble = false;

            if (Peek(text, index) == '.' && char.IsDigit(Peek(text, index + 1)))
            {
                isDouble = true;

                Advance(text, ref index, ref line, ref column);

                while (char.IsDigit(Peek(text, index)))
                {
                    Advance(text, ref index, ref line, ref column);
                }
            }

            string lexeme = text.Substring(startIndex, index - startIndex);

            AddToken(
                result,
                isDouble ? LexerTokenCode.Double : LexerTokenCode.Int,
                isDouble ? "вещественное число" : "целое число",
                lexeme,
                startIndex,
                startLine,
                startColumn);
        }

        private void ReadString(string text, LexerResult result, ref int index, ref int line, ref int column)
        {
            int startIndex = index;
            int startLine = line;
            int startColumn = column;

            Advance(text, ref index, ref line, ref column);

            while (index < text.Length)
            {
                char current = Peek(text, index);

                if (current == '"')
                {
                    Advance(text, ref index, ref line, ref column);

                    string lexeme = text.Substring(startIndex, index - startIndex);

                    AddToken(result, LexerTokenCode.String, "строковый литерал", lexeme, startIndex, startLine, startColumn);
                    return;
                }

                if (IsLineBreak(current))
                {
                    string fragment = text.Substring(startIndex, index - startIndex);

                    AddError(
                        result,
                        "Не закрыт строковый литерал",
                        fragment,
                        startIndex,
                        startLine,
                        startColumn,
                        Math.Max(fragment.Length, 1));

                    return;
                }

                Advance(text, ref index, ref line, ref column);
            }

            string unfinished = text.Substring(startIndex, index - startIndex);

            AddError(
                result,
                "Не закрыт строковый литерал",
                unfinished,
                startIndex,
                startLine,
                startColumn,
                Math.Max(unfinished.Length, 1));
        }

        private void ReadChar(string text, LexerResult result, ref int index, ref int line, ref int column)
        {
            int startIndex = index;
            int startLine = line;
            int startColumn = column;

            Advance(text, ref index, ref line, ref column);

            if (index >= text.Length || IsLineBreak(Peek(text, index)))
            {
                AddError(
                    result,
                    "Не завершён символьный литерал",
                    "'",
                    startIndex,
                    startLine,
                    startColumn,
                    1);

                return;
            }

            if (Peek(text, index) == '\'')
            {
                Advance(text, ref index, ref line, ref column);

                AddError(
                    result,
                    "Пустой символьный литерал",
                    "''",
                    startIndex,
                    startLine,
                    startColumn,
                    2);

                return;
            }

            Advance(text, ref index, ref line, ref column);

            if (index < text.Length && Peek(text, index) == '\'')
            {
                Advance(text, ref index, ref line, ref column);

                string lexeme = text.Substring(startIndex, index - startIndex);

                AddToken(result, LexerTokenCode.Char, "символьный литерал", lexeme, startIndex, startLine, startColumn);
                return;
            }

            while (index < text.Length && !IsLineBreak(Peek(text, index)) && Peek(text, index) != '\'')
            {
                Advance(text, ref index, ref line, ref column);
            }

            if (index < text.Length && Peek(text, index) == '\'')
            {
                Advance(text, ref index, ref line, ref column);
            }

            string invalid = text.Substring(startIndex, index - startIndex);

            AddError(
                result,
                "Символьный литерал должен содержать ровно один символ",
                invalid,
                startIndex,
                startLine,
                startColumn,
                Math.Max(invalid.Length, 1));
        }

        private void SkipWhitespace(string text, ref int index, ref int line, ref int column)
        {
            while (index < text.Length && char.IsWhiteSpace(Peek(text, index)))
            {
                Advance(text, ref index, ref line, ref column);
            }
        }

        private void AddSingleCharToken(
            LexerResult result,
            LexerTokenCode code,
            string typeName,
            string lexeme,
            ref int index,
            ref int line,
            ref int column,
            string text)
        {
            int startIndex = index;
            int startLine = line;
            int startColumn = column;

            Advance(text, ref index, ref line, ref column);

            AddToken(result, code, typeName, lexeme, startIndex, startLine, startColumn);
        }

        private void AddToken(
            LexerResult result,
            LexerTokenCode code,
            string typeName,
            string lexeme,
            int startIndex,
            int line,
            int startColumn)
        {
            result.Items.Add(new LexerItem
            {
                IsError = false,
                Code = (int)code,
                TypeName = typeName,
                Lexeme = lexeme,
                Message = null,
                Line = line,
                StartColumn = startColumn,
                EndColumn = startColumn + lexeme.Length - 1,
                AbsoluteIndex = startIndex
            });
        }

        private void AddError(
            LexerResult result,
            string message,
            string lexeme,
            int startIndex,
            int line,
            int startColumn,
            int length)
        {
            result.Items.Add(new LexerItem
            {
                IsError = true,
                Code = null,
                TypeName = "Ошибка",
                Lexeme = lexeme,
                Message = message,
                Line = line,
                StartColumn = startColumn,
                EndColumn = startColumn + Math.Max(length, 1) - 1,
                AbsoluteIndex = startIndex
            });
        }

        private bool IsIdentifierStart(char c)
        {
            return char.IsLetter(c) || c == '_';
        }

        private bool IsIdentifierPart(char c)
        {
            return char.IsLetterOrDigit(c) || c == '_';
        }

        private bool IsLineBreak(char c)
        {
            return c == '\r' || c == '\n';
        }

        private char Peek(string text, int index)
        {
            if (index < 0 || index >= text.Length)
                return '\0';

            return text[index];
        }

        private void Advance(string text, ref int index, ref int line, ref int column)
        {
            if (index >= text.Length)
                return;

            char current = text[index];

            if (current == '\r')
            {
                index++;

                if (index < text.Length && text[index] == '\n')
                    index++;

                line++;
                column = 1;
                return;
            }

            if (current == '\n')
            {
                index++;
                line++;
                column = 1;
                return;
            }

            index++;
            column++;
        }

        private string DescribeChar(char c)
        {
            switch (c)
            {
                case '\t':
                    return "\\t";
                case '\r':
                    return "\\r";
                case '\n':
                    return "\\n";
                case '\'':
                    return "\\'";
                case '\"':
                    return "\\\"";
                default:
                    return c.ToString();
            }
        }
    }
}