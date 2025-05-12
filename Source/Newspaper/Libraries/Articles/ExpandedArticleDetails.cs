using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Libraries.Articles
{
    [XmlRoot(ElementName ="item")]
    public class ExpandedArticleDetails
    {
        [XmlElement("content:encoded")]
        public string? EncodedContent { get; set; }
        [XmlElement("dc:creator")]
        public string? Creator { get; set; }
        [XmlElement("media")]
        public Media? Media { get; set; }
    }
}
