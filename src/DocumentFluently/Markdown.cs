using System.Collections.Generic;
using IoFluently;

namespace DocumentFluently
{
    public class Markdown : Text
    {
        public Markdown(IEnumerable<string> lines) : base(lines)
        {
        }
    }
}