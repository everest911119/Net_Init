using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
namespace Commons
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class,AllowMultiple =false,
        Inherited =true)]
    public class UnitOfWorkAttribute : Attribute
    {
        public Type[] DbContextTypes { get; init; }
        public UnitOfWorkAttribute(params Type[] DbContextTypes)
        {
            this.DbContextTypes = DbContextTypes;
            foreach(var type in DbContextTypes)
            {
                if (!typeof(DbContext).IsAssignableFrom(type))
                {
                    throw new ArgumentException($"{type} must inherit from Dbcontext");
                }
            }
        }
    }
}
