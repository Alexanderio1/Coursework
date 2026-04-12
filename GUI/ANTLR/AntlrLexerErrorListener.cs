using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.IO;

namespace GUI.ANTLR
{
    public sealed class AntlrLexerErrorListener : IAntlrErrorListener<int>
    {
        private readonly string _sourceText;
        private readonly List<AntlrSyntaxError> _errors;

        public AntlrLexerErrorListener(string sourceText, List<AntlrSyntaxError> errors)
        {
            _sourceText = sourceText ?? string.Empty;
            _errors = errors ?? throw new ArgumentNullException(nameof(errors));
        }

        public void SyntaxError(
            TextWriter output,
            IRecognizer recognizer,
            int offendingSymbol,
            int line,
            int charPositionInLine,
            string msg,
            RecognitionException e)
        {
            int startColumn = charPositionInLine + 1;
            int endColumn = startColumn;
            int absoluteIndex = GetAbsoluteIndex(_sourceText, line, startColumn);

            string fragment = string.Empty;
            if (absoluteIndex >= 0 && absoluteIndex < _sourceText.Length)
                fragment = _sourceText[absoluteIndex].ToString();

            _errors.Add(new AntlrSyntaxError
            {
                InvalidFragment = string.IsNullOrWhiteSpace(fragment) ? "(пусто)" : fragment,
                Line = line,
                StartColumn = startColumn,
                EndColumn = endColumn,
                AbsoluteIndex = Math.Max(0, absoluteIndex),
                Message = string.IsNullOrWhiteSpace(msg) ? "Лексическая ошибка ANTLR" : msg
            });
        }

        private static int GetAbsoluteIndex(string text, int line, int column)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            int currentLine = 1;
            int currentColumn = 1;

            for (int i = 0; i < text.Length; i++)
            {
                if (currentLine == line && currentColumn == column)
                    return i;

                if (text[i] == '\n')
                {
                    currentLine++;
                    currentColumn = 1;
                }
                else
                {
                    currentColumn++;
                }
            }

            return Math.Max(0, text.Length - 1);
        }
    }
}