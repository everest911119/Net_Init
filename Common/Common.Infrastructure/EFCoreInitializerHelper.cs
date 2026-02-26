
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore
{
    public static class EFCoreInitializerHelper
    {
        /// <summary>
        /// 自动为所有的DbContext注册连接配置
        /// </summary>
        /// <param name="services"></param>
        /// <param name="builder"></param>
        /// <param name="assemblies"></param>
        public static IServiceCollection AddAllDbContext(this IServiceCollection services,
            Action<DbContextOptionsBuilder> builder, IEnumerable<Assembly> assemblies)
        {
            Type type = typeof(EntityFrameworkServiceCollectionExtensions);
            string name = nameof(EntityFrameworkServiceCollectionExtensions.AddDbContext);
            //AddDbContextPool不支持DbContext注入其他对象，而且使用不当有内存暴涨的问题，因此不用AddDbContextPool
            Type[] types = new Type[] { typeof(IServiceCollection),
                typeof(Action<DbContextOptionsBuilder>),
                typeof(ServiceLifetime),
                typeof(ServiceLifetime)
            };
            var methodAddDbContext = typeof(EntityFrameworkServiceCollectionExtensions).
                GetMethod(nameof(EntityFrameworkServiceCollectionExtensions.AddDbContext),
                1
                , types);
            foreach (var asmToLoad in assemblies)
            {
                Type[] typesInAsm = asmToLoad.GetTypes();
                foreach (var dbCtxType in typesInAsm.Where(t => !t.IsAbstract && typeof(DbContext).IsAssignableFrom(t)))
                {
                    var methodGenericAddContext = methodAddDbContext.MakeGenericMethod(dbCtxType);
                    methodGenericAddContext.Invoke(null, new object[] { services, builder, ServiceLifetime.Scoped, ServiceLifetime.Scoped });
                }
            }
            return services;
        }
    }
}
