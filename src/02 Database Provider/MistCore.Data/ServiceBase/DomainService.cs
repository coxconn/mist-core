using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using MistCore.Core.Modules;
using MistCore.Core.DTOMapper;

namespace MistCore.Data
{

    /// <summary>
    /// DomainServiceBase
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <seealso cref="PR.WizInspection.Data.EFProvider.DatabaseFactory" />
    /// <seealso cref="PR.WizInspection.Data.IRepository{TEntity}" />
    public abstract class DomainServiceBase<TEntity> : IDomainService<TEntity> where TEntity : class, IEntity
    {

        public DomainServiceBase()
        {
            EntityDao = GlobalConfiguration.ServiceLocator.Instance.GetRequiredService<IRepository<TEntity>>();
            //this.EntityDao = IocManager.Instance.Resolve<IRepository<TEntity>>();
        }

        protected IRepository<TEntity> EntityDao { get; set; }

        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <param name="Id">The identifier.</param>
        /// <returns></returns>
        public virtual TEntity Get(object Id)
        {
            return EntityDao.Get(Id);
        }

        /// <summary>
        /// Gets the list.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        public virtual List<TEntity> GetList(ref PageInfo page)
        {
            return EntityDao.GetList(page);
        }

        /// <summary>
        /// Gets the list no pager.
        /// </summary>
        /// <returns></returns>
        public virtual List<TEntity> GetAllList()
        {
            return EntityDao.GetAllList();
        }

        /// <summary>
        /// Saves the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public virtual TEntity Save(TEntity entity)
        {
            return EntityDao.Save(entity);
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
            return EntityDao.SaveOrUpdateMany(entities);
        }

        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public virtual TEntity Update(TEntity entity)
        {
            return EntityDao.Update(entity);
        }

        public virtual int SaveMany(ICollection<TEntity> entities)
        {
            return EntityDao.SaveMany(entities);
        }

        /// <summary>
        /// Batches the update.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <returns></returns>
        public virtual int UpdateMany(ICollection<TEntity> entities)
        {
            return EntityDao.UpdateMany(entities);
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public virtual void Delete(TEntity entity)
        {
            EntityDao.Delete(entity);
        }

        /// <summary>
        /// Deletes the specified identifier.
        /// </summary>
        /// <param name="Id">The identifier.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public virtual void DeleteById(object Id)
        {
            EntityDao.DeleteById(Id);
        }

        /// <summary>
        /// Deletes the many.
        /// </summary>
        /// <param name="entities">The entities.</param>
        public virtual void DeleteMany(ICollection<TEntity> entities)
        {
            EntityDao.DeleteMany(entities);
        }

        /// <summary>
        /// Deletes the many by ids.
        /// </summary>
        /// <param name="Ids">The ids.</param>
        public virtual void DeleteManyByIds(ICollection<object> Ids)
        {
            EntityDao.DeleteManyByIds(Ids);
        }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <seealso cref="PR.WizInspection.Data.EFProvider.DatabaseFactory" />
    /// <seealso cref="PR.WizInspection.Data.IRepository{TEntity}" />
    public abstract class DomainServiceBase<TEntity, TEntityDTO> : IDomainService<TEntity, TEntityDTO> where TEntity : class, IEntity where TEntityDTO : class, IEntity
    {

        public DomainServiceBase()
        {
            EntityDao = GlobalConfiguration.ServiceLocator.Instance.GetRequiredService<IRepository<TEntity>>();
            this.mapper = GlobalConfiguration.ServiceLocator.Instance.GetRequiredService<IDTOMapper>();

            //Mapper.Initialize(c=>c.CreateMap<TEntity, TEntityDTO>());
            //Mapper.Initialize(c=>c.CreateMap<TEntityDTO, TEntity>());
        }

        protected IRepository<TEntity> EntityDao { get; set; }
        public IDTOMapper mapper;

        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <param name="Id">The identifier.</param>
        /// <returns></returns>
        public virtual TEntityDTO Get(object Id)
        {
            return mapper.Map<TEntityDTO>(EntityDao.Get(Id));
        }

        /// <summary>
        /// Gets the list.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        public virtual List<TEntityDTO> GetList(PageInfo page)
        {
            return mapper.Map<List<TEntityDTO>>(EntityDao.GetList(page));
        }

        /// <summary>
        /// Gets the list no pager.
        /// </summary>
        /// <returns></returns>
        public virtual List<TEntityDTO> GetAllList()
        {
            return mapper.Map<List<TEntityDTO>>(EntityDao.GetAllList());
        }

        /// <summary>
        /// Saves the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public virtual TEntityDTO Save(TEntityDTO entity)
        {
            return mapper.Map<TEntityDTO>(EntityDao.Save(mapper.Map<TEntity>(entity)));
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
            return EntityDao.SaveOrUpdateMany(mapper.Map<ICollection<TEntity>>(entities));
        }

        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public virtual TEntityDTO Update(TEntityDTO entity)
        {
            return mapper.Map<TEntityDTO>(EntityDao.Update(mapper.Map<TEntity>(entity)));
        }

        public virtual int SaveMany(ICollection<TEntityDTO> entities)
        {
            return EntityDao.SaveMany(mapper.Map<ICollection<TEntity>>(entities));
        }

        /// <summary>
        /// Batches the update.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <returns></returns>
        public virtual int UpdateMany(ICollection<TEntityDTO> entities)
        {
            return EntityDao.UpdateMany(mapper.Map<ICollection<TEntity>>(entities));
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public virtual void Delete(TEntityDTO entity)
        {
            EntityDao.Delete(mapper.Map<TEntity>(entity));
        }

        /// <summary>
        /// Deletes the specified identifier.
        /// </summary>
        /// <param name="Id">The identifier.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public virtual void DeleteById(object Id)
        {
            EntityDao.DeleteById(Id);
        }

        /// <summary>
        /// Deletes the many.
        /// </summary>
        /// <param name="entities">The entities.</param>
        public virtual void DeleteMany(ICollection<TEntityDTO> entities)
        {
            EntityDao.DeleteMany(mapper.Map<ICollection<TEntity>>(entities));
        }

        /// <summary>
        /// Deletes the many by ids.
        /// </summary>
        /// <param name="Ids">The ids.</param>
        public virtual void DeleteManyByIds(ICollection<object> Ids)
        {
            EntityDao.DeleteManyByIds(Ids);
        }

    }




    /// <summary>
    /// DomainService
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    internal class DomainService<TEntity> : DomainServiceBase<TEntity> 
        where TEntity : class, IEntity
    {

    }

    /// <summary>
    /// DomainService
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TEntityDTO"></typeparam>
    internal class DomainService<TEntity, TEntityDTO> : DomainServiceBase<TEntity, TEntityDTO>
        where TEntity : class, IEntity where TEntityDTO : class, IEntity
    {

    }



}
