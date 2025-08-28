using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MistCore.Framework.EventBus
{
    /// <summary>
    /// IEventHandlerExecutionContext
    /// </summary>
    public interface IEventHandlerExecutionContext
    {
        /// <summary>
        /// RegisterEventHandler
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <typeparam name="TEventHandler"></typeparam>
        /// <param name="routeKey"></param>
        /// <returns></returns>
        bool RegisterEventHandler<TEvent, TEventHandler>(string routeKey) where TEventHandler : IEventHandler<TEvent>;

        /// <summary>
        /// UnRegisterEventHandler
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <typeparam name="TEventHandler"></typeparam>
        /// <param name="routeKey"></param>
        /// <returns></returns>
        bool UnRegisterEventHandler<TEvent, TEventHandler>(string routeKey) where TEventHandler : IEventHandler<TEvent>;

        /// <summary>
        /// GetAll
        /// </summary>
        /// <param name="routeKey"></param>
        /// <returns></returns>
        IEnumerable<Tuple<string, Type, Type>> GetAll(string routeKey = null);

        /// <summary>
        /// IsRegisterEventHandler
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <typeparam name="TEventHandler"></typeparam>
        /// <param name="routeKey"></param>
        /// <returns></returns>
        bool IsRegisterEventHandler<TEvent, TEventHandler>(string routeKey) where TEventHandler : IEventHandler<TEvent>;

        /// <summary>
        /// HandleAsync
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="routeKey"></param>
        /// <param name="event"></param>
        /// <returns></returns>
        Task<bool> HandleAsync<TEvent>(string routeKey, TEvent @event);

        /// <summary>
        /// HandleAsync
        /// </summary>
        /// <param name="routeKey"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        Task<bool> HandleAsync(string routeKey, string json);

    }
}
