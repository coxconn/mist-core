using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;
using System.Collections;

namespace MistCore.Data.Common
{
    /// <summary>
    /// Ado.NET database session command realize
    /// </summary>
    /// <seealso cref="Mist.Data.Common.IDatabaseSession" />
    internal class DatabaseCommand : IDatabaseSession
    {
        /// <summary>
        /// The command
        /// </summary>
        private readonly IDbCommand Command;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseCommand" /> class.
        /// </summary>
        /// <param name="cmd">The command.</param>
        public DatabaseCommand(IDbCommand cmd)
        {
            Command = cmd;
        }

        /// <summary>
        /// Prepares the command.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        private void PrepareCommand(string sql, object parameters) {
            Command.CommandType = CommandType.Text;
            Command.CommandText = sql;
            Command.Parameters.Clear();
            Command.SetParameters(parameters);
        }

        /// <summary>
        /// Executes the non query.
        /// </summary>
        /// <param name="sSQL">The s SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public int ExecuteNonQuery(string sSQL, object parameters)
        {
            PrepareCommand(sSQL, parameters);
            return Command.ExecuteNonQuery();
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
            PrepareCommand(sSQL, parameters);
            using (var dr = Command.ExecuteReader())
            {
                while (dr.Read())
                    yield return action.Invoke(dr);
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
            PrepareCommand(sSQL, parameters);
            using (var dr = Command.ExecuteReader())
            {
                while (dr.Read())
                    action.Invoke(dr);
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
            PrepareCommand(sSQL, parameters);
            return (T)Command.ExecuteScalar();
        }
    }

    internal static class DatabaseCommandExtensions
    {
        #region Set Parameters
        /// <summary>
        /// Sets the parameters.
        /// </summary>
        /// <param name="cmd">The command.</param>
        /// <param name="parameters">The parameters.</param>
        public static void SetParameters(this IDbCommand cmd, object parameters)
        {
            if (parameters == null)
                return;

            if (parameters is IList)
            {
                var listed = (IList)parameters;
                for (var i = 0; i < listed.Count; i++)
                {
                    AddParameter(cmd, string.Format("@{0}", i), listed[i]);
                }
            }
            else
            {
                var t = parameters.GetType();
                var parameterInfos = t.GetProperties();
                foreach (var pi in parameterInfos)
                {
                    AddParameter(cmd, pi.Name, pi.GetValue(parameters, null));
                }
            }
        }

        /// <summary>
        /// Adds the parameter.
        /// </summary>
        /// <param name="cmd">The command.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        private static void AddParameter(IDbCommand cmd, string name, object value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }
        #endregion


    }
}
