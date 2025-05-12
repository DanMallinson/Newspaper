using Libraries.Articles.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Libraries.Articles
{
    public abstract class Article
    {
        public string? ID { get; set; }
        public string? Source { get; set; }
        public string? Title { get; set; }
        public string? Author { get; set; }
        public List<Block> Content { get; private set; }
        public string? Thumbnail { get; set; }

        public Article()
        {
            Content = new List<Block>();
        }

        public void Parse(ArticleDetails details, Source source)
        {
            OnParse(details, source);
        }

        public bool IsEmpty()
        {
            return Content.Count == 0;
        }

        protected abstract void OnParse(ArticleDetails details, Source source);
    }
}
