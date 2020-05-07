# DocumentFluently

DocumentFluently is a C# library that lets you write fluent file processing code.

Example:

    var ioService = new IoService(new ReactiveProcessFactory());

    var repoRoot = ioService.CurrentDirectory.Ancestors()
        .First(ancestor => (ancestor / "DocumentFluently.sln").Exists());
            
    var readmeMarkdownFile = (repoRoot / "readme.md").AsMarkdownFile();
    var readmeHtmlFile = readmeMarkdownFile.Path.WithExtension(".html").AsHtmlFile();

    var markdown = await readmeMarkdownFile.Read();
    var html = markdown.ToHtml(x =>
        x.UseAdvancedExtensions()
            .UseDiagrams()
            .Build());
    html = new Html(new[]{"<html><body><script src=\"https://cdn.jsdelivr.net/npm/mermaid@8.4.0/dist/mermaid.min.js\"></script><script>mermaid.initialize({startOnLoad:true,securityLevel:'loose'});</script>"}
        .Concat(html.Lines)
        .Concat(new[]{"</body></html>"}));
    await readmeHtmlFile.Write(html);

```mermaid
graph TD;
    A-->B;
    A-->C;
    B-->D;
    C-->D;
```
