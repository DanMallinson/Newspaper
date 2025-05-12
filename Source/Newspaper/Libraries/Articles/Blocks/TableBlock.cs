using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libraries.Articles.Blocks
{
    public class TableBlock : Block
    {
        public List<Columns> Content {  get; set; }
        public int GetMaxColumns()
        {
            var result = 0;

            foreach(var row in Content)
            {
                result = Math.Max(result, row.Count);
            }

            return result;
        }

        public TableBlock()
        {
            Content = new List<Columns>();
        }
    }

    public class Columns : List<string>
    {

    }
}
