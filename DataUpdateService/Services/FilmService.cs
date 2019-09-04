using DataUpdateService.DB;
using DataUpdateService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
namespace DataUpdateService.Services
{
    public class FilmService
    {
        public int AddFilm(List<sys_film> entrys)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("INSERT INTO dbo.sys_film \n");
            sql.Append("        ( link, title, txt,fromurl,[level],imdb,douban ) \n");
            sql.Append(" select @link,  \n");
            sql.Append("          @title,  \n");
            sql.Append("          @txt,@fromurl,@level,@imdb,@douban WHERE NOT EXISTS (SELECT * FROM sys_film WHERE link=@link) \n");

            using (FilmDB db = new FilmDB())
            {
               return db.FilmConn.Execute(sql.ToString(), entrys);
            }
        }
    }
}
