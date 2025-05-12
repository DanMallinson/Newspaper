using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Libraries.Articles
{
    [XmlRoot(ElementName ="item")]
    public class Media
    {
        [XmlAttribute("url")]
        public string? Url { get; set; }
        [XmlAttribute("type")]
        public string? Type { get; set; }
    }
}
