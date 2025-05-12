using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libraries.Outputs
{
    public abstract class RenderedContent
    {
        public abstract void Save(string filename);
    }
}
