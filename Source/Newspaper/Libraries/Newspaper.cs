using Libraries.Articles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Libraries
{
    public class Newspaper
    {
        public string? Name { get; set; }
        public string? MainImage {  get; set; }
        public TimeSpan Frequency { get; set; }
        public List<Category> Categories { get; set; }
        public bool RenderImages { get; set; }

        public Newspaper()
        {
            Categories = new List<Category>();
        }

        public NewspaperContent GenerateContent(DateTime from)
        {
            var content = new NewspaperContent()
            {
                CategorisedArticles = GetCategorisedArticles(from),
                Timestamp = DateTime.Now,
                Paper = this,
            };

            return content;
        }

        private Dictionary<string, List<Article>> GetCategorisedArticles(DateTime from)
        {
            var visitedUrls = new List<string>();

            var results = new Dictionary<string, List<Article>>();

            foreach (var category in Categories)
            {
                results.Add(category.Name, category.GetArticles(from, visitedUrls));
            }

            return results;
        }

        public static void Save(string filename, Newspaper newspaper)
        {
            var serialised = JsonSerializer.Serialize<Newspaper>(newspaper);
            File.WriteAllText(filename, serialised);
        }

        public static Newspaper Load(string filename)
        {
            var serialised = File.ReadAllText(filename);
            return JsonSerializer.Deserialize<Newspaper>(serialised);
        }
    }
}
