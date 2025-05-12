using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Libraries.Articles
{
    [XmlRoot(ElementName ="item")]
    public class ArticleDetails
    {
        [XmlElement("author")]
        public string? Author { get; set; }
        [XmlElement("dc:creator")]
        public string? Creator { get; set; }
        [XmlElement("category")]
        public List<string>? Categories { get; set; }
        [XmlElement("description")]
        public string? Description { get; set; }
        [XmlElement("link")]
        public string? Link { get; set; }
        [XmlElement("pubDate")]
        public string? Published { get; set; }
        [XmlElement("title")]
        public string? Title { get; set; }
        [XmlElement("content")]
        public string? Content { get; set; }
        [XmlElement("content:encoded")]
        public string? EncodedContent { get; set; }
        [XmlElement("media")]
        public Media? Media { get; set; }

        public DateTime Timestamp
        {
            get
            {
                try
                {
                    return DateTime.Parse(Published);
                }
                catch
                {
                    //TODO - handle formats better
                    return DateTime.ParseExact(Published,"ddd, dd MM yy HH:mm:ss zzz", CultureInfo.InvariantCulture);
                }
            }
        }
    }
}
