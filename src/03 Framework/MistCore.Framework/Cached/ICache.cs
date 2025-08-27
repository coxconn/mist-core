using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MistCore.Framework.Cached
{
    /// <summary>
    /// ICache
    /// </summary>
    public interface ICache
    {

        /// <summary>
        /// Exist
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool Exist(string key);

        /// <summary>
        /// Get
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        string Get(string key);

        /// <summary>
        /// Get
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        T Get<T>(string key);

        /// <summary>
        /// Remove
        /// </summary>
        /// <param name="key"></param>
        bool Remove(string key);

        /// <summary>
        /// Refresh
        /// </summary>
        /// <param name="key"></param>
        void Refresh(string key);

        /// <summary>
        /// Set
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="slidingMillisecond"></param>
        /// <param name="absoluteTime"></param>
        void Set<T>(string key, T value, int? slidingMillisecond = null, DateTime? absoluteTime = null);

        /// <summary>
        /// GetOrAdd
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="func"></param>
        /// <param name="slidingMillisecond"></param>
        /// <param name="absoluteTime"></param>
        T GetOrAdd<T>(string key, Func<string, T> func, int? slidingMillisecond = null, DateTime? absoluteTime = null);

    }
}