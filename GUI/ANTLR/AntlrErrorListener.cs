using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.IO;

namespace GUI.ANTLR
{
    public sealed class AntlrErrorListener : BaseErrorListener
    {
        private readonly string _sourceText;
        private readonly List<AntlrSyntaxError> _errors;

        public AntlrErrorListener(string sourceText, List<AntlrSyntaxError> errors)
        {
            _sourceText = sourceText ?? string.Empty;
            _errors = errors ?? throw new ArgumentNullException(nameof(errors));
        }

        public override void SyntaxError(
            TextWriter output,
            IRecognizer recognizer,
            IToken offendingSymbol,
            int line,
            int charPositionInLine,
            string msg,
            RecognitionException e)
        {
            string fragment = offendingSymbol != null
                ? (offendingSymbol.Text ?? string.Empty)
                : string.Empty;

            int startColumn = charPositionInLine + 1;
            int endColumn = startColumn + Math.Max(0, fragment.Length - 1);
            int absoluteIndex = offendingSymbol != null ? offendingSymbol.StartIndex : 0;

            _errors.Add(new AntlrSyntaxError
            {
                InvalidFragment = string.IsNullOrWhiteSpace(fragment) ? "(пусто)" : fragment,
                Line = line,
                StartColumn = startColumn,
                EndColumn = endColumn,
                AbsoluteIndex = Math.Max(0, absoluteIndex),
                Message = string.IsNullOrWhiteSpace(msg) ? "Синтаксическая ошибка" : msg
            });
        }
    }
}