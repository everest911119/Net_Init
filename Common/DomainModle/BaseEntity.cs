using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModle
{
    public class BaseEntity : IEntity, IDomainEvent
    {
        [NotMapped]
        private List<INotification> domainEvents = new List<INotification>();
        public Guid Id { get; protected set; } = Guid.NewGuid();

        public void AddDomainEvent(INotification eventItem)
        {
            domainEvents.Add(eventItem);
        }

        public void AddDomainEventIfAbsent(INotification eventItem)
        {
            if (!domainEvents.Contains(eventItem)) domainEvents.Add(eventItem);
        }

        public void ClearDomainEvent()
        {
            domainEvents.Clear();
        }

        public IEnumerable<INotification> GetDomainEvent()
        {
            return domainEvents;
        }
    }
}
