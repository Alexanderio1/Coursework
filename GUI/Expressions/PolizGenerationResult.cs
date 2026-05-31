using System.Collections.Generic;

namespace GUI.Expressions
{
    public class PolizGenerationResult
    {
        public List<string> Items { get; private set; }

        public bool ContainsIdentifiers { get; set; }

        public string PolizText
        {
            get { return string.Join(" ", Items); }
        }

        public PolizGenerationResult()
        {
            Items = new List<string>();
        }
    }
}