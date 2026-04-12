using System.Collections.Generic;

namespace GUI.ANTLR
{
    public sealed class AntlrSyntaxResult
    {
        public List<AntlrSyntaxError> Errors { get; } = new List<AntlrSyntaxError>();

        public bool HasErrors => Errors.Count > 0;

        public int ErrorCount => Errors.Count;
    }
}