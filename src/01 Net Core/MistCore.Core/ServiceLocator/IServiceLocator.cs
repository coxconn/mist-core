using System;
using System.Collections.Generic;
using System.Text;

namespace MistCore.Core.ServiceLocator
{
    /// <summary>
    /// IServiceLocator
    /// </summary>
    public interface IServiceLocator
    {
        IServiceProvider Instance { get; }
    }
}
