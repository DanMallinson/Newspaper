using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libraries.Articles
{
    public class ParseOptions
    {
        public bool IncludeImages { get; set; }
        public bool IncludeTables { get; set; }
        public ContentNode ContentNode { get; set; }

        public ParseOptions() 
        {
            ContentNode = new ContentNode();
        }
    }
}
