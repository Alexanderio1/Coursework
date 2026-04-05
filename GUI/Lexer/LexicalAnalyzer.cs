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
                        AddSingleCharToken(
                            result,
                            LexerTokenCode.Assign,
                            "оператор присваивания",
                            "=",
                            ref index,
                            ref line,
                            ref column,
                            text);
                        break;

                    case '(':
                        AddSingleCharToken(
                            result,
                            LexerTokenCode.LeftParen,
                            "открывающая круглая скобка",
                            "(",
                            ref index,
                            ref line,
                            ref column,
                            text);
                        break;

                    case ',':
                        AddSingleCharToken(
                            result,
                            LexerTokenCode.Comma,
                            "разделитель (запятая)",
                            ",",
                            ref index,
                            ref line,
                            ref column,
                            text);
                        break;

                    case ')':
                        AddSingleCharToken(
                            result,
                            LexerTokenCode.RightParen,
                            "закрывающая круглая скобка",
                            ")",
                            ref index,
                            ref line,
                            ref column,
                            text);
                        break;

                    case '"':
                        ReadString(text, result, ref index, ref line, ref column);
                        break;

                    case ';':
                        AddSingleCharToken(
                            result,
                            LexerTokenCode.Semicolon,
                            "конец оператора",
                            ";",
                            ref index,
                            ref line,
                            ref column,
                            text);
                        break;

                    case '\'':
                        ReadChar(text, result, ref index, ref line, ref column);
                        break;

                    case '-':
                        AddSingleCharToken(
                            result,
                            LexerTokenCode.Minus,
                            "знак минус",
                            "-",
                            ref index,
                            ref line,
                            ref column,
                            text);
                        break;

                    case '+':
                        AddSingleCharToken(
                            result,
                            LexerTokenCode.Plus,
                            "знак плюс",
                            "+",
                            ref index,
                            ref line,
                            ref column,
                            text);
                        break;

                    default:
                        ReadInvalidSequence(text, result, ref index, ref line, ref column);
                        break;
                }
            }

            return result;
        }

        private void ReadInvalidSequence(string text, LexerResult result, ref int index, ref int line, ref int column)
        {
            int startIndex = index;
            int startLine = line;
            int startColumn = column;

            while (index < text.Length)
            {
                char current = Peek(text, index);

                if (char.IsWhiteSpace(current) || IsStartOfValidToken(current))
                    break;

                Advance(text, ref index, ref line, ref column);
            }

            string invalidLexeme = text.Substring(startIndex, index - startIndex);

            if (string.IsNullOrEmpty(invalidLexeme))
            {
                char current = Peek(text, index);
                invalidLexeme = current.ToString();
                Advance(text, ref index, ref line, ref column);
            }

            AddError(
                result,
                invalidLexeme.Length == 1
                    ? "Недопустимый символ '" + DescribeChar(invalidLexeme[0]) + "'"
                    : "Недопустимый фрагмент \"" + invalidLexeme + "\"",
                invalidLexeme,
                startIndex,
                startLine,
                startColumn,
                Math.Max(invalidLexeme.Length, 1));
        }

        private bool IsStartOfValidToken(char c)
        {
            return c == '\0'
                || IsIdentifierStart(c)
                || char.IsDigit(c)
                || c == '='
                || c == '('
                || c == ','
                || c == ')'
                || c == ';'
                || c == '+'
                || c == '-'
                || c == '"'
                || c == '\'';
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

            if (lexeme == "val")
            {
                code = LexerTokenCode.Val;
                typeName = "ключевое слово";
            }
            else if (lexeme == "listOf")
            {
                code = LexerTokenCode.ListOf;
                typeName = "специальная лексема";
            }
            else if (lexeme == "true")
            {
                code = LexerTokenCode.True;
                typeName = "булев литерал";
            }
            else if (lexeme == "false")
            {
                code = LexerTokenCode.False;
                typeName = "булев литерал";
            }
            else
            {
                code = LexerTokenCode.Identifier;
                typeName = "идентификатор";
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

            if (Peek(text, index) == '.')
            {
                Advance(text, ref index, ref line, ref column);

                if (!char.IsDigit(Peek(text, index)))
                {
                    SkipToDelimiter(text, ref index, ref line, ref column);

                    string invalidLexeme = text.Substring(startIndex, index - startIndex);

                    AddError(
                        result,
                        "Некорректная запись вещественного числа",
                        invalidLexeme,
                        startIndex,
                        startLine,
                        startColumn,
                        Math.Max(invalidLexeme.Length, 1));

                    return;
                }

                while (char.IsDigit(Peek(text, index)))
                {
                    Advance(text, ref index, ref line, ref column);
                }

                if (IsInvalidNumberTail(Peek(text, index)))
                {
                    SkipToDelimiter(text, ref index, ref line, ref column);

                    string invalidLexeme = text.Substring(startIndex, index - startIndex);

                    AddError(
                        result,
                        "Некорректная запись вещественного числа",
                        invalidLexeme,
                        startIndex,
                        startLine,
                        startColumn,
                        Math.Max(invalidLexeme.Length, 1));

                    return;
                }

                string doubleLexeme = text.Substring(startIndex, index - startIndex);

                AddToken(
                    result,
                    LexerTokenCode.Double,
                    "вещественное число",
                    doubleLexeme,
                    startIndex,
                    startLine,
                    startColumn);

                return;
            }

            if (IsInvalidNumberTail(Peek(text, index)))
            {
                SkipToDelimiter(text, ref index, ref line, ref column);

                string invalidLexeme = text.Substring(startIndex, index - startIndex);

                AddError(
                    result,
                    "Некорректная запись целого числа",
                    invalidLexeme,
                    startIndex,
                    startLine,
                    startColumn,
                    Math.Max(invalidLexeme.Length, 1));

                return;
            }

            string intLexeme = text.Substring(startIndex, index - startIndex);

            AddToken(
                result,
                LexerTokenCode.Int,
                "целое число",
                intLexeme,
                startIndex,
                startLine,
                startColumn);
        }

        private void ReadString(string text, LexerResult result, ref int index, ref int line, ref int column)
        {
            int startIndex = index;
            int startLine = line;
            int startColumn = column;

            Advance(text, ref index, ref line, ref column); // открывающая "

            while (index < text.Length)
            {
                char current = Peek(text, index);

                if (current == '"')
                {
                    Advance(text, ref index, ref line, ref column);

                    string lexeme = text.Substring(startIndex, index - startIndex);

                    AddToken(
                        result,
                        LexerTokenCode.String,
                        "строковый литерал",
                        lexeme,
                        startIndex,
                        startLine,
                        startColumn);

                    return;
                }

                if (IsAllowedStringChar(current))
                {
                    Advance(text, ref index, ref line, ref column);
                    continue;
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

                // Недопустимый символ внутри строки:
                // дочитываем до закрывающей " или конца строки,
                // чтобы не оставлять хвостовую кавычку на повторный разбор.
                while (index < text.Length
                       && !IsLineBreak(Peek(text, index))
                       && Peek(text, index) != '"')
                {
                    Advance(text, ref index, ref line, ref column);
                }

                if (Peek(text, index) == '"')
                {
                    Advance(text, ref index, ref line, ref column);
                }

                string invalid = text.Substring(startIndex, index - startIndex);

                AddError(
                    result,
                    "Недопустимый символ в строковом литерале",
                    invalid,
                    startIndex,
                    startLine,
                    startColumn,
                    Math.Max(invalid.Length, 1));

                return;
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

            Advance(text, ref index, ref line, ref column); // открывающая '

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

            // Пустой символьный литерал: ''
            if (Peek(text, index) == '\'')
            {
                Advance(text, ref index, ref line, ref column);

                string invalidLexeme = text.Substring(startIndex, index - startIndex);

                AddError(
                    result,
                    "Символьный литерал должен содержать ровно один символ",
                    invalidLexeme,
                    startIndex,
                    startLine,
                    startColumn,
                    Math.Max(invalidLexeme.Length, 1));

                return;
            }

            char current = Peek(text, index);

            if (!IsAllowedCharLiteralChar(current))
            {
                Advance(text, ref index, ref line, ref column);

                while (index < text.Length
                       && !IsLineBreak(Peek(text, index))
                       && Peek(text, index) != '\'')
                {
                    Advance(text, ref index, ref line, ref column);
                }

                if (Peek(text, index) == '\'')
                {
                    Advance(text, ref index, ref line, ref column);
                }

                string invalidLexeme = text.Substring(startIndex, index - startIndex);

                AddError(
                    result,
                    "Недопустимый символ в символьном литерале",
                    invalidLexeme,
                    startIndex,
                    startLine,
                    startColumn,
                    Math.Max(invalidLexeme.Length, 1));

                return;
            }

            Advance(text, ref index, ref line, ref column);

            if (Peek(text, index) == '\'')
            {
                Advance(text, ref index, ref line, ref column);

                string lexeme = text.Substring(startIndex, index - startIndex);

                AddToken(
                    result,
                    LexerTokenCode.Char,
                    "символьный литерал",
                    lexeme,
                    startIndex,
                    startLine,
                    startColumn);

                return;
            }

            if (index >= text.Length || IsLineBreak(Peek(text, index)))
            {
                string invalid = text.Substring(startIndex, index - startIndex);

                AddError(
                    result,
                    "Не завершён символьный литерал",
                    invalid,
                    startIndex,
                    startLine,
                    startColumn,
                    Math.Max(invalid.Length, 1));

                return;
            }

            while (index < text.Length
                   && !IsLineBreak(Peek(text, index))
                   && Peek(text, index) != '\'')
            {
                Advance(text, ref index, ref line, ref column);
            }

            if (Peek(text, index) == '\'')
            {
                Advance(text, ref index, ref line, ref column);
            }

            string invalidMulti = text.Substring(startIndex, index - startIndex);

            AddError(
                result,
                "Символьный литерал должен содержать ровно один символ",
                invalidMulti,
                startIndex,
                startLine,
                startColumn,
                Math.Max(invalidMulti.Length, 1));
        }

        private void SkipWhitespace(string text, ref int index, ref int line, ref int column)
        {
            while (index < text.Length && char.IsWhiteSpace(Peek(text, index)))
            {
                Advance(text, ref index, ref line, ref column);
            }
        }

        private void SkipToDelimiter(string text, ref int index, ref int line, ref int column)
        {
            while (!IsTokenDelimiter(Peek(text, index)))
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
            return IsLetter(c) || c == '_';
        }

        private bool IsIdentifierPart(char c)
        {
            return IsLetter(c) || char.IsDigit(c) || c == '_';
        }

        private bool IsAllowedStringChar(char c)
        {
            return IsLetter(c) || char.IsDigit(c) || c == '_' || c == ' ';
        }

        private bool IsAllowedCharLiteralChar(char c)
        {
            return IsLetter(c) || char.IsDigit(c) || c == '_' || c == ' ';
        }

        private bool IsInvalidNumberTail(char c)
        {
            return IsLetter(c) || c == '_' || c == '.';
        }

        private bool IsTokenDelimiter(char c)
        {
            return c == '\0'
                   || char.IsWhiteSpace(c)
                   || c == '='
                   || c == '('
                   || c == ','
                   || c == ')'
                   || c == ';'
                   || c == '+'
                   || c == '-'
                   || c == '"'
                   || c == '\'';
        }

        private bool IsLineBreak(char c)
        {
            return c == '\r' || c == '\n';
        }

        private bool IsLetter(char c)
        {
            return char.IsLetter(c);
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
                case '\t': return "\\t";
                case '\r': return "\\r";
                case '\n': return "\\n";
                case '\'': return "\\'";
                case '\"': return "\\\"";
                default: return c.ToString();
            }
        }
    }
}