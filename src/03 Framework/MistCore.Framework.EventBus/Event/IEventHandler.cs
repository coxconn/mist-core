using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MistCore.Framework.EventBus
{

    /// <summary>
    /// IEventHandler
    /// </summary>
    public interface IEventHandler<in TEvent>
    {
        Task<bool> HandleEventAsync(TEvent @event);
    }
}
