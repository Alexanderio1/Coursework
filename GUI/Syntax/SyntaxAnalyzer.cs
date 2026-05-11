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

                if (_stream.Check(LexerTokenCode.Semicolon))
                {
                    ReportExtraSemicolonRun();
                    continue;
                }

                if (!LooksLikeDeclarationStart())
                {
                    ReportTopLevelGarbageRun();
                    continue;
                }

                ParseDeclaration();

                if (!_stream.IsAtEnd && _stream.Position == startPosition)
                    _stream.Advance();
            }

            return _result;
        }

        private void ReportExtraSemicolonRun()
        {
            LexerItem first = _stream.Current;
            LexerItem last = _stream.Current;

            System.Text.StringBuilder fragment = new System.Text.StringBuilder();

            while (_stream.Check(LexerTokenCode.Semicolon))
            {
                last = _stream.Current;

                if (!string.IsNullOrEmpty(_stream.Current.Lexeme))
                    fragment.Append(_stream.Current.Lexeme);

                _stream.Advance();
            }

            _result.Errors.Add(new SyntaxError
            {
                InvalidFragment = fragment.ToString(),
                Line = first.Line,
                StartColumn = first.StartColumn,
                EndColumn = last.EndColumn,
                AbsoluteIndex = first.AbsoluteIndex,
                Message = "Лишний символ ; вне объявления"
            });
        }

        private void ReportTopLevelGarbageRun()
        {
            LexerItem first = _stream.Current;
            LexerItem last = _stream.Current;

            System.Text.StringBuilder fragment = new System.Text.StringBuilder();

            while (!_stream.IsAtEnd)
            {
                if (_stream.Check(LexerTokenCode.Val))
                    break;

                if (_stream.Check(LexerTokenCode.Semicolon))
                    break;

                if (_stream.Current.Line != first.Line)
                    break;

                last = _stream.Current;

                if (!string.IsNullOrEmpty(_stream.Current.Lexeme))
                    fragment.Append(_stream.Current.Lexeme);

                _stream.Advance();
            }

            _result.Errors.Add(new SyntaxError
            {
                InvalidFragment = fragment.ToString(),
                Line = first.Line,
                StartColumn = first.StartColumn,
                EndColumn = last.EndColumn,
                AbsoluteIndex = first.AbsoluteIndex,
                Message = "Лишний фрагмент вне объявления"
            });
        }

        private void ParseDeclaration()
        {
            if (_stream.IsAtEnd)
                return;

            if (_stream.Check(LexerTokenCode.Semicolon))
            {
                AddError("Ожидалось объявление списка с инициализацией.");
                _stream.Advance();
                return;
            }

            _currentDeclarationLine = _stream.Current.Line;
            _declarationStartPosition = _stream.Position;

            if (!Expect(
                LexerTokenCode.Val,
                "ключевое слово val",
                "(пропущен val)",
                LexerTokenCode.Identifier,
                LexerTokenCode.Assign,
                LexerTokenCode.ListOf,
                LexerTokenCode.LeftParen,
                LexerTokenCode.Semicolon))
            {
                AbortCurrentDeclaration();
                return;
            }

            if (!Expect(
                LexerTokenCode.Identifier,
                "идентификатор после val",
                "(пропущен идентификатор)",
                LexerTokenCode.Assign,
                LexerTokenCode.ListOf,
                LexerTokenCode.LeftParen,
                LexerTokenCode.Semicolon))
            {
                AbortCurrentDeclaration();
                return;
            }

            if (!Expect(
                LexerTokenCode.Assign,
                "оператор присваивания =",
                "(пропущен =)",
                LexerTokenCode.ListOf,
                LexerTokenCode.LeftParen,
                LexerTokenCode.Semicolon))
            {
                AbortCurrentDeclaration();
                return;
            }

            if (!ExpectListOf())
            {
                AbortCurrentDeclaration();
                return;
            }

            if (!Expect(
                LexerTokenCode.LeftParen,
                "открывающая круглая скобка (",
                "(пропущена ()",
                LexerTokenCode.String,
                LexerTokenCode.Char,
                LexerTokenCode.True,
                LexerTokenCode.False,
                LexerTokenCode.Int,
                LexerTokenCode.Double,
                LexerTokenCode.Plus,
                LexerTokenCode.Minus,
                LexerTokenCode.RightParen,
                LexerTokenCode.Semicolon))
            {
                AbortCurrentDeclaration();
                return;
            }

            ParseElementsOpt();

            if (!Expect(
                LexerTokenCode.RightParen,
                "закрывающая круглая скобка )",
                "(пропущена ))",
                LexerTokenCode.Semicolon,
                LexerTokenCode.Val))
            {
                AbortCurrentDeclaration();
                return;
            }

            while (_stream.Check(LexerTokenCode.RightParen))
            {
                AddError("Лишняя закрывающая круглая скобка )");
                _stream.Advance();
            }

            if (!Expect(
                LexerTokenCode.Semicolon,
                "символ ; в конце объявления",
                "(пропущен ;)",
                LexerTokenCode.Val))
            {
                AbortCurrentDeclaration();
                return;
            }
        }

        private bool LooksLikeDeclarationStart()
        {
            if (_stream.IsAtEnd)
                return false;

            if (_stream.Check(LexerTokenCode.Val))
                return true;

            if (_stream.Check(LexerTokenCode.Identifier))
            {
                if (CurrentIdentifierLooksLikeVal())
                    return true;

                if (IdentifierLooksLikeDeclarationStart())
                    return true;

                if (LooksLikeMistypedValBeforeIdentifier())
                    return true;
            }

            if (_stream.Check(LexerTokenCode.Invalid))
            {
                LexerItem next = _stream.Peek(1);

                if (next != null && next.Code == (int)LexerTokenCode.Identifier)
                    return true;
            }

            return false;
        }

        private bool CurrentIdentifierLooksLikeVal()
        {
            if (!_stream.Check(LexerTokenCode.Identifier))
                return false;

            return IsNearVal(_stream.Current.Lexeme);
        }

        private bool LooksLikeMistypedValBeforeIdentifier()
        {
            if (!_stream.Check(LexerTokenCode.Identifier))
                return false;

            LexerItem next = _stream.Peek(1);

            if (next == null || next.Code != (int)LexerTokenCode.Identifier)
                return false;

            return IsNearVal(_stream.Current.Lexeme);
        }

        private bool IsNearVal(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            text = text.ToLowerInvariant();

            if (text == "val")
                return true;

            if (text == "vl")
                return true;

            if (text == "va")
                return true;

            if (text == "vla")
                return true;

            if (text == "vall")
                return true;

            if (text == "val1")
                return true;

            return EditDistanceOne(text, "val");
        }

        private bool EditDistanceOne(string a, string b)
        {
            if (a == null || b == null)
                return false;

            if (System.Math.Abs(a.Length - b.Length) > 1)
                return false;

            int i = 0;
            int j = 0;
            int edits = 0;

            while (i < a.Length && j < b.Length)
            {
                if (a[i] == b[j])
                {
                    i++;
                    j++;
                    continue;
                }

                edits++;

                if (edits > 1)
                    return false;

                if (a.Length > b.Length)
                    i++;
                else if (a.Length < b.Length)
                    j++;
                else
                {
                    i++;
                    j++;
                }
            }

            if (i < a.Length || j < b.Length)
                edits++;

            return edits <= 1;
        }

        private bool Expect(
            LexerTokenCode expected,
            string expectedText,
            string missingFragment,
            params LexerTokenCode[] followers)
        {
            if (_stream.Match(expected))
                return true;

            if (expected == LexerTokenCode.Val &&
                _stream.Check(LexerTokenCode.Identifier) &&
                CurrentIdentifierLooksLikeVal())
            {
                AddError("Ожидалось ключевое слово val");
                _stream.Advance();
                return true;
            }

            if (_stream.IsAtEnd)
            {
                AddMissingAfterPrevious(
                    "Ожидался " + expectedText,
                    missingFragment);
                return true;
            }

            if (ConsumeInvalidToken())
            {
                if (_stream.Match(expected))
                    return true;

                if (_stream.IsAtEnd || IsCurrentOneOf(followers))
                    return true;

                RecoverTo(Combine(expected, followers));

                if (_stream.Match(expected))
                    return true;

                return _stream.IsAtEnd || IsCurrentOneOf(followers);
            }

            if (IsCurrentOneOf(followers))
            {
                AddMissingAtCurrentOrAfterPrevious(
                    "Ожидался " + expectedText,
                    missingFragment);
                return true;
            }

            if (CheckNext(expected))
            {
                AddError("Лишний фрагмент перед " + expectedText);
                _stream.Advance();
                _stream.Match(expected);
                return true;
            }

            AddError("Ожидался " + expectedText);

            RecoverTo(Combine(expected, followers));

            if (_stream.Match(expected))
                return true;

            return _stream.IsAtEnd || IsCurrentOneOf(followers);
        }

        private bool ExpectListOf()
        {
            if (_stream.Match(LexerTokenCode.ListOf))
                return true;

            if (_stream.IsAtEnd)
            {
                AddMissingAfterPrevious(
                    "Ожидалась лексема listOf",
                    "(пропущен listOf)");
                return true;
            }

            if (_stream.Check(LexerTokenCode.Identifier))
            {
                string text = _stream.Current.Lexeme ?? string.Empty;

                if (text == "listOf")
                {
                    _stream.Advance();
                    return true;
                }


                if (IsNearListOf(text))
                {
                    AddError("Ожидалась лексема listOf");
                    _stream.Advance();
                    return true;
                }

                if (CheckNext(LexerTokenCode.LeftParen))
                {
                    AddError("Ожидалась лексема listOf");
                    _stream.Advance();
                    return true;
                }
            }

            if (ConsumeInvalidToken())
            {
                if (_stream.Check(LexerTokenCode.LeftParen))
                    return true;

                if (IsElementStart() || _stream.Check(LexerTokenCode.RightParen))
                    return true;

                RecoverTo(
                    LexerTokenCode.ListOf,
                    LexerTokenCode.LeftParen,
                    LexerTokenCode.String,
                    LexerTokenCode.Char,
                    LexerTokenCode.True,
                    LexerTokenCode.False,
                    LexerTokenCode.Int,
                    LexerTokenCode.Double,
                    LexerTokenCode.Plus,
                    LexerTokenCode.Minus,
                    LexerTokenCode.RightParen,
                    LexerTokenCode.Semicolon,
                    LexerTokenCode.Val);

                if (_stream.Match(LexerTokenCode.ListOf))
                    return true;

                return _stream.Check(LexerTokenCode.LeftParen)
                    || IsElementStart()
                    || _stream.Check(LexerTokenCode.RightParen);
            }

            if (_stream.Check(LexerTokenCode.LeftParen))
            {
                AddMissingAtCurrentOrAfterPrevious(
                    "Ожидалась лексема listOf",
                    "(пропущен listOf)");
                return true;
            }


            if (IsElementStart() || _stream.Check(LexerTokenCode.RightParen))
            {
                AddMissingAtCurrentOrAfterPrevious(
                    "Ожидалась лексема listOf",
                    "(пропущен listOf)");
                return true;
            }

            if (CheckNext(LexerTokenCode.ListOf))
            {
                AddError("Лишний фрагмент перед listOf");
                _stream.Advance();
                _stream.Match(LexerTokenCode.ListOf);
                return true;
            }

            AddError("Ожидалась лексема listOf");

            RecoverTo(
                LexerTokenCode.ListOf,
                LexerTokenCode.LeftParen,
                LexerTokenCode.String,
                LexerTokenCode.Char,
                LexerTokenCode.True,
                LexerTokenCode.False,
                LexerTokenCode.Int,
                LexerTokenCode.Double,
                LexerTokenCode.Plus,
                LexerTokenCode.Minus,
                LexerTokenCode.RightParen,
                LexerTokenCode.Semicolon,
                LexerTokenCode.Val);

            if (_stream.Match(LexerTokenCode.ListOf))
                return true;

            return _stream.Check(LexerTokenCode.LeftParen)
                || IsElementStart()
                || _stream.Check(LexerTokenCode.RightParen);
        }

        private bool IsNearListOf(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            if (text == "listOf")
                return false;

            string lower = text.ToLowerInvariant();

            if (lower == "listof")
                return true;

            if (lower == "lisof")
                return true;

            if (lower == "lstof")
                return true;

            if (lower == "listo")
                return true;

            if (lower == "listoff")
                return true;

            return EditDistanceOne(lower, "listof");
        }

        private void ParseElementsOpt()
        {
            if (_stream.IsAtEnd)
                return;

            if (_stream.Check(LexerTokenCode.RightParen))
                return;

            if (_stream.Check(LexerTokenCode.Semicolon))
                return;

            if (IsNextDeclarationStart())
                return;

            if (_stream.Current.Line != _currentDeclarationLine)
                return;

            ParseElements();
        }

        private void ParseElements()
        {
            bool expectElement = true;
            bool commaAfterRealElement = false;

            while (!_stream.IsAtEnd)
            {
                if (IsNextDeclarationStart())
                    return;

                if (_stream.Current.Line != _currentDeclarationLine)
                    return;

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

                if (_stream.Check(LexerTokenCode.Semicolon))
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
                    if (ConsumeInvalidToken())
                    {
                        expectElement = false;
                        commaAfterRealElement = false;
                        continue;
                    }

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

                if (_stream.Check(LexerTokenCode.Invalid))
                {
                    AddMissingAfterPrevious(
                        "Ожидалась запятая между элементами списка",
                        "(пропущена запятая)");

                    _stream.Advance();

                    expectElement = false;
                    commaAfterRealElement = false;
                    continue;
                }

                if (_stream.Check(LexerTokenCode.LeftParen))
                {
                    AddError("Лишняя открывающая круглая скобка (");
                    _stream.Advance();

                    expectElement = false;
                    commaAfterRealElement = false;
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

                AddError("Ожидалась запятая между элементами списка");

                RecoverWithinDeclaration(
                    LexerTokenCode.Comma,
                    LexerTokenCode.RightParen,
                    LexerTokenCode.Semicolon);

                if (_stream.Match(LexerTokenCode.Comma))
                {
                    expectElement = true;
                    commaAfterRealElement = true;
                    continue;
                }

                return;
            }

            if (expectElement && commaAfterRealElement)
            {
                AddMissingAfterPrevious(
                    "Ожидался элемент списка после запятой",
                    "(пропущен элемент)");
            }
        }

        private void ParseElement()
        {
            if (ConsumeInvalidToken())
                return;

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

            if (_stream.Check(LexerTokenCode.Plus) ||
                _stream.Check(LexerTokenCode.Minus))
            {
                ParseSignedNumber();
                return;
            }

            AddError("Ожидался числовой литерал");

            RecoverWithinDeclaration(
                LexerTokenCode.Comma,
                LexerTokenCode.RightParen,
                LexerTokenCode.Semicolon);
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

        private bool RecoverTo(params LexerTokenCode[] anchors)
        {
            while (!_stream.IsAtEnd)
            {
                if (IsCurrentOneOf(anchors))
                    return true;

                if (IsNextDeclarationStart())
                    return false;

                if (_stream.Current.Line != _currentDeclarationLine)
                    return false;

                _stream.Advance();
            }

            return false;
        }

        private bool RecoverWithinDeclaration(params LexerTokenCode[] anchors)
        {
            while (!_stream.IsAtEnd)
            {
                if (IsCurrentOneOf(anchors))
                    return true;

                if (IsNextDeclarationStart())
                    return false;

                if (_stream.Current.Line != _currentDeclarationLine)
                    return false;

                _stream.Advance();
            }

            return false;
        }

        private void AbortCurrentDeclaration()
        {
            RecoverTo(LexerTokenCode.Semicolon, LexerTokenCode.Val);

            if (_stream.Match(LexerTokenCode.Semicolon))
                return;
        }

        private bool ConsumeInvalidToken()
        {
            if (!_stream.Check(LexerTokenCode.Invalid))
                return false;

            _stream.Advance();
            return true;
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

        private bool IsNextDeclarationStart()
        {
            return !_stream.IsAtEnd
                && _stream.Check(LexerTokenCode.Val)
                && _stream.Position != _declarationStartPosition;
        }

        private bool IdentifierLooksLikeDeclarationStart()
        {
            if (!_stream.Check(LexerTokenCode.Identifier))
                return false;

            LexerItem next = _stream.Peek(1);

            if (next == null || !next.Code.HasValue)
                return false;

            return next.Code.Value == (int)LexerTokenCode.Assign
                || next.Code.Value == (int)LexerTokenCode.ListOf
                || next.Code.Value == (int)LexerTokenCode.LeftParen;
        }

        private bool CheckNext(LexerTokenCode code)
        {
            LexerItem next = _stream.Peek(1);

            if (next == null || !next.Code.HasValue)
                return false;

            return next.Code.Value == (int)code;
        }

        private bool IsCurrentOneOf(params LexerTokenCode[] codes)
        {
            if (_stream.IsAtEnd)
                return false;

            return IsTokenOneOf(_stream.Current, codes);
        }

        private bool IsTokenOneOf(LexerItem token, params LexerTokenCode[] codes)
        {
            if (token == null || !token.Code.HasValue)
                return false;

            foreach (LexerTokenCode code in codes)
            {
                if (token.Code.Value == (int)code)
                    return true;
            }

            return false;
        }

        private LexerTokenCode[] Combine(LexerTokenCode first, LexerTokenCode[] rest)
        {
            LexerTokenCode[] result = new LexerTokenCode[rest.Length + 1];
            result[0] = first;

            for (int i = 0; i < rest.Length; i++)
                result[i + 1] = rest[i];

            return result;
        }

        private void AddMissingAtCurrentOrAfterPrevious(string message, string fragmentText)
        {
            if (!_stream.IsAtEnd && _stream.Position == _declarationStartPosition)
            {
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

                return;
            }

            AddMissingAfterPrevious(message, fragmentText);
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
    }
}