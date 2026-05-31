using System.Collections.Generic;

namespace GUI.Expressions
{
    public class ExpressionLexicalResult
    {
        public List<ExpressionToken> Tokens { get; private set; }
        public List<ExpressionLexicalError> Errors { get; private set; }

        public ExpressionLexicalResult()
        {
            Tokens = new List<ExpressionToken>();
            Errors = new List<ExpressionLexicalError>();
        }

        public bool HasErrors
        {
            get { return Errors.Count > 0; }
        }
    }
}