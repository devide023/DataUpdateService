using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataUpdateService.Model
{
    public class sys_film
    {
        public int id { get; set; }
        public string link { get; set; }
        public string title { get; set; }
        public string txt { get; set; }
        public string fromurl { get; set; }
        public int level { get; set; }
        public int pid { get; set; }
        public string imdb { get; set; }
        public string douban { get; set; }
    }
}
