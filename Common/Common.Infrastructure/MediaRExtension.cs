using DomainModle;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Infrastructure
{
    namespace MediatR
    {
        public static class MediatorExtensions
        {
            public static async Task DispatchDomainEventAysnc(this IMediator mediator, DbContext ctx)
            {
                var domainEntries = ctx.ChangeTracker.Entries<IDomainEvent>().
                    Where(c=>c.Entity.GetDomainEvent().Any());
                // 加ToList()是为立即加载，否则会延迟执行，到foreach的时候已经被ClearDomainEvents()了
                var domainEvents = domainEntries.SelectMany(c => c.Entity.GetDomainEvent()).ToList();
                domainEntries.ToList().ForEach(domainEvent => { domainEvent.Entity.ClearDomainEvent(); });
                foreach (var domainEvent in domainEvents)
                {
                    await mediator.Publish(domainEvent);
                }

            }
        }
    }
}
