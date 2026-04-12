using Antlr4.Runtime;
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

            var lexerListener = new AntlrLexerErrorListener(sourceText, result.Errors);
            var parserListener = new AntlrErrorListener(sourceText, result.Errors);

            lexer.AddErrorListener(lexerListener);
            parser.AddErrorListener(parserListener);

            parser.file();

            return result;
        }
    }
}