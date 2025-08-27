using System;
using System.Collections.Generic;
using System.Text;

namespace MistCore.Data
{
    public interface IApplicationService
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public interface IApplicationService<TEntity> : IApplicationService where TEntity : class, IEntity
    {

        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <param name="Id">The identifier.</param>
        /// <returns></returns>
        TEntity Get(object Id);

        /// <summary>
        /// Gets the list.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        List<TEntity> GetList(ref PageInfo page);

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

        //TEntity SaveOrUpdate(TEntity entity);

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
        /// Saves the many.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <returns></returns>
        int SaveMany(ICollection<TEntity> entities);

        /// <summary>
        /// Updates the many.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <returns></returns>
        int UpdateMany(ICollection<TEntity> entities);

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        void Delete(TEntity entity);

        /// <summary>
        /// Deletes the by identifier.
        /// </summary>
        /// <param name="Id">The identifier.</param>
        void DeleteById(object Id);

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
    }


    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public interface IApplicationService<TEntity, TEntityDTO> : IApplicationService where TEntity : class, IEntity where TEntityDTO : class, IEntity
    {

        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <param name="Id">The identifier.</param>
        /// <returns></returns>
        TEntityDTO Get(object Id);

        /// <summary>
        /// Gets the list.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        List<TEntityDTO> GetList(PageInfo page);

        /// <summary>
        /// Gets all list.
        /// </summary>
        /// <returns></returns>
        List<TEntityDTO> GetAllList();

        /// <summary>
        /// Saves the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        TEntityDTO Save(TEntityDTO entity);

        //TEntity SaveOrUpdate(TEntity entity);

        /// <summary>
        /// Saves the or update many.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <returns></returns>
        int SaveOrUpdateMany(ICollection<TEntityDTO> entities);

        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        TEntityDTO Update(TEntityDTO entity);

        /// <summary>
        /// Saves the many.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <returns></returns>
        int SaveMany(ICollection<TEntityDTO> entities);

        /// <summary>
        /// Updates the many.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <returns></returns>
        int UpdateMany(ICollection<TEntityDTO> entities);

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        void Delete(TEntityDTO entity);

        /// <summary>
        /// Deletes the by identifier.
        /// </summary>
        /// <param name="Id">The identifier.</param>
        void DeleteById(object Id);

        /// <summary>
        /// Deletes the many.
        /// </summary>
        /// <param name="entities">The entities.</param>
        void DeleteMany(ICollection<TEntityDTO> entities);

        /// <summary>
        /// Deletes the many by ids.
        /// </summary>
        /// <param name="Ids">The ids.</param>
        void DeleteManyByIds(ICollection<object> Ids);
    }

}
