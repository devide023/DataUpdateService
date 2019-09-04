using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using log4net;
using Ivony.Html.Parser;
using Ivony.Html;
using System.Text.RegularExpressions;
using DataUpdateService.Services;
using DataUpdateService.Model;
using System.Configuration;

namespace DataUpdateService.Jobs
{
    public class GkDataSync : IJob
    {
        private Dictionary<string, dynamic> dic = new Dictionary<string, dynamic>();
        private List<sys_film> downloadlist = new List<sys_film>();
        private string domainurl = string.Empty;
        private string rooturl = string.Empty;
        private int level = 0;
        private int depth = 10;
        private string findexp = "a";
        private ILog log;
        public GkDataSync()
        {
            log = LogManager.GetLogger(this.GetType());
            ConfigurationManager.RefreshSection("appSettings");
            depth = ConfigurationManager.AppSettings["depth"] != null ? Convert.ToInt32(ConfigurationManager.AppSettings["depth"].ToString()) : 10;
            rooturl = ConfigurationManager.AppSettings["rooturl"];
            findexp = ConfigurationManager.AppSettings["findexp"];
            Regex reg = new Regex("(?<domain>.*://www.+?/.*?)");
            domainurl = reg.Match(rooturl).Groups["domain"].Value;
            domainurl = domainurl.Remove(domainurl.Length - 1);
            log.Info(domainurl);
            log.Info(rooturl);
            log.Info("遍历深度：" + depth);
        }
        public Task Execute(IJobExecutionContext context)
        {
            var task = Task.Run(() =>
            {
                Regex reg = new Regex(@".*/\d+.html");
                IHtmlDocument source = new JumonyParser().LoadDocument(rooturl);
                var list = source.Find(findexp);
                foreach (var item in list)
                {
                    try
                    {
                        string link = item.Attribute("href").Value().ToLower();
                        if (link != null && link != "#")
                        {
                            string findurl = string.Empty;
                            if (link.StartsWith("/"))
                            {
                                findurl = domainurl + link;
                            }
                            else
                            {
                                findurl = link;
                                if (link.StartsWith("list_"))
                                {
                                    var m = new Regex("(?<base>.*/)").Match(rooturl);
                                    findurl = m.Groups["base"].Value + link;
                                }
                            }
                            string txt = item.InnerText();
                            dic.Add(findurl, new { link = findurl, txt = txt });
                            if (reg.IsMatch(link))
                            {
                                GetSourceInfo(findurl);
                            }
                            else
                            {
                                level++;
                                SubSite(findurl);
                                level--;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
                FilmService service = new FilmService();
                service.AddFilm(downloadlist);
                log.Info("------数据抓取完毕---------");
            });
            return task;
        }

        private void GetSourceInfo(string url)
        {
            IHtmlDocument source = new JumonyParser().LoadDocument(url);
            var list = source.Find("#Zoom a").Where(t => t.Attribute("href").Value().Contains("magnet:") || t.Attribute("href").Value().Contains("ftp:"));
            var title = source.Find("title").FirstOrDefault();
            var title_all = source.Find(".title_all h1 font").FirstOrDefault().InnerText();
            var desc = source.Find("#Zoom span p:first-child").FirstOrDefault().InnerHtml();
            //评分提取
            Regex regpf = new Regex("(?<imdb>IMDb评分.*?<br />)");
            Regex regdb = new Regex("(?<douban>豆瓣评分.*?<br />)");
            var pfms = regpf.Match(desc);
            var pfdb = regdb.Match(desc);
            string imdb = pfms.Groups["imdb"].Value.Replace("IMDb评分", "").Replace("<br />", "").Trim();
            string douban = pfdb.Groups["douban"].Value.Replace("豆瓣评分", "").Replace("<br />", "").Trim();
            foreach (var item in list)
            {
                downloadlist.Add(new sys_film() { link = item.Attribute("href").Value(), title = title_all, txt = desc, fromurl = url,level=level,imdb=imdb,douban=douban });
            }
        }
        private void SubSite(string url)
        {
            if (level <= depth)
            {
                Regex reg = new Regex(@".*/\d+.html");
                IHtmlDocument source = new JumonyParser().LoadDocument(url);
                var list = source.Find(findexp);
                var title = source.Find("title").FirstOrDefault();
                string name = title.InnerText();
                foreach (var item in list)
                {
                    try
                    {
                        string link = item.Attribute("href").Value().ToLower();
                        if (link != null && link != "#")
                        {
                            string findurl = string.Empty;
                            if (link.StartsWith("/"))
                            {
                                findurl = domainurl + link;
                            }
                            else
                            {
                                findurl = link;
                                if (link.StartsWith("list_"))
                                {
                                    var m = new Regex("(?<base>.*/)").Match(url);
                                    findurl = m.Groups["base"].Value + link;
                                }
                            }
                            string txt = item.InnerText();
                            dic.Add(findurl, new { link = findurl, txt = txt });
                            if (reg.IsMatch(link))
                            {
                                GetSourceInfo(findurl);
                            }
                            else
                            {
                                level++;
                                SubSite(findurl);
                                level--;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
        }
    }
}
