using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
namespace DataUpdateService
{
    public class ShipToCompany : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            return Task.Run(() =>
            {
                Tool.WriteLog(DateTime.Now.ToString() + "->" + this.GetType().ToString());
            });
        }
    }
}
