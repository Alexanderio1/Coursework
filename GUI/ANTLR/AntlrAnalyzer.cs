using Antlr4.Runtime;
using System;
using System.IO;

namespace GUI.ANTLR
{
    public sealed class AntlrAnalyzer
    {
        public AntlrSyntaxResult Analyze(string sourceText)
        {
            var result = new AntlrSyntaxResult();

            var inputStream = new AntlrInputStream(new StringReader(sourceText ?? string.Empty));
            var lexer = new KotlinListLexer(inputStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new KotlinListParser(tokenStream);

            lexer.RemoveErrorListeners();
            parser.RemoveErrorListeners();

            lexer.AddErrorListener(new AntlrLexerErrorListener(sourceText, result.Errors));
            parser.AddErrorListener(new AntlrErrorListener(sourceText, result.Errors));

            tokenStream.Fill();

            while (tokenStream.LA(1) != TokenConstants.EOF)
            {
                int tokenType = tokenStream.LA(1);
                IToken current = tokenStream.LT(1);

                if (tokenType == KotlinListLexer.VAL)
                {
                    parser.declaration();
                    continue;
                }

                if (tokenType == KotlinListLexer.IDENTIFIER)
                {
                    AddManualError(result, current, "Ожидалось ключевое слово val");
                    parser.declarationNoVal();
                    continue;
                }

                AddManualError(result, current, "Ожидалось ключевое слово val");
                SkipToStatementEnd(tokenStream);
            }

            return result;
        }

        private static void SkipToStatementEnd(CommonTokenStream tokenStream)
        {
            while (tokenStream.LA(1) != KotlinListLexer.SEMI &&
                   tokenStream.LA(1) != TokenConstants.EOF)
            {
                tokenStream.Consume();
            }

            if (tokenStream.LA(1) == KotlinListLexer.SEMI)
                tokenStream.Consume();
        }

        private static void AddManualError(AntlrSyntaxResult result, IToken token, string message)
        {
            string fragment = token?.Text ?? string.Empty;
            int startColumn = (token?.Column ?? 0) + 1;
            int endColumn = startColumn + Math.Max(0, fragment.Length - 1);
            int absoluteIndex = token?.StartIndex ?? 0;

            result.Errors.Add(new AntlrSyntaxError
            {
                InvalidFragment = string.IsNullOrWhiteSpace(fragment) ? "(пусто)" : fragment,
                Line = token?.Line ?? 1,
                StartColumn = startColumn,
                EndColumn = endColumn,
                AbsoluteIndex = Math.Max(0, absoluteIndex),
                Message = message
            });
        }
    }
}