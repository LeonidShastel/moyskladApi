using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace moyskladApi
{
    internal class Template
    {
        private string name;
        private string meta;
        public string Name
        {
            get { return name; }
        }
        public string Meta
        {
            get { return meta; }
        }
        public Template(string name, string meta)
        {
            this.name = name;
            this.meta = meta;
        }
    }
}
