using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Quartz;
using System.Net;
using System.Web;
using System.IO;
using Newtonsoft.Json;
using System.Configuration;
namespace DataUpdateService.Jobs
{
    public class SolrUpdate : IJob
    {
        private ILog log;
        public SolrUpdate()
        {
            log = LogManager.GetLogger(this.GetType());
        }

        public Task Execute(IJobExecutionContext context)
        {
            return Task.Run(() =>
            {
                try
                {
                    SolrCommandHelper helper = new SolrCommandHelper();
                    string command = ConfigurationManager.AppSettings["solrdeltadip"] ?? "";
                    string cmd_reload = ConfigurationManager.AppSettings["solrreloadcng"] ?? "";
                    if (!string.IsNullOrEmpty(cmd_reload))
                    {
                        helper.InvokCommand(cmd_reload);
                    }
                    if (!string.IsNullOrEmpty(command))
                    {
                        log.Info("----solr增量更新开始-----");
                        string result = helper.InvokCommand(command);
                        log.Info(result);
                        log.Info("----solr增量更新完成-----");
                    }
                }
                catch (Exception e)
                {
                    log.Error(e.Message);
                }
            });
        }
    }
}
