using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace moyskladApi
{
    internal class Product
    {
        private string url;
        private int count;
        private string id;
        public Product(string id,string url, int cout)
        {
            this.url = url;
            this.count = cout;
            this.id = id;
        }

        public string Url
        {
            get { return url; }
        }
        public int Count
        {
            get { return count; }
        }
        public string Id
        {
            get { return id; }
        }
    }
}
