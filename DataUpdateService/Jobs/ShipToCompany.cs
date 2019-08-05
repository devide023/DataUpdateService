using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using log4net;

namespace DataUpdateService
{
    public class ShipToCompany : IJob
    {
        private ILog log;
        public ShipToCompany()
        {
            log = LogManager.GetLogger(this.GetType());
        }
        public Task Execute(IJobExecutionContext context)
        {
            return Task.Run(() =>
            {
                log.Info(DateTime.Now.ToString());
            });
        }
    }
}
