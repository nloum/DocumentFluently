using System.Collections.Generic;
using IoFluently;

namespace DocumentFluently
{
    public class Html : Text
    {
        public Html(IEnumerable<string> lines) : base(lines)
        {
        }
    }
}