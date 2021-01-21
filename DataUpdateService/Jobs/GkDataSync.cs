using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using log4net;
using Ivony.Html.Parser;
using Ivony.Html;
using System.Text.RegularExpressions;
using DataUpdateService.Services;
using DataUpdateService.Model;
using System.Configuration;

namespace DataUpdateService.Jobs
{
    public class GkDataSync : IJob
    {
        private string domainurl = string.Empty;
        private string rooturl = string.Empty;
        private int level = 0;
        private ILog log;
        public GkDataSync()
        {
            log = LogManager.GetLogger(this.GetType());
            ConfigurationManager.RefreshSection("appSettings");
            rooturl = ConfigurationManager.AppSettings["rooturl"];
            Regex reg = new Regex("(?<domain>.*://www.+?/.*?)");
            domainurl = reg.Match(rooturl).Groups["domain"].Value;
            domainurl = domainurl.Remove(domainurl.Length - 1);
            log.Info(domainurl);
            log.Info(rooturl);
        }
        public Task Execute(IJobExecutionContext context)
        {
            var task = Task.Run(() =>
            {
                log.Info("------开始数据抓取---------");
                FilmService service = new FilmService();
                var films = service.GetItemList(rooturl);
                service.SaveData(films);
                service.GetPageUrlToRedis(rooturl);
                service.Save();
                service.SaveErrorData();
                log.Info("------数据抓取完毕---------");
            });
            return task;
        }
    }
}
