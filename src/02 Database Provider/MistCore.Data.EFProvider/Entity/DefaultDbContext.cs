using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MistCore.Data.EFProvider.Entity
{
    /// <summary>
    /// DefaultDbContext
    /// </summary>
    public abstract class DefaultDbContext : DbContext
    {
        protected DefaultDbContext(DbContextOptions options) : base(options)
        {
        }
    }
}
