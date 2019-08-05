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
            logger = LogManager.GetLogger(this.GetType());
            Init().GetAwaiter().GetResult();
        }

        private async Task Init()
        {
            ISchedulerFactory factory = new StdSchedulerFactory();
            scheduler = await factory.GetScheduler();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                if (!scheduler.IsStarted)
                {
                    //启动调度器
                    scheduler.Start();
                    logger.Info("Quarzt 数据同步服务开启");
                }
            }
            catch (Exception e)
            {
                Tool.WriteLog(e.Message);
                base.OnStop();
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
