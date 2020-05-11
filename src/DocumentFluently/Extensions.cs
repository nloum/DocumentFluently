using System;
using System.Collections.Generic;
using IoFluently;
using Markdig;

namespace DocumentFluently
{
    /// <summary>
    /// Extension methods for fluently generating documentation
    /// </summary>
    public static class Extensions
    {
        public static IFileWithKnownFormatSync<Html, Html> AsHtmlFile(this AbsolutePath path)
        {
            return path.AsFile(abs => new Html(abs.ReadLines()),
                (abs, html) => abs.WriteAllLines(html.Lines));
        }

        public static Html ToHtml(this Markdown markdown)
        {
            var html = Markdig.Markdown.ToHtml(string.Join("\n", markdown.Lines));
            return new Html(html.Split('\n'));
        }

        public static Html ToHtml(this Markdown markdown, Func<MarkdownPipelineBuilder, MarkdownPipeline> pipelineBuilder)
        {
            var pipeline = pipelineBuilder(new MarkdownPipelineBuilder());
            var html = Markdig.Markdown.ToHtml(string.Join("\n", markdown.Lines), pipeline);
            return new Html(html.Split('\n'));
        }
        
        public static IFileWithKnownFormatSync<Markdown, Markdown> AsMarkdownFile(this AbsolutePath path)
        {
            return path.AsFile(abs => new Markdown(abs.ReadLines()),
                (abs, markdown) => abs.WriteAllLines(markdown.Lines));
        }

        public static Markdown AsMarkdown(this IEnumerable<string> lines)
        {
            return new Markdown(lines);
        }

        public static AbsolutePath wkhtmltopdf(this IFileWithKnownFormatSync<Html> htmlFile, AbsolutePath pdfFilePath = null)
        {
            if (pdfFilePath == null)
            {
                pdfFilePath = htmlFile.Path.WithExtension(".pdf");
            }

            var process = htmlFile.Path.IoService.ReactiveProcessFactory.Start("wkhtmltopdf", $"{htmlFile.Path} {pdfFilePath}");
            process.ExitCode.Wait();

            return pdfFilePath;
        }
    }
}