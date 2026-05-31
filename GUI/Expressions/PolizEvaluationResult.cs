using System.Collections.Generic;

namespace GUI.Expressions
{
    public class PolizEvaluationResult
    {
        public List<PolizEvaluationStep> Steps { get; private set; }
        public List<string> Errors { get; private set; }

        public bool CanEvaluate { get; set; }
        public bool Success { get; set; }
        public long Value { get; set; }

        public PolizEvaluationResult()
        {
            Steps = new List<PolizEvaluationStep>();
            Errors = new List<string>();
            CanEvaluate = true;
            Success = false;
        }

        public bool HasErrors
        {
            get { return Errors.Count > 0; }
        }
    }
}