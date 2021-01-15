using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
namespace DataUpdateService.DB
{
    public interface IMyDB
    {
        MySqlConnection GetConn { get; }
    }
}
