using Dapper;
using DataUpdateService.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using DataUpdateService.Model;

namespace DataUpdateService.Services
{
    public class Job
    {
        private ILog log;
        public Job()
        {
            log = LogManager.GetLogger(this.GetType());
        }
        public int SaveJobs(List<sys_job> jobs)
        {
            try
            {
                using (WorkDB db = new WorkDB())
                {
                    StringBuilder sql = new StringBuilder();
                    sql.Append("INSERT INTO sys_jobs \n");
                    sql.Append("( \n");
                    sql.Append("  jobid, \n");
                    sql.Append("  title, \n");
                    sql.Append("  `status` , \n");
                    sql.Append("  tag, \n");
                    sql.Append("  amount, \n");
                    sql.Append("  rq, \n");
                    sql.Append("  gq, \n");
                    sql.Append("  `desc` , \n");
                    sql.Append("  author, \n");
                    sql.Append("  `addtime` , \n");
                    sql.Append("  joburl, \n");
                    sql.Append("  number, \n");
                    sql.Append("  price_max, \n");
                    sql.Append("  price_min \n");
                    sql.Append(") \n");
                    sql.Append("select \n");
                    sql.Append("  @jobid, \n");
                    sql.Append("  @title, \n");
                    sql.Append("  @status, \n");
                    sql.Append("  @tag, \n");
                    sql.Append("  @amount, \n");
                    sql.Append("  @rq, \n");
                    sql.Append("  @gq, \n");
                    sql.Append("  @desc, \n");
                    sql.Append("  @author, \n");
                    sql.Append("  @addtime, \n");
                    sql.Append("  @joburl, \n");
                    sql.Append("  @number, \n");
                    sql.Append("  @price_max, \n");
                    sql.Append("  @price_min \n");
                    sql.Append(" where not exists (select * from sys_jobs where jobid = @jobid) \n");

                    return db.GetConn.Execute(sql.ToString(), jobs);
                }
            }
            catch (Exception e)
            {
                log.Error(e.Message);

                throw;
            }
        }
    }
}
