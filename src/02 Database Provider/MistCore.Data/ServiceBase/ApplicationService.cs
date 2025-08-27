using Microsoft.Extensions.DependencyInjection;
using MistCore.Core.Modules;
using System;
using System.Collections.Generic;
using System.Text;

namespace MistCore.Data
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <seealso cref="PR.WizInspection.Data.EFProvider.DatabaseFactory" />
    /// <seealso cref="PR.WizInspection.Data.IRepository{TEntity}" />
    public abstract class ApplicationServiceBase<TEntity> : IApplicationService, IApplicationService<TEntity> where TEntity : class, IEntity
    {

        public ApplicationServiceBase()
        {
            DomainService = GlobalConfiguration.ServiceLocator.Instance.GetRequiredService<IDomainService<TEntity>>();

            //this.DomainService = IocManager.Instance.Resolve<IDomainService<TEntity, TEntityDTO>>();
        }

        protected IDomainService<TEntity> DomainService { get; set; }

        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <param name="Id">The identifier.</param>
        /// <returns></returns>
        public virtual TEntity Get(object Id)
        {
            return DomainService.Get(Id);
        }

        /// <summary>
        /// Gets the list.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        public virtual List<TEntity> GetList(ref PageInfo page)
        {
            return DomainService.GetList(ref page);
        }

        /// <summary>
        /// Gets the list no pager.
        /// </summary>
        /// <returns></returns>
        public virtual List<TEntity> GetAllList()
        {
            return DomainService.GetAllList();
        }

        /// <summary>
        /// Saves the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public virtual TEntity Save(TEntity entity)
        {
            return DomainService.Save(entity);
        }

        ///// <summary>
        ///// Saves the specified entity.
        ///// </summary>
        ///// <param name="entity">The entity.</param>
        ///// <returns></returns>
        //public virtual TEntity SaveOrUpdate(TEntity entity)
        //{
        //    TEntity result = null;
        //    if ((entity.Id == Guid.Empty))
        //    {
        //        result = EntityDao.Save(entity);
        //    }
        //    else
        //    {
        //        result = EntityDao.Update(entity);
        //    }

        //    return result;
        //}

        public virtual int SaveOrUpdateMany(ICollection<TEntity> entities)
        {
            return DomainService.SaveOrUpdateMany(entities);
        }

        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public virtual TEntity Update(TEntity entity)
        {
            return DomainService.Update(entity);
        }

        public virtual int SaveMany(ICollection<TEntity> entities)
        {
            return DomainService.SaveMany(entities);
        }

        /// <summary>
        /// Batches the update.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <returns></returns>
        public virtual int UpdateMany(ICollection<TEntity> entities)
        {
            return DomainService.UpdateMany(entities);
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public virtual void Delete(TEntity entity)
        {
            DomainService.Delete(entity);
        }

        /// <summary>
        /// Deletes the specified identifier.
        /// </summary>
        /// <param name="Id">The identifier.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public virtual void DeleteById(object Id)
        {
            DomainService.DeleteById(Id);
        }

        /// <summary>
        /// Deletes the many.
        /// </summary>
        /// <param name="entities">The entities.</param>
        public virtual void DeleteMany(ICollection<TEntity> entities)
        {
            DomainService.DeleteMany(entities);
        }

        /// <summary>
        /// Deletes the many by ids.
        /// </summary>
        /// <param name="Ids">The ids.</param>
        public virtual void DeleteManyByIds(ICollection<object> Ids)
        {
            DomainService.DeleteManyByIds(Ids);
        }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <seealso cref="PR.WizInspection.Data.EFProvider.DatabaseFactory" />
    /// <seealso cref="PR.WizInspection.Data.IRepository{TEntity}" />
    public abstract class ApplicationServiceBase<TEntity, TEntityDTO> : IApplicationService, IApplicationService<TEntity, TEntityDTO> where TEntity : class, IEntity where TEntityDTO : class, IEntity
    {

        public ApplicationServiceBase()
        {
            DomainService = GlobalConfiguration.ServiceLocator.Instance.GetRequiredService<IDomainService<TEntity, TEntityDTO>>();
            //this.DomainService = IocManager.Instance.Resolve<IDomainService<TEntity, TEntityDTO>>();
        }

        protected IDomainService<TEntity, TEntityDTO> DomainService { get; set; }

        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <param name="Id">The identifier.</param>
        /// <returns></returns>
        public virtual TEntityDTO Get(object Id)
        {
            return DomainService.Get(Id);
        }

        /// <summary>
        /// Gets the list.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        public virtual List<TEntityDTO> GetList(PageInfo page)
        {
            return DomainService.GetList(page);
        }

        /// <summary>
        /// Gets the list no pager.
        /// </summary>
        /// <returns></returns>
        public virtual List<TEntityDTO> GetAllList()
        {
            return DomainService.GetAllList();
        }

        /// <summary>
        /// Saves the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public virtual TEntityDTO Save(TEntityDTO entity)
        {
            return DomainService.Save(entity);
        }

        ///// <summary>
        ///// Saves the specified entity.
        ///// </summary>
        ///// <param name="entity">The entity.</param>
        ///// <returns></returns>
        //public virtual TEntity SaveOrUpdate(TEntity entity)
        //{
        //    TEntity result = null;
        //    if ((entity.Id == Guid.Empty))
        //    {
        //        result = EntityDao.Save(entity);
        //    }
        //    else
        //    {
        //        result = EntityDao.Update(entity);
        //    }

        //    return result;
        //}

        public virtual int SaveOrUpdateMany(ICollection<TEntityDTO> entities)
        {
            return DomainService.SaveOrUpdateMany(entities);
        }

        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public virtual TEntityDTO Update(TEntityDTO entity)
        {
            return DomainService.Update(entity);
        }

        public virtual int SaveMany(ICollection<TEntityDTO> entities)
        {
            return DomainService.SaveMany(entities);
        }

        /// <summary>
        /// Batches the update.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <returns></returns>
        public virtual int UpdateMany(ICollection<TEntityDTO> entities)
        {
            return DomainService.UpdateMany(entities);
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public virtual void Delete(TEntityDTO entity)
        {
            DomainService.Delete(entity);
        }

        /// <summary>
        /// Deletes the specified identifier.
        /// </summary>
        /// <param name="Id">The identifier.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public virtual void DeleteById(object Id)
        {
            DomainService.DeleteById(Id);
        }

        /// <summary>
        /// Deletes the many.
        /// </summary>
        /// <param name="entities">The entities.</param>
        public virtual void DeleteMany(ICollection<TEntityDTO> entities)
        {
            DomainService.DeleteMany(entities);
        }

        /// <summary>
        /// Deletes the many by ids.
        /// </summary>
        /// <param name="Ids">The ids.</param>
        public virtual void DeleteManyByIds(ICollection<object> Ids)
        {
            DomainService.DeleteManyByIds(Ids);
        }

    }


    /// <summary>
    /// 服务
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    internal class ApplicationService<TEntity> : ApplicationServiceBase<TEntity>
        where TEntity : class, IEntity
    {
    }

    /// <summary>
    /// 服务
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TEntityDTO"></typeparam>
    internal class ApplicationService<TEntity, TEntityDTO> : ApplicationServiceBase<TEntity, TEntityDTO>
        where TEntity : class, IEntity where TEntityDTO : class, IEntity
    {
    }

}
