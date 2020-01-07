using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BiqugeSpeeker
{
    public class RedisConfigInfo
    {
        //唯一实例
        private static RedisConfigInfo uniqueInstance;

        //public static int RedisMaxReadPool = int.Parse(ConfigurationManager.AppSettings["redis_max_read_pool"]);

        //public static int RedisMaxWritePool = int.Parse(ConfigurationManager.AppSettings["redis_max_write_pool"]);
        //定义一个标识确保线程同步

        private static readonly object locker = new object();

        private readonly string[] redisHosts = null;
        //链接池管理对象
        private PooledRedisClientManager _pool;
        //私有构造方法
        private RedisConfigInfo()
        {
            //创建连接池管理对象
            var redisHostStr = System.Configuration.ConfigurationManager.AppSettings["Redis"];
            redisHosts = redisHostStr.Split(',');
            CreateRedisPoolManager(redisHostStr.Split(','), redisHostStr.Split(','));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="redisWriteHost"></param>
        /// <param name="redisReadHost"></param>
        private void CreateRedisPoolManager(string[] redisWriteHost, string[] redisReadHost)
        {
            _pool = new PooledRedisClientManager(redisWriteHost, redisReadHost, new RedisClientManagerConfig()
            {
                MaxWritePoolSize = redisHosts.Length * 5,
                MaxReadPoolSize = redisHosts.Length * 5,
                AutoStart = true
            });

        }

        //唯一全局访问点
        public static RedisConfigInfo GetRedisConfigInfo()
        {
            //双重锁定
            if (uniqueInstance == null)
            {
                lock (locker)
                {
                    if (uniqueInstance == null)
                    {
                        uniqueInstance = new RedisConfigInfo();
                    }
                }
            }
            return uniqueInstance;
        }

        public T _GetKey<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return default(T);
            }
            T obj = default(T);
            try
            {
                if (_pool != null)
                {
                    using (var r = _pool.GetClient())
                    {
                        if (r != null)
                        {
                            r.SendTimeout = 1000;
                            obj = r.Get<T>(key);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format("{0}:{1}发生异常!{2}", "cache", "获取", key);
            }
            return obj;
        }

        public bool _AddKey<T>(string key, T value)
        {
            if (value == null)
            {
                return false;
            }

            try
            {
                if (_pool != null)
                {
                    using (var r = _pool.GetClient())
                    {
                        if (r != null)
                        {
                            r.SendTimeout = 1000;
                            r.Set(key, value);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format("{0}:{1}发生异常!{2}", "cache", "存储", key);
            }
            return false;
        }

        public bool _AddKey<T>(string key, T value,TimeSpan timeSpan)
        {
            if (value == null)
            {
                return false;
            }

            try
            {
                if (_pool != null)
                {
                    using (var r = _pool.GetClient())
                    {
                        if (r != null)
                        {
                            r.SendTimeout = 1000;
                            r.Set(key, value,timeSpan);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format("{0}:{1}发生异常!{2}", "cache", "存储", key);
            }
            return false;
        }

        public bool _Clear(string key)
        {
            try
            {
                if (_pool != null)
                {
                    using (var r = _pool.GetClient())
                    {
                        string val= r.Get<string>(key);
                        if (r != null)
                        {
                            bool result= r.Remove(key);
                            return result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format("{0}:{1}发生异常!{2}", "cache", "移除", key);
            }
            return false;
        }


    }
}
