using System.Collections.Generic;

namespace GUI.Expressions
{
    public class ExpressionParseResult
    {
        public ExpressionNode Root { get; set; }

        public List<ExpressionSyntaxError> Errors { get; private set; }

        public ExpressionParseResult()
        {
            Errors = new List<ExpressionSyntaxError>();
        }

        public bool HasErrors
        {
            get { return Errors.Count > 0; }
        }
    }
}