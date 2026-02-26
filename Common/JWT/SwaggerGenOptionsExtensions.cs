using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JWT
{
    public static class SwaggerGenOptionsExtensions
    {
        public static void AddAuthenticationHeader(this SwaggerGenOptions c)
        {
            var scheme = new OpenApiSecurityScheme()
            {
                Description = "Authorization header. \r\nExample: 'Bearer 12345abcdef'",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Authorization"
                },
                Scheme = "oauth2",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
            };
            c.AddSecurityDefinition("Authorization", scheme);
            var requirement = new OpenApiSecurityRequirement();
            requirement[scheme] = new List<string>();
            c.AddSecurityRequirement(requirement);
        }
    }
}
