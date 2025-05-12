using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libraries
{
    public class ContentNode
    {
        public string? NodeType { get; set; }
        public string? PropertyName { get; set; }
        public string? PropertyValue { get; set; }

        public bool IsNull()
        {
            return NodeType == null && PropertyName == null && PropertyValue == null;
        }
    }
}
