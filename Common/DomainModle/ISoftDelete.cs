using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModle
{
    public interface ISoftDelete
    {
        public bool IsDelete { get; }
        public void SoftDelete();
    }
}
