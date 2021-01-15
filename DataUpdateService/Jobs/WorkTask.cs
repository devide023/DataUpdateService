using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using log4net;
using System.Text.RegularExpressions;
using Ivony.Html.Parser;
using Ivony.Html;

namespace DataUpdateService.Jobs
{
    public class WorkTask : IJob
    {
        private ILog log;
        public WorkTask()
        {
            log = LogManager.GetLogger(this.GetType());
        }
        public Task Execute(IJobExecutionContext context)
        {
            var task = Task.Run(() =>
            {
                log.Info("---------抓取任务数据-------------");
                string url = "https://www.clouderwork.com/jobs/project.html";
                Regex reg = new Regex(@".*/\d+.html");
                IHtmlDocument html = new JumonyParser().LoadDocument(url);
                html.Find(".page-div ul li a");
            });
            return task;
        }
    }
}
