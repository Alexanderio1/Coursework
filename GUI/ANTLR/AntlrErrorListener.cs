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
            string fragment = offendingSymbol != null ? (offendingSymbol.Text ?? string.Empty) : string.Empty;
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
                Message = NormalizeMessage(offendingSymbol, msg)
            });
        }

        private static string NormalizeMessage(IToken offendingSymbol, string msg)
        {
            string text = msg ?? string.Empty;

            if (offendingSymbol != null && offendingSymbol.Type == TokenConstants.EOF)
            {
                if (text.Contains("RPAREN") || text.Contains("')'"))
                    return "Ожидался элемент списка или символ )";

                if (text.Contains("SEMI") || text.Contains("';'"))
                    return "Ожидался символ ; в конце объявления";
            }

            if (text.Contains("LISTOF") || text.Contains("'listOf'"))
                return "Ожидалась лексема listOf";

            if (text.Contains("ASSIGN") || text.Contains("'='"))
                return "Ожидался оператор присваивания =";

            if (text.Contains("IDENTIFIER"))
                return "Ожидался идентификатор после val";

            if (text.Contains("RPAREN") || text.Contains("')'"))
                return "Ожидался элемент списка или символ )";

            if (text.Contains("SEMI") || text.Contains("';'"))
                return "Ожидался символ ; в конце объявления";

            return string.IsNullOrWhiteSpace(text) ? "Синтаксическая ошибка" : text;
        }
    }
}