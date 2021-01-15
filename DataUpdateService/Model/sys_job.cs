using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataUpdateService.Model
{
    public class sys_job
    {
        public int id { get; set; }
        public string jobid { get; set; }
        public string title { get; set; }
        public string status { get; set; }
        public string tag { get; set; }
        public string amount { get; set; }
        public string rq { get; set; }
        public string gq { get; set; }
        public string desc { get; set; }
        public string author { get; set; }
        public DateTime addtime { get; set; }
        public string joburl { get; set; }
        public string number { get; set; }
        public string price_min { get; set; }
        public string price_max { get; set; }
    }
}
