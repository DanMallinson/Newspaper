using Libraries.Articles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Libraries
{
    public class Source
    {
        const int DEFAULT_TIMEOUT = 30 * 1000;

        public enum DataSource
        {
            Content,
            Link,
            Description,
        };

        public string Name { get; set; }
        public string Url { get; set; }
        public bool IncludeTables { get; set; }
        public bool IncludeImages { get; set; }
        public ContentNode ContentNode { get; set; }
        public int Timeout { get; set; }
        [JsonPropertyName("Includes")]
        public List<string> IncludeCategories { get; set; }
        [JsonPropertyName("Excludes")]
        public List<string> ExcludeCategories { get; set; }
        [JsonPropertyName("ParseOrder")]
        public DataSource[] ParseOrder { get; set; }

        public Source()
        {
            ContentNode = new ContentNode();
            Timeout = DEFAULT_TIMEOUT;
            IncludeCategories = new List<string>();
            ExcludeCategories = new List<string>();
            IncludeTables = true;
            IncludeImages = false;

            ParseOrder = [DataSource.Content, DataSource.Link, DataSource.Description];
        }

        public List<Article> GetArticles (DateTime from, List<string> visitedUrls)
        {
            var results = new List<Article>();

            var articleDetails = GetArticleDetails();

            foreach (var articleDetail in articleDetails)
            {
                if(!ShouldArticleBeIncluded(articleDetail,from, visitedUrls))
                {
                    continue;
                }

                var article = CreateArticle(articleDetail);

                if(article == null || string.IsNullOrEmpty(article.Title) || article.IsEmpty())
                {
                    continue;
                }
                results.Add(article);
                visitedUrls.Add(articleDetail.Link);
            }

            return results;
        }

        private List<ArticleDetails> GetArticleDetails()
        {
            var results = new List<ArticleDetails>();

            var xml = GetFeedContent();

            var xmlNodes = GetFeedNodes(xml);

            if(xmlNodes == null)
            {
                return results;
            }

            var serialiser = new XmlSerializer(typeof(ArticleDetails));

            foreach (XmlElement xmlNode in xmlNodes)
            {
                var reader = new StringReader(xmlNode.OuterXml);
                var details = serialiser.Deserialize(reader) as ArticleDetails;

                if (details == null)
                {
                    continue;
                }
                //HACK for nodes with a colon
                var expandedDetails = GetExpandedDetails(xmlNode.OuterXml);

                details.EncodedContent = expandedDetails.EncodedContent;
                details.Creator = expandedDetails.Creator;
                details.Media = expandedDetails.Media;

                results.Add(details);
            }

            return results;
        }

        private string GetFeedContent()
        {
            var result = string.Empty;

            try
            {
                var httpClient = new HttpClient()
                {
                    Timeout = TimeSpan.FromMilliseconds(Timeout)
                };
                var message = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(Url),
                };

                var response = httpClient.Send(message);
                
                var stream = response.Content.ReadAsStream();

                using (var reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }

            }
            catch (Exception ex)
            {
            }

            return result;
        }

        private XmlNodeList? GetFeedNodes(string input)
        {
            try
            {
                if (string.IsNullOrEmpty(input))
                {
                    return null;
                }

                var document = new XmlDocument();
                document.LoadXml(input);

                return document.GetElementsByTagName("item");
            }

            catch (Exception ex)
            {
            }
            return null;
        }

        private Article CreateArticle(ArticleDetails details)
        {
            var result = new BasicArticle()
            {
                Author = !string.IsNullOrEmpty(details.Author) ? details.Author : details.Creator,
                Title = details.Title,
                Source = Name,
            };

            if(IncludeImages && details.Media != null)
            {
                result.Thumbnail = details.Media.Url;
            }

            result.Parse(details, this);

            return result;
        }

        private ExpandedArticleDetails GetExpandedDetails(string outerXml)
        {
            var result = new ExpandedArticleDetails();

            var xDoc = new XmlDocument();
            xDoc.LoadXml(outerXml);
            var contentNodes = xDoc.DocumentElement.GetElementsByTagName("content:encoded");
            if (contentNodes.Count > 0)
            {
                result.EncodedContent = contentNodes[0].InnerText;
            }
            
            var creatorNodes = xDoc.DocumentElement.GetElementsByTagName("dc:creator");
            if (creatorNodes.Count > 0)
            {
                result.Creator = creatorNodes[0].InnerText;
            }
            var mediaNodes = xDoc.DocumentElement.GetElementsByTagName("media:content");
            if (mediaNodes.Count > 0)
            {
                var mediaNode = mediaNodes[0];
                if (GetAttributeValue(mediaNode, "medium") == "image")
                {
                    var url = GetAttributeValue(mediaNode, "url");

                    if (url != null)
                    {
                        result.Media = new Media()
                        {
                            Url = url,
                        };
                    }
                }
            }

            return result;
        }

        private bool ShouldArticleBeIncluded(ArticleDetails details,DateTime from, List<string> visitedUrls)
        {
            if (details.Timestamp < from)
            {
                return false;
            }

            if (visitedUrls.Contains(details.Link))
            {
                return false;
            }

            if (details.Categories.Any(item => ExcludeCategories.Contains(item.ToLower())))
            {
                return false;
            }

            if (IncludeCategories.Count > 0 && !details.Categories.Any(item => IncludeCategories.Contains(item.ToLower())))
            {
                return false;
            }

            return true;
        }

        private string GetAttributeValue(XmlNode node, string attributeName)
        {
            if (node == null)
            {
                return string.Empty;
            }

            var attribute = node.Attributes[attributeName];

            if (attribute == null)
            {
                return string.Empty;
            }

            return attribute.Value;
        }
    }
}
