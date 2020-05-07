using System.Collections.Generic;

namespace DocumentFluently
{
    public class Html
    {
        public IEnumerable<string> Lines { get; }

        public Html(IEnumerable<string> lines)
        {
            Lines = lines;
        }
    }
}