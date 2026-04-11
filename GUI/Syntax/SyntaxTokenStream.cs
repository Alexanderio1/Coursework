using System;
using System.Collections.Generic;
using GUI.Lexer;

namespace GUI.Syntax
{
    public sealed class SyntaxTokenStream
    {
        private readonly IReadOnlyList<LexerItem> _tokens;
        private readonly LexerItem _fallbackToken;
        private int _position;

        public SyntaxTokenStream(IReadOnlyList<LexerItem> tokens)
        {
            _tokens = tokens ?? Array.Empty<LexerItem>();
            _position = 0;

            _fallbackToken = new LexerItem
            {
                IsError = false,
                Code = null,
                TypeName = string.Empty,
                Lexeme = string.Empty,
                Message = string.Empty,
                Line = 1,
                StartColumn = 1,
                EndColumn = 1,
                AbsoluteIndex = 0
            };
        }

        public bool IsAtEnd
        {
            get { return _position >= _tokens.Count; }
        }

        public LexerItem Current
        {
            get
            {
                if (_tokens.Count == 0)
                    return _fallbackToken;

                if (IsAtEnd)
                    return _tokens[_tokens.Count - 1];

                return _tokens[_position];
            }
        }

        public LexerItem Previous
        {
            get
            {
                if (_tokens.Count == 0)
                    return _fallbackToken;

                if (_position <= 0)
                    return _tokens[0];

                return _tokens[_position - 1];
            }
        }

        public void Advance()
        {
            if (!IsAtEnd)
                _position++;
        }

        public bool Check(LexerTokenCode code)
        {
            return !IsAtEnd && Current.Code == (int)code;
        }

        public bool Match(LexerTokenCode code)
        {
            if (!Check(code))
                return false;

            Advance();
            return true;
        }
    }
}