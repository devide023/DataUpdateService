using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using StackExchange.Redis;
namespace DataUpdateService.DB
{
    public static class RedisDb
    {
        private static IDatabase db;
        private static ConnectionMultiplexer redis;

        public static void InitDb()
        {
            string redisconn = ConfigurationManager.AppSettings["redis_conn"].ToString();
            redis = ConnectionMultiplexer.Connect(redisconn);
            db = redis.GetDatabase(0);
        }

        public static ConnectionMultiplexer GetRedis { get { return redis; } }
        public static IDatabase GetRedisDb { get { return db; } }
        /// <summary>
        /// 唯一值缓存
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public static void CacheDic_Set(Dictionary<string,List<string>> dic)
        {
            foreach (var item in dic)
            {
                for (int i = 0; i < item.Value.Count; i++)
                {
                    db.SetAdd(item.Key, item.Value[i]);
                }
                
            }
        }

    }
}
