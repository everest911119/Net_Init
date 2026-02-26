using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons
{
    [AttributeUsage(AttributeTargets.Property| AttributeTargets.Field |
        AttributeTargets.Parameter, AllowMultiple = false)]
    public class RequireGuidAttribute : ValidationAttribute
    {
        public const string DefaultErrorMessage = "The {0} field is requird and not Guid.Empty";
        public RequireGuidAttribute() : base(DefaultErrorMessage){ }
        public override bool IsValid(object? value)
        {
            if (value is null) return false;
            if (value is Guid)
            {
                Guid guid = (Guid)value;
                return guid != Guid.Empty;
            }else
            {
                return false;
            }
        }
    }
}
