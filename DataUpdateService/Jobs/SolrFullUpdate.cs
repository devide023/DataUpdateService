using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using Quartz;
using System.Configuration;

namespace DataUpdateService.Jobs
{
    public class SolrFullUpdate:IJob
    {
        private ILog log;
        public SolrFullUpdate()
        {
            log = LogManager.GetLogger(this.GetType());
        }

        public Task Execute(IJobExecutionContext context)
        {
            return Task.Run(() =>
            {
                SolrCommandHelper helper = new SolrCommandHelper();
                string command = ConfigurationManager.AppSettings["solrfulldip"] ?? "";
                string cmd_reload = ConfigurationManager.AppSettings["solrreloadcng"] ?? "";
                if(!string.IsNullOrEmpty(cmd_reload))
                {
                    helper.InvokCommand(cmd_reload);
                }
                if (!string.IsNullOrEmpty(command))
                {
                    log.Info("----solr全量更新开始-----");                    
                    string result = helper.InvokCommand(command);
                    log.Info(result);
                    log.Info("----solr全量更新完成-----");
                }
            });
        }
    }
}
