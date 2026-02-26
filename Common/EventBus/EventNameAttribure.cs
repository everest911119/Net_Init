using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple =true)]
    public class EventNameAttribure : Attribute 
    {
        public string Name { get; init; }
        public EventNameAttribure(string name)
        {
            Name = name;
        }
    }
}
