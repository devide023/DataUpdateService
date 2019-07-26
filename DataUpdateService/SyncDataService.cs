using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using log4net;
using Quartz.Impl;
using DataUpdateService.Jobs;

namespace DataUpdateService
{
    public partial class SyncDataService : ServiceBase
    {
        private ILog logger;
        private IScheduler scheduler;
        public SyncDataService()
        {
            InitializeComponent();
            Init().GetAwaiter().GetResult();
        }

        private async Task Init()
        {
            //初始化
            logger = LogManager.GetLogger(this.GetType());
            //新建一个调度器工工厂
            ISchedulerFactory factory = new StdSchedulerFactory();
            //使用工厂生成一个调度器
            scheduler = await factory.GetScheduler();
        }

        protected override void OnStart(string[] args)
        {
            if (!scheduler.IsStarted)
            {
                //启动调度器
                scheduler.Start();
                //新建一个任务
                IJobDetail job = JobBuilder.Create<GkDataSync>()
                    .WithIdentity("GkDataSync", "GkDataSyncGroup")
                    .Build();
                //新建一个触发器
                ITrigger trigger = TriggerBuilder.Create()
                    .StartNow()
                    .WithCronSchedule("0/5 * * * * ?")
                    .Build();
                //将任务与触发器关联起来放到调度器中
                IJobDetail shiptocom_job = JobBuilder.Create<ShipToCompany>()
                    .WithIdentity("ShipToCompany", "GkDataSyncGroup")
                    .Build();
                ITrigger shiptocom_trigger = TriggerBuilder.Create()
                    .StartNow()
                    .WithCronSchedule("0/10 * * * * ?")
                    .Build();
                scheduler.ScheduleJob(job, trigger);
                scheduler.ScheduleJob(shiptocom_job, shiptocom_trigger);
                logger.Info("Quarzt 数据同步服务开启");
            }
        }

        protected override void OnStop()
        {
            if (!scheduler.IsShutdown)
            {
                scheduler.Shutdown();
            }
            base.OnStop();
        }
        protected override void OnPause()
        {
            scheduler.PauseAll();
            base.OnPause();
        }
        protected override void OnContinue()
        {
            scheduler.ResumeAll();
            base.OnContinue();
        }
    }
}
