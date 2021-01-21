using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;
using Ivony.Html;
using Ivony.Html.Parser;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using DataUpdateService.Model;
using StackExchange.Redis;
using System.Configuration;
using DataUpdateService.DB;
namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        string domain = "https://www.yuanjisong.com";
        HashSet<string> pageurl_hs = new HashSet<string>();
        HashSet<string> joburl_hs = new HashSet<string>();
        List<sys_job> joblist = new List<sys_job>();
        string lasturl = "";
        string end_page_url = "";
        int level = 0;
        [TestMethod]
        public void TestRedis()
        {
            DataUpdateService.Services.JobService jobs = new DataUpdateService.Services.JobService();
            jobs.SaveJobs();
        }
        [TestMethod]
        public void TestMethod1()
        {
            try
            {
                string url = "https://www.ygdy8.net/html/gndy/dyzz/list_23_9.html";
                string infourl = "https://www.ygdy8.net/html/gndy/dyzz/20200731/60279.html";
                DataUpdateService.Services.FilmService service = new DataUpdateService.Services.FilmService();
                //service.Get_FilmInfo(infourl);
                //string rooturl = ConfigurationManager.AppSettings["rooturl"];
                service.Save();
                //var films = service.GetItemList(url);
                //service.SaveData(films);
                //service.Save();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }

        }
        [TestMethod]
        public void TestJobs()
        {
            List<sys_job> list = new List<sys_job>();
            list.Add(new sys_job
            {
                jobid="234",
                author="devide",
                joburl="tets.com",
                addtime=DateTime.Now,
                amount="1000",
                desc="asdasdf",
                gq="10天",
                number="3人投标",
                status="招募",
                tag="项目制",
                rq="2020-12-31",
                title="微信开发",
                price_max="3000",
                price_min="2000"

            });
            DataUpdateService.Services.JobService jobs = new DataUpdateService.Services.JobService();
            jobs.SaveJobs(list);

        }
        private void PageUrlList(string url)
        {
            IHtmlDocument html = new JumonyParser().LoadDocument(url);
            var pagelist = html.Find(".page-div nav ul li a");
            var size = pagelist.Count();
            IHtmlElement lastpage_el = pagelist.ToList()[size - 2];
            string lastpageurl = lastpage_el.Attribute("href").Value();
            string lastpage_fullurl = domain + lastpageurl;
            end_page_url = domain + pagelist.ToList()[size - 1].Attribute("href").Value();
            for (int i = 0; i < size-1; i++)
            {
                var item = pagelist.ToList()[i];
                string page_fullurl = domain + item.Attribute("href").Value();
                pageurl_hs.Add(page_fullurl);
            }
            if(lasturl!= lastpage_fullurl)
            {
                lasturl = lastpage_fullurl;
                PageUrlList(lastpage_fullurl);
            }
            pageurl_hs.Add(end_page_url);
        }
        private void SubPageUrl(string url)
        {
            if (level > 1) return;
            IHtmlDocument html = new JumonyParser().LoadDocument(url);
            var pagelist = html.Find(".page-div nav ul li a");
            foreach (var item in pagelist)
            {
                string page_fullurl = domain + item.Attribute("href").Value();
                if (pageurl_hs.Contains(page_fullurl))
                {
                    continue;
                }
                pageurl_hs.Add(page_fullurl);
                level++;
                SubPageUrl(page_fullurl);
                level--;
            }
        }
        private void Get_PageUrl(string url)
        {
            try
            {
                if (level > 5 ) return;
                IHtmlDocument html = new JumonyParser().LoadDocument(url);
                var pagelist = html.Find(".page-div nav ul li a[href]");
                foreach (var item in pagelist)
                {
                    string page_fullurl = domain + item.Attribute("href").Value();
                    if (pageurl_hs.Contains(page_fullurl))
                    {
                        continue;
                    }
                    joblist.AddRange(Get_Jobs(page_fullurl));
                    pageurl_hs.Add(page_fullurl);
                    level++;
                    Get_PageUrl(page_fullurl);
                    level--;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        private List<sys_job> Get_Jobs(string url)
        {

            try
            {
                IHtmlDocument html = new JumonyParser().LoadDocument(url);
                var jobs = html.Find(".search-result a.job-item");
                List<sys_job> retlist = new List<sys_job>();
                foreach (var item in jobs)
                {
                    string joburl = item.Attribute("href").Value();
                    string full_joburl = domain + item.Attribute("href").Value();
                    if (joburl_hs.Contains(full_joburl))
                    {
                        continue;
                    }
                    joburl_hs.Add(full_joburl);
                    int pos1 = joburl.LastIndexOf("/");
                    int pos2 = joburl.LastIndexOf(".");
                    string jobidstr = joburl.Substring(pos1 + 1, pos2 - (pos1 + 1));
                    var dic = getjobdesc(full_joburl);
                    sys_job sysjob = new sys_job
                    {
                        addtime = DateTime.Now,
                        amount = item.Find(".money").FirstOrDefault().InnerText().Replace("预算：￥", ""),
                        author = dic["zz"],
                        desc = Regex.Replace(dic["desc"], "<.*?>", ""),
                        gq = item.Find(".period").FirstOrDefault().InnerText().Replace("工期：", ""),
                        jobid = jobidstr,
                        joburl = full_joburl,
                        number = dic["number"],
                        rq = item.Find(".publish_at").FirstOrDefault().InnerText().Replace("发布时间：", ""),
                        status = item.Find(".work_status").FirstOrDefault().InnerText(),
                        tag = item.Find(".pattern").FirstOrDefault().InnerText(),
                        title = item.Find(".job-title div").FirstOrDefault().InnerText(),
                        price_min = dic.ContainsKey("price_1")?dic["price_1"]:"",
                        price_max = dic.ContainsKey("price_2") ? dic["price_2"] : ""
                    };
                    retlist.Add(sysjob);
                }
                return retlist;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        private Dictionary<string,string> getjobdesc(string url)
        {
            try
            {
                IHtmlDocument html = new JumonyParser().LoadDocument(url);
                Regex jgr = new Regex("(?<jg>\\d+)");
                var desc = html.Find(".main-detail .desc").FirstOrDefault().InnerHtml();
                var zz = html.Find(".name a").FirstOrDefault().InnerText();
                var number = html.Find(".main-top .number").FirstOrDefault().InnerText();
                var price = html.Find(".main-top .detail-row .budgets .budget span").FirstOrDefault().InnerText();
                var prices = jgr.Matches(price);
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("desc", desc);
                dic.Add("zz", zz);
                dic.Add("number", number);
                if (prices.Count >0)
                {
                    for (int i = 0; i < prices.Count; i++)
                    {
                        dic.Add("price_"+(i+1),prices[i].Groups["jg"].Value);
                    }
                }
                else
                {
                    dic.Add("price_min", "");
                    dic.Add("price_max", "");
                }
                return dic;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }
        
    }
}
