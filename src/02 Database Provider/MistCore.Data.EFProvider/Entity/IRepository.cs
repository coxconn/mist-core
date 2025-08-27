using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MistCore.Data.EFProvider.Entity
{

    /// <summary>
    /// IRepository
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public interface IRepository<TDbContext, TEntity> : IRepository<TEntity>, IRepository where TEntity : class, IEntity
    {

    }

    /// <summary>
    /// IRepository
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public interface IRepository<ReadDbContext, WriteDbContext, TEntity> : IRepository<TEntity>, IRepository where TEntity : class, IEntity
    {

    }


}
