using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MistCore.Data.EventBus
{

    /// <summary>
    /// 定义事件源接口，所有的事件源都要实现该接口
    /// </summary>
    public interface IEventData
    {
        /// <summary>
        /// 事件发生的时间
        /// </summary>
        DateTime EventTime { get; set; }

        /// <summary>
        /// 触发事件的对象
        /// </summary>
        object EventSource { get; set; }
    }

    /// <summary>
    /// 定义事件处理器公共接口，所有的事件处理都要实现该接口
    /// </summary>
    public interface IEventHandler
    {
    }

     /// <summary>
     /// 泛型事件处理器接口
     /// </summary>
     /// <typeparam name="TEventData"></typeparam>
     public interface IEventHandler<TEventData> : IEventHandler where TEventData : IEventData
     {
         /// <summary>
         /// 事件处理器实现该方法来处理事件
         /// </summary>
         /// <param name="eventData"></param>
         void HandleEvent(TEventData eventData);
     }


     public interface IEventBus
     {
         void Register<TEventData>(IEventHandler<TEventData> eventHandler) where TEventData : IEventData;
         void Register<TEventData>(Action<TEventData> action) where TEventData : IEventData;

         void UnRegister<TEventData>(IEventHandler<TEventData> eventHandler) where TEventData : IEventData;
         void UnRegisterAll<TEventData>(IEventHandler<TEventData> eventHandler) where TEventData : IEventData;

         void Trigger<TEventData>(TEventData eventData) where TEventData : IEventData;
     }



    public class EventBus : IEventBus
    {
        private class Event<TEventData> where TEventData : IEventData
        {
            public delegate void HandleEventExecute(TEventData data);
            public event HandleEventExecute HandleEvent;
            public void Execute(TEventData data)
            {
                HandleEvent(data);
            }
        }

        private static Dictionary<Type, object> _dicEventHandler = new Dictionary<Type, object>();
        private readonly object _syncObject = new object();

        public void Register<TEventData>(IEventHandler<TEventData> eventHandler) where TEventData: IEventData
        {
            lock (_syncObject)
            {
                var eventType = typeof(TEventData);

                if (_dicEventHandler.ContainsKey(eventType))
                {
                    var handlers = (Event<TEventData>)_dicEventHandler[eventType];
                    if (handlers != null)
                    {
                        handlers.HandleEvent += eventHandler.HandleEvent;
                    }
                    else
                    {
                        handlers = new Event<TEventData>();
                        handlers.HandleEvent += eventHandler.HandleEvent;
                    }
                }
                else
                {
                    var handlers = new Event<TEventData>();
                    handlers.HandleEvent += eventHandler.HandleEvent;
                    _dicEventHandler.Add(eventType, handlers);
                }
            }
        }

        public void Register<TEventData>(Action<TEventData> action) where TEventData : IEventData
        {
            lock (_syncObject)
            {
                var eventType = typeof(TEventData);

                if (_dicEventHandler.ContainsKey(eventType))
                {
                    var handlers = (Event<TEventData>)_dicEventHandler[eventType];
                    if (handlers != null)
                    {
                        handlers.HandleEvent += (a)=> action(a);
                    }
                    else
                    {
                        handlers = new Event<TEventData>();
                        handlers.HandleEvent += (a) => action(a);
                    }
                }
                else
                {
                    var handlers = new Event<TEventData>();
                    handlers.HandleEvent += (a) => action(a);
                    _dicEventHandler.Add(eventType, handlers);
                }
            }
        }

        public void UnRegister<TEventData>(IEventHandler<TEventData> eventHandler) where TEventData : IEventData
        {
            throw new NotImplementedException();
        }

        public void UnRegisterAll<TEventData>(IEventHandler<TEventData> eventHandler) where TEventData : IEventData
        {
            throw new NotImplementedException();
        }

        public void Trigger<TEventData>(TEventData eventData) where TEventData : IEventData
        {
            lock (_syncObject)
            {
                var eventType = typeof(TEventData);
                if (_dicEventHandler.ContainsKey(eventType))
                {
                    var handlers = (Event<TEventData>)_dicEventHandler[eventType];
                    handlers.Execute(eventData);
                }
            }
        }

    }
}
