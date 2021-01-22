using DataUpdateService.DB;
using DataUpdateService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using log4net;
using StackExchange.Redis;
using Ivony.Html;
using Ivony.Html.Parser;
using System.Text.RegularExpressions;
using System.Configuration;

namespace DataUpdateService.Services
{
    public class FilmService : IWebData<sys_film>
    {
        ILog log;
        private IDatabase db;
        private string rooturl = string.Empty;
        private string domainurl = string.Empty;
        private string page_baseurl = string.Empty;
        private bool isfirstpage = true;
        private bool isendpage = false;
        int index = 0;
        int looplast_index = 0;
        public FilmService()
        {
            log = LogManager.GetLogger(this.GetType());
            RedisDb.InitDb();
            this.db = RedisDb.GetRedisDb;
            rooturl = ConfigurationManager.AppSettings["rooturl"];
            int pos = rooturl.LastIndexOf("/");
            page_baseurl = rooturl.Substring(0, pos+1);
            Regex reg = new Regex("(?<domain>.*://www.+?/.*?)");
            domainurl = reg.Match(rooturl).Groups["domain"].Value;
            domainurl = domainurl.Remove(domainurl.Length - 1);
        }

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

        public List<sys_film> GetItemList(string url)
        {
            try
            {
                List<sys_film> filmlist = new List<sys_film>();
                IHtmlDocument html = new JumonyParser().LoadDocument(url,Encoding.UTF8);
                var jobs = html.Find(".co_content8 ul table");
                foreach (var item in jobs)
                {
                    string filmurl = item.Find("a.ulink").FirstOrDefault().Attribute("href").Value();
                    string film_fullurl = domainurl + filmurl;
                    var films = Get_FilmInfo(film_fullurl);
                    filmlist.AddRange(films);
                }
                return filmlist;
            }
            catch (Exception e)
            {
                log.Error(url+"-----" + e.Message);
                this.db.ListLeftPush("error_pageurl", url);
                return new List<sys_film>();
            }
        }

        public List<sys_film> Get_FilmInfo(string url)
        {
            try
            {
                List<sys_film> filmlist = new List<sys_film>();
                IHtmlDocument source = new JumonyParser().LoadDocument(url);
                int pos = source.InnerHtml().IndexOf("Zoom");
                if (pos < 0) {
                    return filmlist;
                }
                //var list = source.Find("#Zoom a").Where(t => t.Attribute("href").Value().Contains("magnet:") || t.Attribute("href").Value().Contains("ftp:"));
                var list = source.Find("#Zoom a");
                var title_all = source.Find(".title_all h1 font").FirstOrDefault().InnerText();
                var desc = source.Find("#Zoom span").FirstOrDefault().InnerHtml();
                //评分提取
                Regex regpf = new Regex("(?<imdb>IMDb评分.*?<br />)");
                Regex regdb = new Regex("(?<douban>豆瓣评分.*?<br />)");
                var pfms = regpf.Match(desc);
                var pfdb = regdb.Match(desc);
                string imdb = pfms.Groups["imdb"].Value.Replace("IMDb评分", "").Replace("<br />", "").Trim();
                string douban = pfdb.Groups["douban"].Value.Replace("豆瓣评分", "").Replace("<br />", "").Trim();               
                foreach (var item in list)
                {
                    string filmlink = item.Attribute("href").Value();
                    if (filmlink == null)
                    {
                        continue;
                    }
                    if (filmlink.Contains("magnet:") || filmlink.Contains("ftp:"))
                    {
                        bool isok = db.SetAdd("filmlink", filmlink);
                        if (isok)
                        {
                            filmlist.Add(new sys_film() { link = filmlink, title = title_all, txt = desc, fromurl = url, imdb = imdb, douban = douban });
                        }
                    }
                }

                return filmlist;
            }
            catch (Exception e)
            {
                log.Error(url+"----"+e.Message);
                this.db.ListLeftPush("error_infourl", url);
                return new List<sys_film>();
            }
        }

        public List<string> GetPageUrl(string url)
        {
           RedisValue[] list =  db.SortedSetRangeByRank("filmpageurl");
            List<string> ret = new List<string>();
            foreach (var item in list)
            {
                ret.Add(item);
            }
            return ret;
        }

        public void GetPageUrlToRedis(string url)
        {
            try
            {
                IHtmlDocument html = new JumonyParser().LoadDocument(url);
                var pagelist = html.Find(".co_content8 .x a");
                var last_index = pagelist.Count() - 3;
                var end_index = pagelist.Count() - 1;
                var last_url = pagelist.ToList()[last_index].Attribute("href").Value();
                var end_txt = pagelist.ToList()[end_index].InnerText();
                this.isendpage = end_txt.IndexOf("末页") >= 0 ? false : true;
                looplast_index = isendpage ? pagelist.Count() : pagelist.Count() - 1;
                var last_full_url = page_baseurl + last_url;
                int i = 0;
                i = isfirstpage ? 0 : 2;
                for (; i < looplast_index; i++)
                {
                    IHtmlElement item = pagelist.ToList()[i];
                    string pageurl = item.Attribute("href").Value();
                    string pagefullurl = page_baseurl + pageurl;
                    db.SortedSetAdd("filmpageurl", pagefullurl, (double)index++);
                    if (i == last_index && !isendpage)
                    {
                        isfirstpage = false;
                        GetPageUrl(pagefullurl);
                    }
                }
            }
            catch (Exception e)
            {
                log.Error(e.Message);
                throw;
            }
        }

        public void SaveData(List<sys_film> list)
        {
            this.AddFilm(list);
        }

        public void Save()
        {
            RedisValue[] list = db.SortedSetRangeByRank("filmpageurl");
            foreach (var item in list)
            {
               var films = GetItemList(item);
                AddFilm(films);
            }
        }

        public void SaveErrorData()
        {
            string url = string.Empty;
            try
            {
                List<sys_film> fs = new List<sys_film>();
                long errors = db.ListLength("error_infourl");
                for (int i = 0; i < errors; i++)
                {
                    url = db.ListLeftPop("error_infourl");
                    fs.AddRange(Get_FilmInfo(url));
                }
                AddFilm(fs);
            }
            catch (Exception e)
            {
                log.Error(url + "----" + e.Message);
                this.db.ListRightPush("error_infourl", url);
            }
        }
    }
}
