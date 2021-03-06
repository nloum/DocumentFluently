﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using IoFluently;
using LiveLinq;
using ReactiveProcesses;
using System.Reactive.Linq;
using System.Xml;
using SimpleMonads;

namespace DocumentFluently.Documentation
{
    class Program
    {
        static void Main(string[] args)
        {
            var service = new IoService(new ReactiveProcessFactory());
            var repositoryRoot = service.CurrentDirectory.Ancestors().First(ancestor => (ancestor / ".git").Exists());
            var documentationRoot = service.ParseAbsolutePath("/Users/family/Resilio Sync/NathanLoumFamily");

            var markdownFiles = documentationRoot.Descendants()
                .ToLiveLinq()
                .Where(x => x.HasExtension(".md") && x.GetPathType() == PathType.File)
                .Select(x => x.AsSmallTextFile());

            var markdownHtmls = markdownFiles
                .Select(markdownFile =>
                {
                    var markdown = markdownFile.Read();
                    var html = "<html>" + Markdig.Markdown.ToHtml(markdown) + "</html>";
                    var document = new XmlDocument();
                    document.LoadXml(html);
                    return new {markdownFile.Path, Html = document};
                });

            var docxFiles = documentationRoot.Descendants()
                .ToLiveLinq()
                .Where(x => x.HasExtension(".docx") && x.GetPathType() == PathType.File);

            var docxHtmls = docxFiles
                .Select(docxPath => new {Source = docxPath, Target = docxPath.WithExtension(".html")})
                .Do(x => x.Source.PandocToHtml(x.Target), x => x.Target.DeleteFile())
                .Select(x => new {Path = x.Source, Html = x.Target.AsXmlFile().Read()});

            var htmls = markdownHtmls.Concat(docxHtmls, true);

            var backLinks = htmls.SelectMany(html =>
                {
                    var links = html.Html.SelectNodes("//a");
                    var backLinksForThisFile = new List<Tuple<AbsolutePath, AbsolutePath, string>>();
                    foreach (var link in links)
                    {
                        var linkEl = link as XmlElement;
                        var href = linkEl.Attributes["href"].InnerText;
                        var text = linkEl.InnerText;
                        if (href.Contains(":"))
                        {
                            continue;
                        }

                        var maybePath = service.TryParseRelativePath(href);
                        if (!maybePath.HasValue)
                        {
                            continue;
                        }

                        var linkTo = (html.Path.Parent() / maybePath.Value).Simplify();
                        backLinksForThisFile.Add(Tuple.Create(html.Path, linkTo, text));
                    }

                    var parentReadme = string.Equals(html.Path.Name, "readme.md", StringComparison.OrdinalIgnoreCase) ? html.Path.Ancestor(2) / "readme.md" : html.Path.Ancestor(1) / "readme.md";
                    backLinksForThisFile.Add(Tuple.Create(html.Path, parentReadme, "Parent"));
                    
                    return backLinksForThisFile.AsEnumerable();
                })
                .GroupBy(x => x.Item2)
                .SelectValue(x => x.Select(y => Tuple.Create(y.Item1, y.Item3)))
                .ToReadOnlyObservableDictionary();
            
            var transformedHtmls = htmls.Select(html => backLinks.ToLiveLinq()[html.Path]
                .SelectLatest(x =>
                {
                    return x.Select(y => y.ToObservableState()).Otherwise(() =>
                            Observable.Return(ImmutableHashSet<Tuple<AbsolutePath, string>>.Empty));
                })
                .Select(backLinksForThisHtml =>
            {
                var links = html.Html.SelectNodes("//a");
                foreach (var link in links)
                {
                    var linkEl = link as XmlElement;
                    var href = linkEl.Attributes["href"].InnerText;
                    var text = linkEl.InnerText;
                    if (href.Contains(":"))
                    {
                        continue;
                    }

                    var maybePath = service.TryParseRelativePath(href);
                    if (!maybePath.HasValue)
                    {
                        continue;
                    }

                    if (linkEl.Attributes["href"].InnerText.EndsWith(".md"))
                    {
                        linkEl.Attributes["href"].InnerText =
                            linkEl.Attributes["href"].InnerText
                                .Substring(0, linkEl.Attributes["href"].InnerText.Length - 3) + ".html";
                    }
                    
                    if (linkEl.Attributes["href"].InnerText.EndsWith(".docx"))
                    {
                        linkEl.Attributes["href"].InnerText =
                            linkEl.Attributes["href"].InnerText
                                .Substring(0, linkEl.Attributes["href"].InnerText.Length - 3) + ".html";
                    }
                }
                
                var backLinkHtml =
                    "<ul>" + string.Join("\n",
                        backLinksForThisHtml.Select(x =>
                        {
                            var relativePath = x.Item1.WithExtension(".html").RelativeTo(html.Path.Parent());
                            return
                                    $"<li>This page is \"{x.Item2}\" of - <a href=\"{relativePath}\">{relativePath}</a></li>";
                        })) + "</ul>";
                return new {html.Path, Html = html.Html.OuterXml + backLinkHtml};
            }));
            
            transformedHtmls
                .Subscribe(addedHtml => {
                    Console.WriteLine($"Adding a markdown file: {addedHtml.Path}");
                    addedHtml.Path.WithExtension(".html").WriteAllText(addedHtml.Html);
                }, (removedHtml, removalMode) => {
                    Console.WriteLine($"Removing a markdown file: {removedHtml.Path}");
                    removedHtml.Path.WithExtension(".html").Delete();
                });
        }
    }
}