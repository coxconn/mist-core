using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;

namespace MistCore.Data.Common
{
    /// <summary>
    /// Ado.NET Database session realize
    /// </summary>
    /// <seealso cref="Mist.Data.Common.IDatabaseSession" />
    public class DatabaseSession : IDatabaseSession
    {
        /// <summary>
        /// The connection
        /// </summary>
        private readonly IDbConnection connection;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseSession" /> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public DatabaseSession(IDbConnection connection)
        {
            this.connection = connection;
        }

        /// <summary>
        /// Creates the connection.
        /// </summary>
        /// <returns></returns>
        private IDbConnection GetConnection()
        {
            return connection;
        }

        /// <summary>
        /// Executes the non query.
        /// </summary>
        /// <param name="sSQL">The s SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public int ExecuteNonQuery(string sSQL, object parameters)
        {
            var connection = GetConnection();
            using (var cmd = connection.CreateCommand())
            {
                return new DatabaseCommand(cmd).ExecuteNonQuery(sSQL, parameters);
            }
        }

        /// <summary>
        /// Executes the data reader.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sSQL">The s SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public IEnumerable<T> ExecuteDataReader<T>(string sSQL, object parameters, Func<IDataReader, T> action)
        {
            var connection = GetConnection();
            using (var cmd = connection.CreateCommand())
            {
                var db = new DatabaseCommand(cmd);
                //延迟执行，可供调用者解析 DataReader
                foreach (var r in db.ExecuteDataReader(sSQL, parameters, action))
                    yield return r;
            }
        }

        /// <summary>
        /// Executes the data reader.
        /// </summary>
        /// <param name="sSQL">The s SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="action">The action.</param>
        public void ExecuteDataReader(string sSQL, object parameters, Action<IDataReader> action)
        {
            var connection = GetConnection();
            using (var cmd = connection.CreateCommand())
            {
                var db = new DatabaseCommand(cmd);
                db.ExecuteDataReader(sSQL, parameters, action);
            }
        }

        /// <summary>
        /// Executes the scalar.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sSQL">The s SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public T ExecuteScalar<T>(string sSQL, object parameters)
        {
            var connection = GetConnection();
            using (var cmd = connection.CreateCommand())
            {
                var db = new DatabaseCommand(cmd);
                return db.ExecuteScalar<T>(sSQL, parameters);
            }
        }

    }
}
