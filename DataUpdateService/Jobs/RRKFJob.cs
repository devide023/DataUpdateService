using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using log4net;
using DataUpdateService.Services;
using DataUpdateService.Model;
namespace DataUpdateService.Jobs
{
    public class RRKFJob : IJob
    {
        private ILog log;
        public RRKFJob()
        {
            log = LogManager.GetLogger(this.GetType());
        }
        public Task Execute(IJobExecutionContext context)
        {
            return Task.Run(() =>
            {
                log.Info("--开始采集人人开发网数据--");
                RRKFService rrkf = new RRKFService();
                var pageurls = rrkf.GetPageUrl("");
                foreach (var item in pageurls)
                {
                    var jobs = rrkf.GetItemList(item);
                    rrkf.SaveJobs(jobs);
                }
                log.Info("--人人开发网数据采集结束--");
            });
        }
    }
}
