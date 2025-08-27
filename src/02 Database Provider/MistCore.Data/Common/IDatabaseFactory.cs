using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;

namespace MistCore.Data.Common
{
    /// <summary>
    /// Data Provider Factory
    /// 数据源提供工厂
    /// 配置数据库连接方式，提供数据库连接IDatabaseSession
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface IDatabaseFactory : IDisposable
    {
        /// <summary>
        /// Creates the database session.
        /// </summary>
        /// <returns></returns>
        IDatabaseSession CreateDbSession();

        /// <summary>
        /// Creates the connection.
        /// </summary>
        /// <returns></returns>
        IDbConnection CreateConnection();

        /// <summary>
        /// Creates the command.
        /// </summary>
        /// <returns></returns>
        IDbCommand CreateCommand();

        /// <summary>
        /// Creates the parameter.
        /// </summary>
        /// <returns></returns>
        IDbDataParameter CreateParameter();

    }
}
