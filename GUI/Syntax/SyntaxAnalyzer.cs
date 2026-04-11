using System.Collections.Generic;
using GUI.Lexer;

namespace GUI.Syntax
{
    public sealed class SyntaxAnalyzer
    {
        private SyntaxTokenStream _stream;
        private SyntaxResult _result;

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
            if (!Expect(LexerTokenCode.Val, "Ожидалось ключевое слово val"))
                SkipTo(LexerTokenCode.Identifier, LexerTokenCode.Assign, LexerTokenCode.ListOf, LexerTokenCode.Semicolon, LexerTokenCode.Val);

            if (!Expect(LexerTokenCode.Identifier, "Ожидался идентификатор после val"))
                SkipTo(LexerTokenCode.Assign, LexerTokenCode.ListOf, LexerTokenCode.Semicolon, LexerTokenCode.Val);

            if (!Expect(LexerTokenCode.Assign, "Ожидался оператор присваивания ="))
                SkipTo(LexerTokenCode.ListOf, LexerTokenCode.LeftParen, LexerTokenCode.Semicolon, LexerTokenCode.Val);

            ParseListInitializer();

            if (!Expect(LexerTokenCode.Semicolon, "Ожидался символ ; в конце объявления"))
            {
                SkipTo(LexerTokenCode.Semicolon, LexerTokenCode.Val);
                _stream.Match(LexerTokenCode.Semicolon);
            }
        }

        private void ParseListInitializer()
        {
            if (!Expect(LexerTokenCode.ListOf, "Ожидалась лексема listOf"))
                SkipTo(LexerTokenCode.LeftParen, LexerTokenCode.RightParen, LexerTokenCode.Semicolon, LexerTokenCode.Val);

            if (!Expect(LexerTokenCode.LeftParen, "Ожидалась открывающая круглая скобка ("))
                SkipTo(LexerTokenCode.RightParen, LexerTokenCode.Semicolon, LexerTokenCode.Val);

            ParseElementsOpt();

            if (!Expect(LexerTokenCode.RightParen, "Ожидалась закрывающая круглая скобка )"))
            {
                SkipTo(LexerTokenCode.RightParen, LexerTokenCode.Semicolon, LexerTokenCode.Val);
                _stream.Match(LexerTokenCode.RightParen);
            }
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

            AddError("Ожидался элемент списка или символ )");
            SkipTo(LexerTokenCode.RightParen, LexerTokenCode.Semicolon, LexerTokenCode.Val);
        }

        private void ParseElements()
        {
            ParseElement();

            while (_stream.Match(LexerTokenCode.Comma))
            {
                ParseElement();
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

            AddError("Ожидался корректный элемент списка");
            SkipTo(LexerTokenCode.Comma, LexerTokenCode.RightParen, LexerTokenCode.Semicolon, LexerTokenCode.Val);
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
            SkipTo(LexerTokenCode.Comma, LexerTokenCode.RightParen, LexerTokenCode.Semicolon, LexerTokenCode.Val);
        }

        private void ParseSignedNumber()
        {
            ParseSign();

            if (_stream.Match(LexerTokenCode.Int))
                return;

            if (_stream.Match(LexerTokenCode.Double))
                return;

            AddError("После знака ожидался int или double");
            SkipTo(LexerTokenCode.Comma, LexerTokenCode.RightParen, LexerTokenCode.Semicolon, LexerTokenCode.Val);
        }

        private void ParseSign()
        {
            if (_stream.Match(LexerTokenCode.Plus))
                return;

            if (_stream.Match(LexerTokenCode.Minus))
                return;

            AddError("Ожидался знак + или -");
        }

        private bool Expect(LexerTokenCode code, string message)
        {
            if (_stream.Match(code))
                return true;

            AddError(message);
            return false;
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

        private void SkipTo(params LexerTokenCode[] syncTokens)
        {
            HashSet<int> syncSet = new HashSet<int>();

            foreach (LexerTokenCode token in syncTokens)
                syncSet.Add((int)token);

            while (!_stream.IsAtEnd)
            {
                int currentCode = _stream.Current.Code.HasValue ? _stream.Current.Code.Value : -1;

                if (syncSet.Contains(currentCode))
                    break;

                _stream.Advance();
            }
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
    }
}