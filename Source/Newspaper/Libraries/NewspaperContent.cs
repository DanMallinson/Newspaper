using Libraries.Articles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libraries
{
    public class NewspaperContent
    {
        public Newspaper? Paper {  get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, List<Article>>? CategorisedArticles { get; set; }
    }
}
