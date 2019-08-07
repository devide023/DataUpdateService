using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data.SqlClient;
using System.Configuration;
namespace DataUpdateService.DB
{
    public class FilmDB : IDisposable
    {
        private string str_conn = string.Empty;
        public FilmDB()
        {
            str_conn = ConfigurationManager.AppSettings["film_conn"];
        }
        public SqlConnection FilmConn
        {
            get
            {
                return new SqlConnection(str_conn);
            }
        }

        public void Dispose()
        {

        }
    }
}
