using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus
{
    public interface IIntergrationEventHandler
    {
        Task Handle(string eventName, string EventData);
    }
}
