using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using log4net;
using System.Configuration;
using DataUpdateService.Services;

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
                JobService jobservice = new JobService();
                string url = ConfigurationManager.AppSettings["joburl"];
                log.Info("--工作任务采集开始--");
                jobservice.PageUrlList(url);
                int ret = jobservice.SaveJobs();
                log.Info("--工作任务采集完成--");
            });
            return task;
        }
    }
}
