using System;
using System.Collections.Generic;
using System.Linq;

namespace MistCore.Framework.EntityOperate
{

    /// <summary>
    /// 对象比较器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EntitySelector<T> : IEqualityComparer<T>
    {
        private Func<T, T, bool> pred;

        /// <summary>
        /// 对象比较器
        /// </summary>
        /// <param name="pred">对象比较规则</param>
        public EntitySelector(Func<T, T, bool> pred)
        {
            this.pred = pred;
        }

        /// <summary>
        /// 使用构造器的对象比较规则比较两个对象
        /// </summary>
        /// <param name="x">T</param>
        /// <param name="y">T</param>
        /// <returns>是否符合比较规则</returns>
        public bool Equals(T x, T y)
        {
            return pred(x, y);
        }

        /// <summary>
        /// 对象哈希码
        /// </summary>
        /// <param name="obj">T</param>
        /// <returns>T 类型的哈希码</returns>
        public int GetHashCode(T obj)
        {
            return obj.GetType().GetHashCode();
        }
    }

}