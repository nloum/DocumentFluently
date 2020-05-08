using System.Linq;
using System.Threading.Tasks;
using IoFluently;
using Markdig;
using ReactiveProcesses;

namespace DocumentFluently.Documentation
{
    class Program
    {
        static void Main(string[] args)
        {
            var ioService = new IoService(new ReactiveProcessFactory());

            var repoRoot = ioService.CurrentDirectory.Ancestors()
                .First(ancestor => (ancestor / "src" / "DocumentFluently.sln").Exists());
            
            var readmeMarkdownFile = (repoRoot / "readme.md").AsMarkdownFile();
            var readmeHtmlFile = readmeMarkdownFile.Path.WithExtension(".html").AsHtmlFile();

            var markdown = readmeMarkdownFile.Read();
            var html = markdown.ToHtml(x =>
                x.UseAdvancedExtensions()
                    .UseDiagrams()
                    .Build());
            html = new Html(new[]{"<html><body><script src=\"https://cdn.jsdelivr.net/npm/mermaid@8.4.0/dist/mermaid.min.js\"></script><script>mermaid.initialize({startOnLoad:true,securityLevel:'loose'});</script>"}
                .Concat(html.Lines)
                .Concat(new[]{"</body></html>"}));
            readmeHtmlFile.Write(html);
        }
    }
}