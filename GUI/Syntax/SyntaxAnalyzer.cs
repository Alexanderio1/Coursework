using System.Collections.Generic;
using GUI.Lexer;

namespace GUI.Syntax
{
    public sealed class SyntaxAnalyzer
    {
        private SyntaxTokenStream _stream;
        private SyntaxResult _result;
        private int _currentDeclarationLine;
        private bool _elementsExpectedReported;

        public SyntaxResult Parse(IReadOnlyList<LexerItem> tokens)
        {
            _stream = new SyntaxTokenStream(tokens);
            _result = new SyntaxResult();

            if (tokens == null || tokens.Count == 0)
            {
                AddErrorFromEmptyInput("Ожидалось объявление списка с инициализацией.");
                return _result;
            }

            while (!_stream.IsAtEnd)
            {
                int startPosition = _stream.Position;

                ParseDeclaration();

                _stream.Match(LexerTokenCode.Semicolon);

                if (!_stream.IsAtEnd && _stream.Position == startPosition)
                    _stream.Advance();
            }

            return _result;
        }

        private void ParseDeclaration()
        {
            _elementsExpectedReported = false;

            if (_stream.IsAtEnd)
                return;

            _currentDeclarationLine = _stream.Current.Line;

            if (!_stream.Check(LexerTokenCode.Val))
            {
                AddError("Ожидалось ключевое слово val");
                SkipGarbageLineOrStatement();
                return;
            }
            _stream.Advance();

            if (!_stream.Check(LexerTokenCode.Identifier))
            {
                AddMissingAfterPrevious(
                    "Ожидался идентификатор после val",
                    "(пропущен идентификатор)");

                if (!RecoverWithinDeclaration(
                    LexerTokenCode.Identifier,
                    LexerTokenCode.Assign,
                    LexerTokenCode.ListOf,
                    LexerTokenCode.LeftParen,
                    LexerTokenCode.RightParen))
                    return;
            }

            if (_stream.Check(LexerTokenCode.Identifier))
                _stream.Advance();

            if (!_stream.Check(LexerTokenCode.Assign))
            {
                AddMissingAfterPrevious("Ожидался оператор присваивания =");

                if (!RecoverWithinDeclaration(
                    LexerTokenCode.Assign,
                    LexerTokenCode.ListOf,
                    LexerTokenCode.LeftParen,
                    LexerTokenCode.RightParen))
                    return;
            }

            if (_stream.Check(LexerTokenCode.Assign))
                _stream.Advance();

            if (!_stream.Check(LexerTokenCode.ListOf))
            {
                AddMissingAfterPrevious("Ожидалась лексема listOf");

                if (!RecoverWithinDeclaration(
                    LexerTokenCode.ListOf,
                    LexerTokenCode.LeftParen,
                    LexerTokenCode.RightParen))
                    return;
            }

            if (_stream.Check(LexerTokenCode.ListOf))
                _stream.Advance();

            if (!_stream.Check(LexerTokenCode.LeftParen))
            {
                AddMissingAfterPrevious("Ожидалась открывающая круглая скобка (");

                if (!RecoverWithinDeclaration(
                    LexerTokenCode.LeftParen,
                    LexerTokenCode.RightParen))
                    return;
            }

            if (_stream.Check(LexerTokenCode.LeftParen))
            {
                _stream.Advance();
                ParseElementsOpt();
            }

            if (!_stream.Check(LexerTokenCode.RightParen))
            {
                if (!_elementsExpectedReported)
                {
                    if (_stream.IsAtEnd
                        || IsNextDeclarationStart()
                        || _stream.Current.Line != _currentDeclarationLine
                        || _stream.Check(LexerTokenCode.Semicolon))
                    {
                        AddMissingAfterPrevious(
                            "Ожидалась закрывающая круглая скобка )",
                            "(пропущена ))");
                    }
                    else
                    {
                        AddError("Ожидалась закрывающая круглая скобка )");
                    }
                }

                if (!RecoverWithinDeclaration(LexerTokenCode.RightParen, LexerTokenCode.Semicolon))
                    return;
            }

            if (_stream.Check(LexerTokenCode.RightParen))
                _stream.Advance();

            if (!_stream.Check(LexerTokenCode.Semicolon))
            {
                AddMissingAfterPrevious(
                    "Ожидался символ ; в конце объявления",
                    "(пропущен ;)");

                if (!RecoverWithinDeclaration(LexerTokenCode.Semicolon))
                    return;
            }

            if (_stream.Check(LexerTokenCode.Semicolon))
                _stream.Advance();
        }

        private void ParseElementsOpt()
        {
            if (_stream.Check(LexerTokenCode.RightParen))
                return;

            if (IsElementStart())
            {
                ParseElements();
                return;
            }

            _elementsExpectedReported = true;

            if (_stream.IsAtEnd
                || IsNextDeclarationStart()
                || _stream.Current.Line != _currentDeclarationLine
                || _stream.Check(LexerTokenCode.Semicolon))
            {
                AddMissingAfterPrevious(
                    "Ожидался элемент списка или символ )",
                    "(пропущено содержимое списка)");
                return;
            }

            AddError("Ожидался элемент списка или символ )");
            RecoverWithinDeclaration(LexerTokenCode.RightParen, LexerTokenCode.Semicolon);
        }

        private void ParseElements()
        {
            ParseElement();

            while (true)
            {
                if (_stream.Match(LexerTokenCode.Comma))
                {
                    ParseElement();
                    continue;
                }

                if (IsElementStart())
                {
                    AddMissingAfterPrevious(
                        "Ожидалась запятая между элементами списка",
                        "(пропущена запятая)");
                    ParseElement();
                    continue;
                }

                break;
            }
        }

        private void ParseElement()
        {
            if (_stream.Match(LexerTokenCode.String))
                return;

            if (_stream.Match(LexerTokenCode.Char))
                return;

            if (_stream.Match(LexerTokenCode.True))
                return;

            if (_stream.Match(LexerTokenCode.False))
                return;

            if (_stream.Check(LexerTokenCode.Int)
                || _stream.Check(LexerTokenCode.Double)
                || _stream.Check(LexerTokenCode.Plus)
                || _stream.Check(LexerTokenCode.Minus))
            {
                ParseNumberLiteral();
                return;
            }

            AddError("Ожидался элемент списка");
            RecoverWithinDeclaration(LexerTokenCode.Comma, LexerTokenCode.RightParen, LexerTokenCode.Semicolon);
        }

        private void ParseNumberLiteral()
        {
            if (_stream.Match(LexerTokenCode.Int))
                return;

            if (_stream.Match(LexerTokenCode.Double))
                return;

            if (_stream.Check(LexerTokenCode.Plus) || _stream.Check(LexerTokenCode.Minus))
            {
                ParseSignedNumber();
                return;
            }

            AddError("Ожидался числовой литерал");
            RecoverWithinDeclaration(LexerTokenCode.Comma, LexerTokenCode.RightParen, LexerTokenCode.Semicolon);
        }

        private void ParseSignedNumber()
        {
            ParseSign();

            if (_stream.Match(LexerTokenCode.Int))
                return;

            if (_stream.Match(LexerTokenCode.Double))
                return;

            AddError("После знака ожидался int или double");
            RecoverWithinDeclaration(LexerTokenCode.Comma, LexerTokenCode.RightParen, LexerTokenCode.Semicolon);
        }

        private void ParseSign()
        {
            if (_stream.Match(LexerTokenCode.Plus))
                return;

            if (_stream.Match(LexerTokenCode.Minus))
                return;

            AddError("Ожидался знак + или -");
        }

        private void AddError(string message)
        {
            LexerItem token = _stream.IsAtEnd ? _stream.Previous : _stream.Current;

            _result.Errors.Add(new SyntaxError
            {
                InvalidFragment = token.Lexeme ?? string.Empty,
                Line = token.Line,
                StartColumn = token.StartColumn,
                EndColumn = token.EndColumn,
                AbsoluteIndex = token.AbsoluteIndex,
                Message = message
            });
        }

        private void AddErrorFromEmptyInput(string message)
        {
            _result.Errors.Add(new SyntaxError
            {
                InvalidFragment = string.Empty,
                Line = 1,
                StartColumn = 1,
                EndColumn = 1,
                AbsoluteIndex = 0,
                Message = message
            });
        }

        private bool IsElementStart()
        {
            return _stream.Check(LexerTokenCode.String)
                || _stream.Check(LexerTokenCode.Char)
                || _stream.Check(LexerTokenCode.True)
                || _stream.Check(LexerTokenCode.False)
                || _stream.Check(LexerTokenCode.Int)
                || _stream.Check(LexerTokenCode.Double)
                || _stream.Check(LexerTokenCode.Plus)
                || _stream.Check(LexerTokenCode.Minus);
        }

        private void AddMissingAfterPrevious(string message)
        {
            AddMissingAfterPrevious(message, "(пропуск)");
        }

        private int CurrentCodeOrMinusOne()
        {
            return !_stream.IsAtEnd && _stream.Current.Code.HasValue
                ? _stream.Current.Code.Value
                : -1;
        }

        private bool IsNextDeclarationStart()
        {
            return !_stream.IsAtEnd
                && _stream.Check(LexerTokenCode.Val)
                && _stream.Current.Line != _currentDeclarationLine;
        }

        private void SkipGarbageLineOrStatement()
        {
            int startLine = !_stream.IsAtEnd ? _stream.Current.Line : _currentDeclarationLine;

            while (!_stream.IsAtEnd)
            {
                if (_stream.Check(LexerTokenCode.Semicolon))
                {
                    _stream.Advance();
                    return;
                }

                if (_stream.Current.Line != startLine)
                    return;

                _stream.Advance();
            }
        }

        private bool RecoverWithinDeclaration(params LexerTokenCode[] anchors)
        {
            HashSet<int> anchorSet = new HashSet<int>();

            foreach (LexerTokenCode anchor in anchors)
                anchorSet.Add((int)anchor);

            while (!_stream.IsAtEnd)
            {
                int currentCode = CurrentCodeOrMinusOne();

                if (anchorSet.Contains(currentCode))
                    return true;

                if (_stream.Check(LexerTokenCode.Semicolon))
                    return false;

                if (IsNextDeclarationStart())
                    return false;

                if (_stream.Current.Line != _currentDeclarationLine)
                    return false;

                _stream.Advance();
            }

            return false;
        }

        private void AddMissingAfterPrevious(string message, string fragmentText)
        {
            LexerItem token = _stream.Previous;

            int startColumn = token.EndColumn + 1;
            int absoluteIndex = token.AbsoluteIndex + (token.Lexeme != null ? token.Lexeme.Length : 0);

            _result.Errors.Add(new SyntaxError
            {
                InvalidFragment = fragmentText,
                Line = token.Line,
                StartColumn = startColumn,
                EndColumn = startColumn,
                AbsoluteIndex = absoluteIndex,
                Message = message
            });
        }
    }
}