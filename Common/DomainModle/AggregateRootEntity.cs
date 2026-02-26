using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModle
{
    public  class AggregateRootEntity : BaseEntity, IAggregateRoot, ISoftDelete, IHasCreationTime, IHasDeletionTime
    {
        public DateTime CreationTime { get; private set; }= DateTime.Now;

        public DateTime? DeletionTime { get; private set; }
        public DateTime? LastModificationDate { get; private set; }
        

        public bool IsDelete { get; private set; }

        public virtual void SoftDelete()
        {
            this.IsDelete = true;
            this.DeletionTime = DateTime.Now;
        }

        public virtual void NotifyModified()
        {
            this.LastModificationDate = DateTime.Now;
        }
    }
}
