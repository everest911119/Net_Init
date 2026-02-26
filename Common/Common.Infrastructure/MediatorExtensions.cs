using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MediatR;
namespace Common.Infrastructure
{
    public static class MediatorExtensions
    {

        /// <summary>
        /// 把rootAssembly及直接或者间接引用的程序集（排除系统程序集）中的MediatR 相关类进行注册
        /// </summary>
        /// <param name="services"></param>
        /// <param name="rootAssembly"></param>
        /// <returns></returns>
        /// 
        public static IServiceCollection AddMyMediatR(this IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            return services.AddMediatR(cfg=>cfg.RegisterServicesFromAssemblies(assemblies.ToArray()));
        }
    }
}
