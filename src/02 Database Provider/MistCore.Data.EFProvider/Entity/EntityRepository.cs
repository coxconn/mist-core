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
    /// <typeparam name="TDbContext"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    public abstract class EntityRepositoryBase<TDbContext, TEntity> : IRepository, IRepository<TEntity>
        where TDbContext : DefaultDbContext
        where TEntity : class, IEntity
    {

        /// <summary>
        /// Gets EF DbContext object.
        /// </summary>
        //public virtual TDbContext Context => _dbContextProvider.GetDbContext();

        ///// <summary>
        ///// Gets DbSet for given entity.
        ///// </summary>
        //public virtual DbSet<TEntity> Table => Context.Set<TEntity>();

        private readonly TDbContext _dbContext;

        public TDbContext GetDbContext()
        {
            return _dbContext;
        }

        public override string ToString()
        {
            return $"{System.Threading.Thread.CurrentThread.ManagedThreadId} {_dbContext.ContextId.GetHashCode()} {_dbContext.ContextId.InstanceId} {_dbContext.ContextId.Lease}";
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public EntityRepositoryBase()
        {
            _dbContext = GlobalConfiguration.ServiceLocator.Instance.GetRequiredService<TDbContext>();
        }

        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <param name="Id">The identifier.</param>
        /// <returns></returns>
        public virtual TEntity Get(object Id)
        {
            return GetDbContext().Set<TEntity>().Find(Id);
        }

        /// <summary>
        /// Gets the list.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        public virtual List<TEntity> GetList(PageInfo pageInfo)
        {
            return GetDbContext().Set<TEntity>().OrderBy(c => c).Pagination(pageInfo).ToList();
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
                return GetDbContext().Set<TEntity>().AsQueryable();
            }
            else
            {
                return GetDbContext().Set<TEntity>().OrderBy(c => c).Pagination(pageInfo);
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
            return GetDbContext().Set<TEntity>().FromSqlRaw(sql, parameters);
        }

        /// <summary>
        /// Gets the queryable.
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public virtual IQueryable<TEntity> GetQueryableFromSqlInterpolated(FormattableString sql)
        {
            return GetDbContext().Set<TEntity>().FromSqlInterpolated(sql);
        }

        /// <summary>
        /// Execute Sql.
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public virtual int ExecuteSql(FormattableString sql)
        {
#if NET8_0
            return GetDbContext().Database.ExecuteSql(sql);
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
            return GetDbContext().Database.ExecuteSqlInterpolated(sql);
        }

        /// <summary>
        /// Execute Sql Raw.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public virtual int ExecuteSqlRaw(string sql, params object[] parameters)
        {
            return GetDbContext().Database.ExecuteSqlRaw(sql, parameters);
        }

        /// <summary>
        /// Gets all list.
        /// </summary>
        /// <returns></returns>
        public virtual async Task<List<TEntity>> GetListAsync()
        {
            var ctx = GetDbContext();
            var list = await ctx.Set<TEntity>().ToListAsync();
            return list;
        }

        /// <summary>
        /// Gets all list.
        /// </summary>
        /// <returns></returns>
        public virtual List<TEntity> GetAllList()
        {
            var ctx = GetDbContext();
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
            var ctx = GetDbContext();
            var model = ctx.Set<TEntity>().Add(entity);
            ctx.SaveChanges();
            ctx.Entry(entity).State = EntityState.Detached;
            return model.Entity;
        }

        /// <summary>
        /// Saves the many.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <returns></returns>
        public int SaveMany(ICollection<TEntity> entities)
        {
            var ctx = GetDbContext();
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
            var ctx = GetDbContext();
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
            var ctx = GetDbContext();
            ctx.Set<TEntity>().Update(entity);
            //ctx.Entry(entity).State = EntityState.Modified;
            ctx.SaveChanges();
            ctx.Entry(entity).State = EntityState.Detached;
            return entity;
        }

        /// <summary>
        /// Batches the update.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <returns></returns>
        public virtual int UpdateMany(ICollection<TEntity> entities)
        {
            var ctx = GetDbContext();
            var dbSet = ctx.Set<TEntity>();
            dbSet.UpdateRange(entities);

            //foreach (var entity in entities)
            //{
            //    ctx.Entry(entity).State = EntityState.Modified;
            //}
            return ctx.SaveChanges();
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public virtual void Delete(TEntity entity)
        {
            var ctx = GetDbContext();
            ctx.Set<TEntity>().Remove(entity);
            ctx.SaveChanges();
        }

        /// <summary>
        /// Deletes the specified entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        public virtual void DeleteMany(ICollection<TEntity> entities)
        {
            var ctx = GetDbContext();
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
            var ctx = GetDbContext();
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
            GetDbContext().Entry(entity).State = EntityState.Detached;
        }
        /// <summary>
        /// Detacheds the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public virtual void DetachedMany(ICollection<TEntity> entities)
        {
            var ctx = GetDbContext();
            foreach (var entity in entities)
            {
                ctx.Entry(entity).State = EntityState.Detached;
            }
        }

    }

    /// <summary>
    /// EntityRepository
    /// </summary>
    /// <typeparam name="TDbContext"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    public class EntityRepository<TDbContext, TEntity> : EntityRepositoryBase<TDbContext, TEntity>, IRepository<TDbContext, TEntity>, IRepository, IRepository<TEntity>
        where TDbContext : DefaultDbContext
        where TEntity : class, IEntity

    {

    }

}
