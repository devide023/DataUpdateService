using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;

namespace DataUpdateService.Jobs
{
    public class GkDataSync : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            var task = Task.Run(() =>
            {
                Tool.WriteLog(DateTime.Now.ToString());
            });
            return task;
        }
    }
}
