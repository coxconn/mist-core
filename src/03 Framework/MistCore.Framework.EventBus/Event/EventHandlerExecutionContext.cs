using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MistCore.Framework.EventBus
{

    /// <summary>
    /// EventHandlerExecutionContext
    /// </summary>
    public class EventHandlerExecutionContext : IEventHandlerExecutionContext
    {

        private readonly Dictionary<string, List<Tuple<Type, Type>>> registrations = new Dictionary<string, List<Tuple<Type, Type>>>();

        private IServiceProvider serviceProvider;

        public EventHandlerExecutionContext(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// GetAll
        /// </summary>
        /// <param name="routeKey"></param>
        /// <returns></returns>
        public IEnumerable<Tuple<string, Type, Type>> GetAll(string routeKey = null)
        {
            if (!string.IsNullOrWhiteSpace(routeKey))
            {
                if (registrations.TryGetValue(routeKey, out List<Tuple<Type, Type>> handlertypelist))
                {
                    foreach (var item in handlertypelist)
                    {
                        yield return new Tuple<string, Type, Type>(routeKey, item.Item1, item.Item2);
                    }
                }
            }
            else
            {
                foreach (var dic in registrations)
                {
                    foreach (var item in dic.Value)
                    {
                        yield return new Tuple<string, Type, Type>(dic.Key, item.Item1, item.Item2);
                    }
                }
            }
        }

        /// <summary>
        /// HandleAsync
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="routeKey"></param>
        /// <param name="event"></param>
        /// <returns></returns>
        public async Task<bool> HandleAsync<TEvent>(string routeKey, TEvent @event)
        {
            //var key = !string.IsNullOrWhiteSpace(routeKey) ? routeKey : typeof(TEvent).FullName;
            var key = routeKey;
            var result = false;

            if (registrations.TryGetValue(key, out List<Tuple<Type, Type>> handlertypes) && handlertypes.Count > 0)
            {
                foreach (var handlertype in handlertypes)
                {
                    if (handlertype.Item1 == @event.GetType())
                    {
                        //var handler = Activator.CreateInstance(handlertype.Item2) as IEventHandler;
                        var handler = this.serviceProvider.GetService(handlertype.Item2);
                        var handleEvent = handlertype.Item2.GetMethod("HandleEventAsync");
                        result = await Task.Run(() => (Task<bool>)handleEvent.Invoke(handler, new object[] { @event }));
                    }
                }
            }
            return result;
        }

        public async Task<bool> HandleAsync(string routeKey, string json)
        {
            var result = false;

            if (string.IsNullOrEmpty(routeKey))
            {
                return result;
            }

            if (registrations.TryGetValue(routeKey, out List<Tuple<Type, Type>> handlertypes) && handlertypes.Count > 0)
            {
                foreach (var handlertype in handlertypes)
                {
                    var data = Newtonsoft.Json.JsonConvert.DeserializeObject(json, handlertype.Item1);

                    if (handlertype.Item1 == data.GetType())
                    {
                        //var handler = Activator.CreateInstance(handlertype.Item2) as IEventHandler;
                        var handler = this.serviceProvider.GetService(handlertype.Item2);
                        var handleEvent = handlertype.Item2.GetMethod("HandleEventAsync");
                        result = await Task.Run(() => (Task<bool>)handleEvent.Invoke(handler, new object[] { data }));
                    }
                }
            }
            return result;
        }

        public bool IsRegisterEventHandler<TEvent, TEventHandler>(string routeKey) where TEventHandler : IEventHandler<TEvent>
        {
            //var key = !string.IsNullOrWhiteSpace(routeKey) ? routeKey : typeof(TEvent).FullName;
            var key = routeKey;

            if (registrations.TryGetValue(key, out List<Tuple<Type, Type>> handlertypelist))
            {
                return handlertypelist != null && handlertypelist.Any(c => c.Item1 == typeof(TEvent) && c.Item2 == typeof(TEventHandler));
            }
            return false;
        }

        public bool RegisterEventHandler<TEvent, TEventHandler>(string routeKey) where TEventHandler : IEventHandler<TEvent>
        {
            //var key = !string.IsNullOrWhiteSpace(routeKey) ? routeKey : typeof(TEvent).FullName;
            var key = routeKey;

            lock (this.registrations)
            {
                if (!registrations.TryGetValue(key, out List<Tuple<Type, Type>> handlertypelist))
                {
                    registrations.Add(key, new List<Tuple<Type, Type>> { new Tuple<Type, Type>(typeof(TEvent), typeof(TEventHandler)) });
                    return true;
                }
                else if (!handlertypelist.Any(c => c.Item1 == typeof(TEvent) && c.Item2 == typeof(TEventHandler)))
                {
                    handlertypelist.Add(new Tuple<Type, Type>(typeof(TEvent), typeof(TEventHandler)));
                    return true;
                }
            }
            return false;
        }

        public bool UnRegisterEventHandler<TEvent, TEventHandler>(string routeKey) where TEventHandler : IEventHandler<TEvent>
        {
            //var key = !string.IsNullOrWhiteSpace(routeKey) ? routeKey : typeof(TEvent).FullName;
            var key = routeKey;

            lock (this.registrations)
            {
                if (registrations.TryGetValue(routeKey, out List<Tuple<Type, Type>> handlertypelist))
                {
                    var items = handlertypelist.Where(c => c.Item1 == typeof(TEvent) && c.Item2 == typeof(TEventHandler)).ToList();
                    foreach (var item in items)
                    {
                        handlertypelist.Remove(item);
                    }
                    if (items.Count == 0)
                    {
                        registrations.Remove(key);
                    }
                    return items.Count > 0;
                }
            }
            return false;
        }
    }
}
