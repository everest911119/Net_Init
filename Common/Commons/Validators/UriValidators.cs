using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentValidation
{
    public static class UriValidators
    {
        // url 长度
        public static IRuleBuilderOptions<T, Uri> Length<T>(this IRuleBuilder<T, Uri> ruleBuilder, int min, int max)
        {
            return ruleBuilder.Must(p => string.IsNullOrWhiteSpace(p.OriginalString) 
            || (p.OriginalString.Length > min &&
            p.OriginalString.Length < max)).WithMessage($"The length of Uri must not be between {min} and {max}.");
        }

        public static IRuleBuilderOptions<T, Uri> NotEmptyUri<T>(this IRuleBuilder<T, Uri> ruleBuilder)
        {
            return ruleBuilder.Must(p => p == null || !string.IsNullOrWhiteSpace(p.OriginalString))
                .WithMessage("The Uri must not be null nor empty.");
        }

       
    }
}
