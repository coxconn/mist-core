using System;
using System.Collections.Generic;
using System.Text;

namespace MistCore.Data
{
    public interface IEntity 
    {
    }

    public abstract class Entity : IEntity
    {
    }

    public interface IEntity<TPrimaryKey> : IEntity
    {
        TPrimaryKey Id { get; set; }
    }

    public abstract class Entity<TPrimaryKey> : Entity, IEntity<TPrimaryKey>
    {
        public virtual TPrimaryKey Id { get; set; }
    }


}
