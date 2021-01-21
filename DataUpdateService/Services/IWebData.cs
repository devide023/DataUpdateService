using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataUpdateService.Services
{
    public interface IWebData<T>
    {
        /// <summary>
        /// 页码连接保存到redis
        /// </summary>
        /// <param name="url"></param>
        void GetPageUrlToRedis(string url);
        /// <summary>
        /// 返回所有页码连接
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        List<string> GetPageUrl(string url);
        /// <summary>
        /// 获取每页要获取的项目
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        List<T> GetItemList(string url);
        /// <summary>
        /// 保存项目到数据库
        /// </summary>
        /// <param name="list"></param>
        void SaveData(List<T> list);
    }
}
