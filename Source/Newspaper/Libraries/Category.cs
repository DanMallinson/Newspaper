using Libraries.Articles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libraries
{
    public class Category
    {
        public string? Name { get; set; }
        public List<Source> Sources { get; set; }

        public Category()
        {
            Sources = new List<Source>();
        }

        public List<Article> GetArticles(DateTime from ,List<string> visitedUrls)
        {
            var articles = new List<Article>();

            foreach(var source in Sources)
            {
                articles.AddRange(source.GetArticles(from, visitedUrls));
            }

            return articles;
        }
    }
}
