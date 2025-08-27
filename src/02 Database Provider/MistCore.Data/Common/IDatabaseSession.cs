using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;

namespace MistCore.Data.Common
{
    /// <summary>
    /// Database Session Provider
    /// 数据库连接提供者
    /// 用于连接数据库与一些基本数据库操作
    /// </summary>
    public interface IDatabaseSession
    {

        //void ExecuteProcedure(string procName, object parameters = null);

        /// <summary>
        /// Executes the non query.
        /// </summary>
        /// <param name="sSQL">The s SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        int ExecuteNonQuery(string sSQL, object parameters);

        /// <summary>
        /// Executes the data reader.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sSQL">The s SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        IEnumerable<T> ExecuteDataReader<T>(string sSQL, object parameters, Func<IDataReader, T> action);

        /// <summary>
        /// Executes the data reader.
        /// </summary>
        /// <param name="sSQL">The s SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="action">The action.</param>
        void ExecuteDataReader(string sSQL, object parameters, Action<IDataReader> action);

        /// <summary>
        /// Executes the scalar.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sSQL">The s SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        T ExecuteScalar<T>(string sSQL, object parameters);

    }
}
