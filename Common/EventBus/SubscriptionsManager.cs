using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus
{
    public class SubscriptionsManager
    {
        //key是eventName，值是监听这个事件的实现了IIntegrationEventHandler接口的类型
        private readonly Dictionary<string,List<Type>> _handlers = new Dictionary<string,List<Type>>();
        public event EventHandler<string> OnEventRemoved;
        public bool IsEmpty => !_handlers.Keys.Any();
        public void Clear()=>_handlers.Clear();
        public bool HasSsubScritpiontForEvent(string eventName)
        {
            return _handlers.ContainsKey(eventName);
        }


        /// <summary>
        /// 把eventHandlerType类型（实现了eventHandlerType接口）注册为监听了eventName事件
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="eventHandlerType"></param>
        /// 
        public void AddSubscription(string eventName,Type eventHandlerType)
        {
            if (!HasSsubScritpiontForEvent(eventName))
            {
                this._handlers.Add(eventName, new List<Type>());
            }
            if (_handlers[eventName].Contains(eventHandlerType))
            {
                throw new ArgumentException($"Handler Type {eventHandlerType} already registered for '{eventName}'", nameof(eventHandlerType));
            }
            this._handlers[eventName].Add(eventHandlerType);
        }

        public void RemoveSubscription(string eventName, Type handlerType)
        {
            _handlers[eventName].Remove(handlerType);
            if (_handlers[eventName].Any())
            {
                _handlers.Remove(eventName);
                OnEventRemoved.Invoke(this, eventName);
            }
        }

        /// <summary>
        /// 得到名字为eventName的所有监听者
        /// </summary>
        /// <param name="eventName"></param>
        /// <returns></returns>
        /// 

        public IEnumerable<Type> GetHandlerFromEvent(string eventName)
        {
            return _handlers[eventName];
        }

    }
}
