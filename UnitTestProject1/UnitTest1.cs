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
using DataUpdateService.Services;
namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            try
            {
                DataUpdateService.Services.FilmService s = new DataUpdateService.Services.FilmService();
                string url = ConfigurationManager.AppSettings["redis_conn"];
                ConnectionMultiplexer conn = ConnectionMultiplexer.Connect(url);
                IDatabase db = conn.GetDatabase(0);
                long cnt = db.ListLength("error_infourl");
                for (int i = 0; i < cnt; i++)
                {
                    var item = db.ListLeftPop("error_infourl");
                    s.Get_FilmInfo(item);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }

        }
        [TestMethod]
        public void TestRRKF()
        {
            RRKFService service = new RRKFService();
            string url = "http://www.rrkf.com/serv/requestDetail?id=10544592-d0a3-4d0f-a6e1-aeb757378af1";

            service.GetJobInfo(url);
        }
    }
}
