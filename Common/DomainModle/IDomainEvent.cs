using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModle
{
    public interface IDomainEvent
    {
        public IEnumerable<INotification> GetDomainEvent();
        public void AddDomainEvent(INotification notification);
        public void ClearDomainEvent();
        public void AddDomainEventIfAbsent(INotification eventItem);
    }
}
