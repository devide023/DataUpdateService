using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Configuration;

namespace DataUpdateService.DB
{
    public class WorkDB : IMyDB,IDisposable
    {
        private MySqlConnection conn;
        public WorkDB()
        {
            string connstr = ConfigurationManager.AppSettings["work_conn"] != null ? ConfigurationManager.AppSettings["work_conn"].ToString() : "";
            this.conn = new MySqlConnection(connstr);
        }
        public WorkDB(string connstr)
        {
            this.conn = new MySqlConnection(connstr);
        }
        public MySqlConnection GetConn
        {
            get
            {
                return conn;
            }
        }

        public void Dispose()
        {
            
        }
    }
}
