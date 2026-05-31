using System.Collections.Generic;

namespace GUI.Expressions
{
    public class QuadrupleGenerationResult
    {
        public List<Quadruple> Quadruples { get; private set; }

        public string ResultName { get; set; }

        public QuadrupleGenerationResult()
        {
            Quadruples = new List<Quadruple>();
        }
    }
}