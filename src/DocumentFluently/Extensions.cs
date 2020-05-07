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
        public static FileWithKnownFormat<Html, Html> AsHtmlFile(this AbsolutePath path)
        {
            return new FileWithKnownFormat<Html, Html>(path,
                async path => new Html(path.ReadLines()), 
                async (path, markdown) => path.WriteAllLines(markdown.Lines));
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
        
        public static FileWithKnownFormat<Markdown, Markdown> AsMarkdownFile(this AbsolutePath path)
        {
            return new FileWithKnownFormat<Markdown, Markdown>(path,
                async path => new Markdown(path.ReadLines()), 
                async (path, markdown) => path.WriteAllLines(markdown.Lines));
        }

        public static Markdown AsMarkdown(this IEnumerable<string> lines)
        {
            return new Markdown(lines);
        }
    }
}