using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MistCore.Data
{
    public interface IRepository
    {
    }

    /// <summary>
    /// IRepository
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public interface IRepository<TEntity> : IRepository where TEntity : class, IEntity
    {
        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <param name="Id">The identifier.</param>
        /// <returns></returns>
        TEntity Get(object Id);

        /// <summary>
        /// Gets all list.
        /// </summary>
        /// <returns></returns>
        Task<List<TEntity>> GetListAsync();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageInfo"></param>
        /// <returns></returns>
        IQueryable<TEntity> GetQueryable(PageInfo pageInfo = null);

        /// <summary>
        /// Gets the queryable.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        IQueryable<TEntity> GetQueryableFromSqlRaw(string sql, params object[] parameters);

        /// <summary>
        /// Gets the queryable.
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        IQueryable<TEntity> GetQueryableFromSqlInterpolated(FormattableString sql);

        /// <summary>
        /// Execute Sql.
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        int ExecuteSql(FormattableString sql);

        /// <summary>
        /// Execute Sql Interpolated.
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        int ExecuteSqlInterpolated(FormattableString sql);

        /// <summary>
        /// Execute Sql Raw.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        int ExecuteSqlRaw(string sql, params object[] parameters);

        /// <summary>
        /// Gets the list.
        /// </summary>
        /// <param name="pageInfo">The page information.</param>
        /// <returns></returns>
        List<TEntity> GetList(PageInfo pageInfo);

        /// <summary>
        /// Gets all list.
        /// </summary>
        /// <returns></returns>
        List<TEntity> GetAllList();

        /// <summary>
        /// Saves the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        TEntity Save(TEntity entity);

        /// <summary>
        /// Saves the many.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <returns></returns>
        int SaveMany(ICollection<TEntity> entities);

        /// <summary>
        /// Saves the or update many.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <returns></returns>
        int SaveOrUpdateMany(ICollection<TEntity> entities);

        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        TEntity Update(TEntity entity);

        /// <summary>
        /// Updates the many.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <returns></returns>
        int UpdateMany(ICollection<TEntity> entities);

        /// <summary>
        /// Deletes the specified identifier.
        /// </summary>
        /// <param name="Id">The identifier.</param>
        void DeleteById(object Id);

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        void Delete(TEntity entity);

        /// <summary>
        /// Deletes the many.
        /// </summary>
        /// <param name="entities">The entities.</param>
        void DeleteMany(ICollection<TEntity> entities);

        /// <summary>
        /// Deletes the many by ids.
        /// </summary>
        /// <param name="Ids">The ids.</param>
        void DeleteManyByIds(ICollection<object> Ids);

        /// <summary>
        /// Detacheds the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        void Detached(TEntity entity);

        /// <summary>
        /// Detacheds the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        void DetachedMany(ICollection<TEntity> entities);

    }
}
