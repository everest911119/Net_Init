using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EventBus
{
    public abstract class JsonIntegrationEventHandler<T> : IIntergrationEventHandler
    {
        public Task Handle(string eventName, string EventData)
        {
            T? eventData = JsonSerializer.Deserialize<T>(EventData);
            return HandleJson(eventName, eventData);
        }
        public abstract Task HandleJson(string eventName, T? EventData);  
    }
}
