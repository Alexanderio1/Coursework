using System.Collections.Generic;

namespace GUI.Lexer
{
    public class LexerResult
    {
        public LexerResult()
        {
            Items = new List<LexerItem>();
        }

        public List<LexerItem> Items { get; private set; }

        public bool HasErrors
        {
            get
            {
                foreach (var item in Items)
                {
                    if (item.IsError)
                        return true;
                }

                return false;
            }
        }
    }
}