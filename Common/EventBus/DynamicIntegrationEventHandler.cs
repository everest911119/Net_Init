using Dynamic.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus
{
    public abstract class DynamicIntegrationEventHandler : IIntergrationEventHandler
    {
        public Task Handle(string eventName, string EventData)
        {
            dynamic dynamicEventData = DJson.Parse(EventData);
            return HandleDynamic(eventName, dynamicEventData);

        }

        public abstract Task HandleDynamic(string eventName, dynamic eventData);
    }
}
