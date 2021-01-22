using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using DataUpdateService.Model;
using DataUpdateService.DB;
using log4net;
using StackExchange.Redis;
using Ivony.Html;
using Ivony.Html.Parser;
using System.Text.RegularExpressions;

namespace DataUpdateService.Services
{
    public class JobService:Job
    {
        private ILog log;
        private IDatabase db;
        string domain = "https://www.clouderwork.com";
        string lasturl = "";
        int index = 1;
        int firstpage = 0;
        bool yjsfirstpage = true;
        bool yjs_endflag = false;
        public JobService()
        {
            log = LogManager.GetLogger(this.GetType());
            RedisDb.InitDb();
            this.db = RedisDb.GetRedisDb;
        }
        public void YjsPageUrlList(string url)
        {
            try
            {
                IHtmlDocument html = new JumonyParser().LoadDocument(url);
                var pagelist = html.Find("nav.page_nav_div ul.pagination_webpage li a");
                var pagenext = html.Find("nav.page_nav_div ul.pagination_webpage li.disabled");
                foreach (var item in pagenext)
                {
                    string pagetxt = Regex.Replace(item.InnerHtml(), "<.*?>", "");
                    if (pagetxt.IndexOf("下一页") >= 0)
                    {
                        yjs_endflag = true;
                    }
                }
                var size = pagelist.Count();
                int i = 0;
                i = yjsfirstpage ? 0 : 2;
                for (; i < size - 1; i++)
                {
                    var item = pagelist.ToList()[i];
                    string pageurl = item.Attribute("href").Value();
                    this.db.SortedSetAdd("yjspageurl", pageurl, (double)index++);
                    if (i == size - 2 && !yjs_endflag)
                    {
                        yjsfirstpage = false;
                        YjsPageUrlList(pageurl);
                    }
                }
            }
            catch (Exception e)
            {
                log.Error(e.Message);
                throw;
            }
        }

        public List<sys_job> YjsJobs(string url)
        {
            try
            {
                string regtxt = "<.*?>";
                IHtmlDocument html = new JumonyParser().LoadDocument(url);
                var jobs = html.Find("#db_adapt_id .weui_panel");
                List<sys_job> listjob = new List<sys_job>();
                foreach (var item in jobs)
                {
                    string joburl = item.Find("a").FirstOrDefault().Attribute("href").Value();
                    int pos1 = joburl.LastIndexOf("/");
                    string jobid = joburl.Substring(pos1 + 1, joburl.Length - (pos1 + 1));
                    bool isok = this.db.SetAdd("yjsjobid", jobid);
                    if (!isok)
                    {
                        continue;
                    }
                    string jobtitle = item.Find("a .topic_title")!=null? item.Find("a .topic_title").FirstOrDefault().InnerText():"";
                    string jobdesc = string.Empty;
                    string jobgs = string.Empty;
                    string jobprice = string.Empty;
                    var subitems = item.Find(".job_list_item_div .media_desc_adapt");
                    string author = item.Find("h4.weui_media_title ")!=null? item.Find("h4.weui_media_title ").FirstOrDefault().InnerText():"";
                    bool isover = item.ToString().IndexOf("zhushi_span") > 0 ? false : true;
                    if (isover)
                    {
                        continue;
                    }
                    string numberhtml = item.Find("span.zhushi_span") != null ? item.Find("span.zhushi_span").FirstOrDefault().InnerHtml() : "";
                    string number = Regex.Replace(numberhtml, regtxt, "");
                    foreach (var subitem in subitems)
                    {
                        string subitemhtml = subitem.InnerHtml();
                        if (subitemhtml.IndexOf("glyphicon-th-large") >= 0)
                        {
                            jobdesc = Regex.Replace(subitemhtml, regtxt, "").Replace("描述：", "");
                        }
                        if (subitemhtml.IndexOf("glyphicon-hourglass") >= 0)
                        {
                            jobgs = Regex.Replace(subitemhtml, regtxt, "").Replace("工时：", "");
                        }
                        if (subitemhtml.IndexOf("glyphicon-yen") >= 0)
                        {
                            jobprice = Regex.Replace(subitemhtml, regtxt, "").Replace("总价：", "").Replace("元", "");
                        }
                    }
                    sys_job jobentry = new sys_job
                    {
                        jobid = jobid,
                        title = jobtitle,
                        desc = jobdesc,
                        number = number,
                        joburl = joburl,
                        addtime = DateTime.Now,
                        amount = jobprice,
                        author = author,
                        gq = jobgs
                    };
                    listjob.Add(jobentry);
                }
                return listjob;
            }
            catch (Exception e)
            {
                log.Error(e.Message);
                throw;
            }

        }
        /// <summary>
        /// 获取页码连接保存到redis
        /// </summary>
        /// <param name="url"></param>
        public void PageUrlList(string url)
        {
            IHtmlDocument html = new JumonyParser().LoadDocument(url);
            var pagelist = html.Find(".page-div nav ul li a");
            var size = pagelist.Count();
            IHtmlElement lastpage_el = pagelist.ToList()[size - 2];
            string lastpageurl = lastpage_el.Attribute("href").Value();
            string lastpage_fullurl = domain + lastpageurl;
            string end_page_url = domain + pagelist.ToList()[size - 1].Attribute("href").Value();
            for (int i = firstpage; i < size - 1; i++)
            {
                var item = pagelist.ToList()[i];
                string page_fullurl = domain + item.Attribute("href").Value();
                this.db.SortedSetAdd("pageurl", page_fullurl, (double)index++);
            }
            if (lasturl != lastpage_fullurl)
            {
                lasturl = lastpage_fullurl;
                firstpage = 1;
                PageUrlList(lastpage_fullurl);
            }
            this.db.SortedSetAdd("pageurl", end_page_url, (double)index++);
        }
        /// <summary>
        /// 获取每页工作内容,保存工作id到redis
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public List<sys_job> Get_Jobs(string url)
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
                    int pos1 = joburl.LastIndexOf("/");
                    int pos2 = joburl.LastIndexOf(".");
                    string jobidstr = joburl.Substring(pos1 + 1, pos2 - (pos1 + 1));
                    bool isok = db.SetAdd("jobid", jobidstr);
                    if (!isok)
                    {
                        continue;
                    }
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
                        price_min = dic.ContainsKey("price_1") ? dic["price_1"] : "",
                        price_max = dic.ContainsKey("price_2") ? dic["price_2"] : ""
                    };
                    retlist.Add(sysjob);
                }
                return retlist;
            }
            catch (Exception e)
            {
                log.Error(e.Message);
                throw;
            }
        }
        /// <summary>
        /// 获取工作描述
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private Dictionary<string, string> getjobdesc(string url)
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
                if (prices.Count > 0)
                {
                    for (int i = 0; i < prices.Count; i++)
                    {
                        dic.Add("price_" + (i + 1), prices[i].Groups["jg"].Value);
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
                log.Error(e.Message);
                throw;
            }
        }
        /// <summary>
        /// 从redis数据库获取每页任务保存到mysql数据库
        /// </summary>
        /// <returns></returns>
        public int SaveJobs()
        {
            RedisValue[] list = db.SortedSetRangeByRank("pageurl");
            List<int> errors = new List<int>();
            foreach (var item in list)
            {
                List<sys_job> joblist = Get_Jobs(item);
                int ret = SaveJobs(joblist);
                errors.Add(ret);
            }
            return errors.Where(t => t == 0).Count();
        }

        public void SaveYjsJobs()
        {
            try
            {
                RedisValue[] list = db.SortedSetRangeByRank("yjspageurl");
                foreach (var item in list)
                {
                    List<sys_job> joblist = YjsJobs(item);
                    SaveJobs(joblist);
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
