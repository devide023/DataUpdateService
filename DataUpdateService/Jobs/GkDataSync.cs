using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using log4net;
namespace DataUpdateService.Jobs
{
    public class GkDataSync : IJob
    {
        private ILog log;
        public GkDataSync()
        {
            log = LogManager.GetLogger(this.GetType());
        }
        public Task Execute(IJobExecutionContext context)
        {
            var task = Task.Run(() =>
            {
                //Tool.WriteLog(DateTime.Now.ToString());
                log.Info(DateTime.Now.ToString());
            });
            return task;
        }
    }
}
