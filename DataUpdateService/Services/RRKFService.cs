using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataUpdateService.Model;
using Ivony.Html;
using Ivony.Html.Parser;
using StackExchange.Redis;
using log4net;
using DataUpdateService.DB;
using System.Text.RegularExpressions;
using System.Configuration;
namespace DataUpdateService.Services
{
    public class RRKFService : Job,IWebData<sys_job>
    {
        ILog log;
        private IDatabase db;
        string domain = string.Empty;
        private string rooturl = string.Empty;
        public RRKFService()
        {
            log = LogManager.GetLogger(this.GetType());
            RedisDb.InitDb();
            this.db = RedisDb.GetRedisDb;
            rooturl = ConfigurationManager.AppSettings["rrkf_url"];
            Regex reg = new Regex("(?<domain>.*://www.+?/.*?)");
            domain = reg.Match(rooturl).Groups["domain"].Value;
            domain = domain.Remove(domain.Length - 1);
        }
        public List<sys_job> GetItemList(string url)
        {
            try
            {
                List<sys_job> retlist = new List<sys_job>();
                IHtmlDocument html = new JumonyParser().LoadDocument(url, Encoding.UTF8);
                var list = html.Find("#r-list-wrapper .row .r-list");
                foreach (var item in list)
                {
                    var joblink = item.Find(".r-info a").FirstOrDefault().Attribute("href").Value();
                    var jobtitle = item.Find(".r-info a").FirstOrDefault().InnerText();
                    int pos = joblink.LastIndexOf("id=");
                    var jobid = joblink.Substring(pos+3,joblink.Length-(pos+3));
                    string joburl = domain + joblink;
                    //
                    bool isok = db.SetAdd("rrkf_jobid", jobid);
                    if (!isok)
                    {
                        continue;
                    }
                    var price1 = item.Find(".r-price").SingleOrDefault().InnerText();
                    var number = item.Find("div:nth-child(3)").SingleOrDefault().InnerText();
                    var status = item.Find("div:last-child span").SingleOrDefault().InnerText();
                    sys_job job = GetJobInfo(joburl);
                    if (job.title != null)
                    {
                        job.joburl = joburl;
                        job.jobid = jobid;
                        job.number = number;
                        job.status = status;
                        job.amount = price1;
                        job.addtime = DateTime.Now;
                        retlist.Add(job);
                    }
                    
                }
                return retlist;
            }
            catch (Exception e)
            {
                log.Error(url + "----" + e.Message);
                return new List<sys_job>();
            }
        }

        public sys_job GetJobInfo(string url)
        {
            try
            {
                IHtmlDocument html = new JumonyParser().LoadDocument(url, Encoding.UTF8);
                int pos = html.InnerHtml().IndexOf("product-info-summary");
                if (pos < 0)
                {
                    return new sys_job();
                }
                string jobtitle = html.Find(".product-info-summary .row h4").FirstOrDefault().InnerText();
                string author = html.Find(".product-info-summary .row small").FirstOrDefault().InnerText().Replace("发布者：", "");
                string price = html.Find(".product-info-summary .row .p-desc").FirstOrDefault().InnerText().Replace(" 预算： ", "");
                string rq = html.Find("#p-other ul li:first-child").FirstOrDefault().InnerText();
                string xqh = html.Find("#p-other ul li:nth-child(3)").SingleOrDefault().InnerText();
                string describe = html.Find("#wrap").SingleOrDefault().InnerHtml();
                string t = @"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>";
                describe = Regex.Replace(describe, t, "");
                describe = Regex.Replace(describe, "<.*?>", "").Replace("\t", "").Replace("\r", "").Replace("\n", "");
                return new sys_job
                {
                    title = jobtitle,
                    author = author,
                    desc = describe,
                    rq = rq,
                    tag = xqh,
                    price_min = price
                };
            }
            catch (Exception e)
            {
                log.Error(url + "----" + e.Message);
                return new sys_job();
            }
        }

        public List<string> GetPageUrl(string url)
        {
            try
            {
                url = rooturl;
                List<string> list = new List<string>();
                IHtmlDocument html = new JumonyParser().LoadDocument(url, Encoding.UTF8);
                string entityCount = html.Find("#entityCount").SingleOrDefault().Attribute("value").Value();
                string maxEntityPerPage = html.Find("#maxEntityPerPage").SingleOrDefault().Attribute("value").Value();
                string maxPagePerRow = html.Find("#maxPagePerRow").SingleOrDefault().Attribute("value").Value();
                string pageCount = html.Find("#pageCount").SingleOrDefault().Attribute("value").Value();
                string currentPage = html.Find("#currentPage").SingleOrDefault().Attribute("value").Value();
                string currentPageRow = html.Find("#currentPageRow").SingleOrDefault().Attribute("value").Value();
                string pageRowCount = html.Find("#pageRowCount").SingleOrDefault().Attribute("value").Value();
                Int32 count = Convert.ToInt32(pageCount);
                Int32 current = Convert.ToInt32(currentPage);
                string query = string.Empty;
                for (Int32 i = current; i <= count; i++)
                {
                    query = "entityCount=" + entityCount + "&maxEntityPerPage=" + maxEntityPerPage +
                    "&maxPagePerRow=" + maxPagePerRow + "&pageCount=" + pageCount +
                    "&currentPage=" + i + "&currentPageRow=" + currentPageRow +
                    "&pageRowCount=" + pageRowCount +
                    "&cBudget=0-1000000000&budgetTo=&statusBy=&categoryBy=&typeBy=&typeName=&orderByClause=a.c_postDate+desc";
                    string pageurl = url + "?" + query;
                    list.Add(pageurl);
                }
                return list;
            }
            catch (Exception e)
            {
                log.Error(e.Message);
                return new List<string>();
            }
        }

        public void GetPageUrlToRedis(string url)
        {
            
        }

        public void SaveData(List<sys_job> list)
        {
            throw new NotImplementedException();
        }
    }
}
