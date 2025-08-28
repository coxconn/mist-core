using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MistCore.Framework.EventBus
{
    /// <summary>
    /// IEventBus
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// Trigger
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="routeKey"></param>
        /// <param name="event"></param>
        void Trigger<TEvent>(string routeKey, TEvent @event);

        /// <summary>
        /// Register
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <typeparam name="TEventHandler"></typeparam>
        /// <param name="routeKey"></param>
        void Register<TEvent, TEventHandler>(string routeKey) where TEventHandler : IEventHandler<TEvent>;

        /// <summary>
        /// UnRegister
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <typeparam name="TEventHandler"></typeparam>
        /// <param name="routeKey"></param>
        void UnRegister<TEvent, TEventHandler>(string routeKey) where TEventHandler : IEventHandler<TEvent>;

    }
}
