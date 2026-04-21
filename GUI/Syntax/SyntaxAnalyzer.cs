using System.Collections.Generic;
using GUI.Lexer;

namespace GUI.Syntax
{
    public sealed class SyntaxAnalyzer
    {
        private SyntaxTokenStream _stream;
        private SyntaxResult _result;
        private int _currentDeclarationLine;
        private int _declarationStartPosition;
        private int _parenBalance;
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
            if (_stream.IsAtEnd)
                return;

            _currentDeclarationLine = _stream.Current.Line;
            _declarationStartPosition = _stream.Position;
            _parenBalance = 0;

            if (!_stream.Check(LexerTokenCode.Val))
            {
                AddError("Ожидалось ключевое слово val");

                if (_stream.IsAtEnd)
                    return;

                if (_stream.Check(LexerTokenCode.Semicolon))
                {
                    _stream.Advance();
                    return;
                }


                if (_stream.Check(LexerTokenCode.Identifier) &&
                    _stream.CheckNext(LexerTokenCode.Identifier))
                {
                    _stream.Advance();
                }
                else if (!_stream.Check(LexerTokenCode.Identifier))
                {
                    SkipGarbageLineOrStatement();
                    return;
                }
            }
            else
            {
                _stream.Advance();
            }

            if (!_stream.Check(LexerTokenCode.Identifier))
                AddMissingAfterPrevious(
                    "Ожидался идентификатор после val",
                    "(пропущен идентификатор)");
            else
                _stream.Advance();

            if (!_stream.Check(LexerTokenCode.Assign))
                AddMissingAfterPrevious(
                    "Ожидался оператор присваивания =",
                    "(пропущен =)");
            else
                _stream.Advance();

            if (!_stream.Check(LexerTokenCode.ListOf))
                AddMissingAfterPrevious(
                    "Ожидалась лексема listOf",
                    "(пропущен listOf)");
            else
                _stream.Advance();

            if (!_stream.Check(LexerTokenCode.LeftParen))
            {
                AddMissingAfterPrevious(
                    "Ожидалась открывающая круглая скобка (",
                    "(пропущена ()");
                _parenBalance++;
            }
            else
            {
                _stream.Advance();
                _parenBalance++;
            }

            ParseElementsOpt();

            if (_stream.Check(LexerTokenCode.RightParen))
            {
                _stream.Advance();
                _parenBalance--;
            }
            else
            {
                AddMissingAfterPrevious(
                    "Ожидалась закрывающая круглая скобка )",
                    "(пропущена ))");
                _parenBalance--;
            }

            while (_stream.Check(LexerTokenCode.RightParen))
            {
                AddError("Лишняя закрывающая круглая скобка )");
                _stream.Advance();
            }

            if (_parenBalance != 0)
                _parenBalance = 0;

            if (!_stream.Check(LexerTokenCode.Semicolon))
                AddMissingAfterPrevious(
                    "Ожидался символ ; в конце объявления",
                    "(пропущен ;)");
            else
                _stream.Advance();
        }
        private bool TryConsumeUnexpectedOpenParen()
        {
            if (!_stream.Check(LexerTokenCode.LeftParen))
                return false;

            AddError("Лишняя открывающая круглая скобка (");
            _stream.Advance();
            return true;
        }

        private void AddMissingAtCurrentOrAfterPrevious(string message)
        {
            AddMissingAtCurrentOrAfterPrevious(message, "(пропуск)");
        }

        private void AddMissingAtCurrentOrAfterPrevious(string message, string fragmentText)
        {
            if (_stream.IsAtEnd || _stream.Position > _declarationStartPosition)
            {
                AddMissingAfterPrevious(message, fragmentText);
                return;
            }

            LexerItem token = _stream.Current;

            _result.Errors.Add(new SyntaxError
            {
                InvalidFragment = fragmentText,
                Line = token.Line,
                StartColumn = token.StartColumn,
                EndColumn = token.StartColumn,
                AbsoluteIndex = token.AbsoluteIndex,
                Message = message
            });
        }

        private void ParseElementsOpt()
        {
            while (TryConsumeUnexpectedOpenParen())
            {
            }

            if (_stream.Check(LexerTokenCode.RightParen))
                return;

            if (_stream.IsAtEnd ||
                _stream.Check(LexerTokenCode.Semicolon) ||
                IsNextDeclarationStart() ||
                _stream.Current.Line != _currentDeclarationLine)
            {
                return;
            }

            ParseElements();
        }

        private void ParseElements()
        {
            bool expectElement = true;
            bool commaAfterRealElement = false;

            while (true)
            {
                while (TryConsumeUnexpectedOpenParen())
                {
                }

                if (_stream.Check(LexerTokenCode.RightParen))
                {
                    if (expectElement && commaAfterRealElement)
                    {
                        AddMissingAfterPrevious(
                            "Ожидался элемент списка после запятой",
                            "(пропущен элемент)");
                    }

                    return;
                }

                if (_stream.IsAtEnd ||
                    _stream.Check(LexerTokenCode.Semicolon) ||
                    IsNextDeclarationStart() ||
                    _stream.Current.Line != _currentDeclarationLine)
                {
                    if (expectElement && commaAfterRealElement)
                    {
                        AddMissingAfterPrevious(
                            "Ожидался элемент списка после запятой",
                            "(пропущен элемент)");
                    }

                    return;
                }

                if (expectElement)
                {
                    if (IsElementStart())
                    {
                        ParseElement();
                        expectElement = false;
                        commaAfterRealElement = false;
                        continue;
                    }

                    if (_stream.Check(LexerTokenCode.Comma))
                    {
                        AddError("Ожидался элемент списка");
                        _stream.Advance();
                        expectElement = true;
                        commaAfterRealElement = false;
                        continue;
                    }

                    AddError("Ожидался элемент списка");
                    RecoverWithinDeclaration(
                        LexerTokenCode.Comma,
                        LexerTokenCode.RightParen,
                        LexerTokenCode.Semicolon);

                    if (_stream.Match(LexerTokenCode.Comma))
                    {
                        expectElement = true;
                        commaAfterRealElement = false;
                        continue;
                    }

                    return;
                }

                if (_stream.Match(LexerTokenCode.Comma))
                {
                    expectElement = true;
                    commaAfterRealElement = true;
                    continue;
                }

                if (IsElementStart())
                {
                    AddMissingAfterPrevious(
                        "Ожидалась запятая между элементами списка",
                        "(пропущена запятая)");
                    ParseElement();
                    expectElement = false;
                    commaAfterRealElement = false;
                    continue;
                }

                return;
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

            if (_stream.Check(LexerTokenCode.Int) ||
                _stream.Check(LexerTokenCode.Double) ||
                _stream.Check(LexerTokenCode.Plus) ||
                _stream.Check(LexerTokenCode.Minus))
            {
                ParseNumberLiteral();
                return;
            }

            if (TryConsumeUnexpectedOpenParen())
                return;

            if (_stream.IsAtEnd ||
                _stream.Check(LexerTokenCode.Comma) ||
                _stream.Check(LexerTokenCode.RightParen) ||
                _stream.Check(LexerTokenCode.Semicolon))
            {
                AddMissingAtCurrentOrAfterPrevious(
                    "Ожидался элемент списка",
                    "(пропущен элемент)");
                return;
            }

            AddError("Ожидался элемент списка");
            RecoverWithinDeclaration(
                LexerTokenCode.Comma,
                LexerTokenCode.RightParen,
                LexerTokenCode.Semicolon);
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

            if (_stream.IsAtEnd ||
                _stream.Check(LexerTokenCode.Comma) ||
                _stream.Check(LexerTokenCode.RightParen) ||
                _stream.Check(LexerTokenCode.Semicolon))
            {
                AddMissingAfterPrevious(
                    "После знака ожидался int или double",
                    "(пропущено число)");
                return;
            }

            AddError("После знака ожидался int или double");
            RecoverWithinDeclaration(
                LexerTokenCode.Comma,
                LexerTokenCode.RightParen,
                LexerTokenCode.Semicolon);
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