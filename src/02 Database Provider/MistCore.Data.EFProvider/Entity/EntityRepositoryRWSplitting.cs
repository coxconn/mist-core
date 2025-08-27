using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MistCore.Core.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MistCore.Data.EFProvider.Entity
{

    /// <summary>
    /// RepositoryBase
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <seealso cref="PR.WizInspection.Data.EFProvider.DatabaseFactory" />
    /// <seealso cref="IRepository{TEntity}" />
    public abstract class EntityRepositoryRWSplittingBase<ReadDbContext, WriteDbContext, TEntity> : IRepository, IRepository<TEntity>
        where ReadDbContext : DefaultDbContext
        where WriteDbContext : DefaultDbContext
        where TEntity : class, IEntity
    {

        private readonly ReadDbContext _rdbContext;
        private readonly WriteDbContext _wdbContext;
        public ReadDbContext GetReadDbContext()
        {
            return _rdbContext;
        }

        public WriteDbContext GetWriteDbContext()
        {
            return _wdbContext;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public EntityRepositoryRWSplittingBase()
        {
            _rdbContext = GlobalConfiguration.ServiceLocator.Instance.GetRequiredService<ReadDbContext>();
            _wdbContext = GlobalConfiguration.ServiceLocator.Instance.GetRequiredService<WriteDbContext>();
        }

        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <param name="Id">The identifier.</param>
        /// <returns></returns>
        public virtual TEntity Get(object Id)
        {
            return GetReadDbContext().Set<TEntity>().Find(Id);
        }

        /// <summary>
        /// Gets the list.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        public virtual List<TEntity> GetList(PageInfo pageInfo)
        {
            return GetReadDbContext().Set<TEntity>().OrderBy(c => c).Pagination(pageInfo).ToList();
        }

        /// <summary>
        /// Gets the queryable.
        /// </summary>
        /// <param name="pageInfo">The page information.</param>
        /// <returns></returns>
        public virtual IQueryable<TEntity> GetQueryable(PageInfo pageInfo = null)
        {
            if (pageInfo == null)
            {
                return GetReadDbContext().Set<TEntity>().AsQueryable();
            }
            else
            {
                return GetReadDbContext().Set<TEntity>().OrderBy(c => c).Pagination(pageInfo);
            }
        }


        /// <summary>
        /// Gets the queryable.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public virtual IQueryable<TEntity> GetQueryableFromSqlRaw(string sql, params object[] parameters)
        {
            return GetReadDbContext().Set<TEntity>().FromSqlRaw(sql, parameters);
        }

        /// <summary>
        /// Gets the queryable.
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public virtual IQueryable<TEntity> GetQueryableFromSqlInterpolated(FormattableString sql)
        {
            return GetReadDbContext().Set<TEntity>().FromSqlInterpolated(sql);
        }

        /// <summary>
        /// Execute Sql.
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public virtual int ExecuteSql(FormattableString sql)
        {
#if NET8_0
            return GetWriteDbContext().Database.ExecuteSql(sql);
#else
            throw new NotImplementedException();
#endif
        }

        /// <summary>
        /// Execute Sql Interpolated.
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public virtual int ExecuteSqlInterpolated(FormattableString sql)
        {
            return GetWriteDbContext().Database.ExecuteSqlInterpolated(sql);
        }

        /// <summary>
        /// Execute Sql Raw.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public virtual int ExecuteSqlRaw(string sql, params object[] parameters)
        {
            return GetWriteDbContext().Database.ExecuteSqlRaw(sql, parameters);
        }

        /// <summary>
        /// Gets all list.
        /// </summary>
        /// <returns></returns>
        public virtual async Task<List<TEntity>> GetListAsync()
        {
            var ctx = GetReadDbContext();
            var list = await ctx.Set<TEntity>().ToListAsync();
            return list;
        }

        /// <summary>
        /// Gets all list.
        /// </summary>
        /// <returns></returns>
        public virtual List<TEntity> GetAllList()
        {
            var ctx = GetReadDbContext();
            var list = ctx.Set<TEntity>().ToList();
            //DetachedMany(list);
            return list;
        }

        /// <summary>
        /// Saves the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public virtual TEntity Save(TEntity entity)
        {
            var ctx = GetWriteDbContext();
            var model = ctx.Set<TEntity>().Add(entity);
            ctx.SaveChanges();
            return model.Entity;
        }

        /// <summary>
        /// Saves the many.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <returns></returns>
        public int SaveMany(ICollection<TEntity> entities)
        {
            var ctx = GetWriteDbContext();
            ctx.Set<TEntity>().AddRange(entities);
            return ctx.SaveChanges();
        }

        /// <summary>
        /// Saves the or update many.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <returns></returns>
        public int SaveOrUpdateMany(ICollection<TEntity> entities)
        {
            var ctx = GetWriteDbContext();
            var dbSet = ctx.Set<TEntity>();
            foreach (var entity in entities)
            {
                ctx.Entry(entity).State = EntityState.Modified;
            }
            return ctx.SaveChanges();
        }

        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public virtual TEntity Update(TEntity entity)
        {
            var ctx = GetWriteDbContext();
            ctx.Entry(entity).State = EntityState.Modified;
            ctx.SaveChanges();
            return entity;
        }

        /// <summary>
        /// Batches the update.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <returns></returns>
        public virtual int UpdateMany(ICollection<TEntity> entities)
        {
            var ctx = GetWriteDbContext();
            var dbSet = ctx.Set<TEntity>();
            foreach (var entity in entities)
            {
                ctx.Entry(entity).State = EntityState.Modified;
            }
            return ctx.SaveChanges();
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public virtual void Delete(TEntity entity)
        {
            var ctx = GetWriteDbContext();
            ctx.Set<TEntity>().Remove(entity);
            ctx.SaveChanges();
        }

        /// <summary>
        /// Deletes the specified entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        public virtual void DeleteMany(ICollection<TEntity> entities)
        {
            var ctx = GetWriteDbContext();
            ctx.Set<TEntity>().RemoveRange(entities);
            ctx.SaveChanges();
        }

        /// <summary>
        /// Deletes the specified identifier.
        /// </summary>
        /// <param name="Id">The identifier.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public virtual void DeleteById(object Id)
        {
            var ctx = GetWriteDbContext();
            var dbSet = ctx.Set<TEntity>();
            var entity = dbSet.Find(Id);
            dbSet.Remove(entity);
            ctx.SaveChanges();
        }

        /// <summary>
        /// Betches the delete.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public virtual void DeleteManyByIds(ICollection<object> Ids)
        {
            foreach (var Id in Ids)
            {
                DeleteById(Id);
            }
        }

        /// <summary>
        /// Detacheds the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public virtual void Detached(TEntity entity)
        {
            GetWriteDbContext().Entry(entity).State = EntityState.Detached;
        }
        /// <summary>
        /// Detacheds the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public virtual void DetachedMany(ICollection<TEntity> entities)
        {
            var ctx = GetWriteDbContext();
            foreach (var entity in entities)
            {
                ctx.Entry(entity).State = EntityState.Detached;
            }
        }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="ReadDbContext"></typeparam>
    /// <typeparam name="WriteDbContext"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    internal class EntityRepositoryRWSplitting<ReadDbContext, WriteDbContext, TEntity> : EntityRepositoryRWSplittingBase<ReadDbContext, WriteDbContext, TEntity>, IRepository<ReadDbContext, WriteDbContext, TEntity>, IRepository, IRepository<TEntity>
        where ReadDbContext : DefaultDbContext
        where WriteDbContext : DefaultDbContext
        where TEntity : class, IEntity

    {

    }

}
