using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MistCore.Framework.Cached.RedisProvider
{
    /// <summary>
    /// RedisCache
    /// </summary>
    public class RedisCache : ICache
    {
        private IDistributedCache cache;

        /// <summary>
        /// RedisCache
        /// </summary>
        /// <param name="cache"></param>
        public RedisCache(IDistributedCache cache)
        {
            this.cache = cache;
        }

        /// <summary>
        /// Exist
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Exist(string key)
        {
            return cache.Get(key) != null;
        }

        /// <summary>
        /// Get
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Get(string key)
        {
            var buffer = cache.Get(key);
            if (buffer == null)
            {
                return null;
            }
            return Encoding.UTF8.GetString(buffer);
        }

        /// <summary>
        /// Get
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            var buffer = cache.Get(key);
            if (buffer == null)
            {
                return default(T);
            }
            var stringObject = Encoding.UTF8.GetString(buffer);

            var resValue = JsonConvert.DeserializeObject<T>(stringObject, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            });
            return resValue;
        }

        /// <summary>
        /// Remove
        /// </summary>
        /// <param name="key"></param>
        public bool Remove(string key)
        {
            if (cache.Get(key) != null)
            {
                cache.Remove(key);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Refresh
        /// </summary>
        /// <param name="key"></param>
        public void Refresh(string key)
        {
            cache.Refresh(key);
        }

        /// <summary>
        /// Set
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="slidingMillisecond"></param>
        /// <param name="absoluteTime"></param>
        public void Set<T>(string key, T value, int? slidingMillisecond = null, DateTime? absoluteTime = null)
        {
            var json = (value is string) ? Convert.ToString(value) : JsonConvert.SerializeObject(value, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.All
            });
            var buffer = Encoding.UTF8.GetBytes(json);

            var options = new DistributedCacheEntryOptions();
            if (slidingMillisecond != null)
            {
                options.SetSlidingExpiration(TimeSpan.FromMilliseconds(slidingMillisecond.Value));
                cache.Set(key, buffer, options);
            }
            else if(absoluteTime != null)
            {
                options.SetAbsoluteExpiration(absoluteTime.Value);
                cache.Set(key, buffer, options);
            }
            else
            {
                //这个插件存在的key 设置了过期时间无法设置为不过期，也就是下面的设置不会更新过期时间
                //需要清除过期时间需要删除后重新设置
                cache.Remove(key);
                cache.Set(key, buffer);
            }
        }

        /// <summary>
        /// GetOrAdd
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="func"></param>
        /// <param name="slidingMillisecond"></param>
        /// <param name="absoluteTime"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public T GetOrAdd<T>(string key, Func<string, T> func, int? slidingMillisecond = null, DateTime? absoluteTime = null)
        {
            var item = this.Get<T>(key);

            if (item != null)
            {
                return item;
            }
            else
            {
                var value = func(key);
                this.Set<T>(key, value, slidingMillisecond, absoluteTime);
                return value;
            }
        }
    }
}