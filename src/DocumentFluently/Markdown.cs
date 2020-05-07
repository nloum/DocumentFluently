using System.Collections.Generic;

namespace DocumentFluently
{
    public class Markdown
    {
        public IEnumerable<string> Lines { get; }

        public Markdown(IEnumerable<string> lines)
        {
            Lines = lines;
        }
    }
}