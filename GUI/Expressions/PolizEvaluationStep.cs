namespace GUI.Expressions
{
    public class PolizEvaluationStep
    {
        public int Number { get; private set; }
        public string Token { get; private set; }
        public string Action { get; private set; }
        public string StackState { get; private set; }

        public PolizEvaluationStep(int number, string token, string action, string stackState)
        {
            Number = number;
            Token = token;
            Action = action;
            StackState = stackState;
        }
    }
}