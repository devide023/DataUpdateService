using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using log4net;
using System.Configuration;
namespace DataUpdateService
{
    public class SolrCommandHelper
    {
        public ILog log;
        public SolrCommandHelper()
        {
            log = LogManager.GetLogger(this.GetType());
        }

        public string InvokCommand(string command)
        {
            try
            {
                string user = ConfigurationManager.AppSettings["solruser"];
                string pwd = ConfigurationManager.AppSettings["solrpwd"];
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.CreateHttp(command);
                string credentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(user + ":" + pwd));
                webRequest.Headers.Add("Authorization", "Basic " + credentials);
                WebResponse response = webRequest.GetResponse();
                Stream stream = response.GetResponseStream();
                StreamReader sr = new StreamReader(stream, Encoding.UTF8);
                string result = sr.ReadToEnd();
                stream.Close();
                sr.Close();
                return result;
            }
            catch (Exception e)
            {
                log.Error(e.Message);
                return e.Message;
            }
        }

    }
}
