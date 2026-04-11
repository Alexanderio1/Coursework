using System.Collections.Generic;

namespace GUI.Syntax
{
    public sealed class SyntaxResult
    {
        public SyntaxResult()
        {
            Errors = new List<SyntaxError>();
        }

        public List<SyntaxError> Errors { get; private set; }

        public bool HasErrors
        {
            get { return Errors.Count > 0; }
        }

        public int ErrorCount
        {
            get { return Errors.Count; }
        }
    }
}