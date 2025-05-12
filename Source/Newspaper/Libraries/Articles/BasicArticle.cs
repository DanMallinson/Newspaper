using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Libraries.Articles.Blocks;

namespace Libraries.Articles
{
    public class BasicArticle : Article
    {
        const int DEFAULT_TIMEOUT = 30 * 1000;

        public int Timeout { get; set; }

        private Dictionary<string, string>
            _remapTable;

        private List<string>
            _allowedNodeClasses;

        public BasicArticle()
        {
            Timeout = DEFAULT_TIMEOUT;

            _remapTable = new Dictionary<string, string>();
            _allowedNodeClasses = new List<string>();
            SetupDefaultRemaps();
            SetupDefaultAllowedClasses();
        }

        private void SetupDefaultRemaps()
        {
            _remapTable = new Dictionary<string, string>()
            {
                { "&quot;", "\"" },
                { "&#x27;", "'" },
                { "&#8216;", "'" },
                { "&#8217;", "'" },
                { "&#8220;", "\"" },
                { "&#8221;", "\"" },
                { "&#8211;", "-" },
                { "&rsquo;", "'" },
                { "&lsquo;", "'" },
                { "&ndash;", "-" },
                { "&nbsp;", " " },
                { "&rdquo;", "\"" },
                { "&ldquo;", "\"" },
                { "&pound;", "£" },
                { "&amp;", "&" },
                { "&#x2014;", "-" },
                { "&#x2019;", "'" },
                { "&#x2018;", "'" },
                { "&#x201C;", "\"" },
                { "&#x201D;", "\"" },
                { "&#xB0;", "°" },
                { "&#xA3;", "£" },
                { "&#x2013;", "-" },
                { "&#xA0;", "" },
                { "&apos;", "'" },
                { "&#038;", "&" },
                { "&#8230;","..." },
                { "&#39;","'" },
                { "&#8212;","-" },
                { "&eacute;","é" },
                { "&lt;","<" },
                { "&gt;",">" },
            };
        }

        private void SetupDefaultAllowedClasses()
        {
            _allowedNodeClasses = new List<string>()
            {
                "p",
                "#text",
                "html",
                "body",
                "strong",
                "em",
                "i",
                "b",
                "a",
                "h1",
                "h2",
                "h3",
                "h4",
                "h5",
                "h6",
                //"div",

                "img",
                "table",
            };
        }

        protected override void OnParse(ArticleDetails details, Source source)
        {
            var options = new ParseOptions()
            {
                IncludeImages = source.IncludeImages,
                IncludeTables = source.IncludeTables,
                ContentNode = source.ContentNode,
            };

            for (var i = 0; i < source.ParseOrder.Length; ++i)
            {
                switch (source.ParseOrder[i])
                {
                    case Libraries.Source.DataSource.Content:
                        if(!string.IsNullOrEmpty(details.Content))
                        {
                            ParseRaw(details.Content,options); 
                        }
                        else if (!string.IsNullOrEmpty(details.EncodedContent))
                        {
                            ParseRaw(details.EncodedContent, options);
                        }
                        break;
                    case Libraries.Source.DataSource.Description:
                        if(!string.IsNullOrEmpty(details.Description))
                        {
                            ParseRaw(details.Description,options);
                        }
                        break;
                    case Libraries.Source.DataSource.Link:
                        if (!string.IsNullOrEmpty(details.Link))
                        {
                            ParseLinked(details.Link,options);
                        }
                        break;
                }

                if(Content.Count != 0)
                {
                    break;
                }
            }
        }

        protected virtual void ParseLinked(string url, ParseOptions options)
        {
            if (options == null || options.ContentNode == null || string.IsNullOrEmpty(options.ContentNode.NodeType))
            {
                return;
            }

            var content = GetLinkedContent(url);
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(content);

            var output = new StringBuilder();
            var nodes = htmlDocument.DocumentNode.SelectNodes($"//{options.ContentNode.NodeType}[contains(@{options.ContentNode.PropertyName}, '{options.ContentNode.PropertyValue}')]");

            if(nodes == null)
            {
                return;
            }

            foreach(var node in nodes)
            {
                ParseNode(node, options);
            }
        }

        private void ParseNode(HtmlNode node, ParseOptions options)
        {
            if(node.Name == "img")
            {
                if(options.IncludeImages)
                {
                    ParseImage(node);
                }
            }
            else if (node.Name == "table")
            {
                if ((options.IncludeTables))
                {
                    ParseTable(node);
                }
            }
            else
            {
                if (Contains(node,"img",true) || Contains(node,"table",true))
                {
                    foreach(var child in node.ChildNodes)
                    {
                        if (_allowedNodeClasses.Contains(child.Name) ||
                            (Contains(child,"img",true) && options.IncludeImages) ||
                            (Contains(child, "table", true) && options.IncludeTables))
                        {
                            ParseNode(child, options);
                        }
                    }
                }
                else
                {
                    TrimNode(node);
                    ParseNodeContent(node);
                }
            }
        }

        private void ParseImage(HtmlNode node)
        {
            var src = node.GetAttributeValue("src",string.Empty);

            if(string.IsNullOrEmpty(src))
            {
                return;
            }

            Content.Add(new ImageUrlBlock()
            { 
                Url = src 
            });
        }

        private void ParseTable(HtmlNode node)
        {
            var rows = node.SelectNodes(".//tr");
            var tableObject = new List<Columns>();
            foreach (var row in rows)
            {
                var rowObject = new Columns();

                var columns = row.SelectNodes(".//td | .//th");
                var blank = true;

                foreach (var column in columns)
                {
                    var content = Tidy(column.InnerText);
                    if(!string.IsNullOrEmpty(content))
                    {
                        blank  = false;
                    }
                    rowObject.Add(content);
                }

                if(!blank)
                {
                    tableObject.Add(rowObject);
                }
            }

            if (tableObject.Count > 0)
            {
                var block = new TableBlock()
                {
                    Content = tableObject
                };

                Content.Add(block);
            }
        }

        private void TrimNode(HtmlNode node)
        {
            var removalList = new List<HtmlNode>();
            foreach(var child in node.ChildNodes)
            {
                if(!_allowedNodeClasses.Contains(child.Name))
                {
                    removalList.Add(child);
                }
            }

            foreach(var item in removalList)
            {
                node.RemoveChild(item);
            }
        }

        private string GetLinkedContent(string url)
        {
            var result = string.Empty;

            try
            {
                //TODO - this is reused from source, could possibly collate into a function
                var httpClient = new HttpClient()
                {
                    Timeout = TimeSpan.FromMilliseconds(Timeout)
                };
                var message = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(url),
                };

                var response = httpClient.Send(message);

                var stream = response.Content.ReadAsStream();

                using (var reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }
            }
            catch
            {

            }

            return result;
        }

        protected virtual void ParseRaw(string content, ParseOptions options)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(content);
            var node = htmlDocument.DocumentNode;
            ParseNode(node, options);
        }

        protected virtual void ParseNodeContent(HtmlNode node)
        {
            var tidiedText = Tidy(node.InnerText);

            if(string.IsNullOrEmpty(tidiedText))
            {
                return;
            }

            var newContentBlock = new TextBlock()
            {
                Text = tidiedText 
            };

            Content.Add(newContentBlock);
        }

        private string Tidy(string input)
        {
            foreach (var pair in _remapTable)
            {
                input = input.Replace(pair.Key, pair.Value);
            }

            input = Regex.Replace(input, @"(\r?\n){2,}", "\n").Trim();

            return input;
        }

        private bool Contains(HtmlNode parent, string childType, bool includeDescendants)
        {
            return parent.SelectSingleNode($"{(includeDescendants ? ".//" : "./")}{childType}") != null;
        }
    }
}
