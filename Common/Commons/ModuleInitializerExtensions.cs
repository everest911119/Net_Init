using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Commons
{
    public static class ModuleInitializerExtensions
    {
        /// <summary>
		/// 每个项目中都可以自己写一些实现了IModuleInitializer接口的类，在其中注册自己需要的服务，这样避免所有内容到入口项目中注册
		/// </summary>
		/// <param name="services"></param>
		/// <param name="assemblies"></param>
        /// 
        public static IServiceCollection RunModuleInitializers(this IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            foreach (Assembly assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();
                var moduleInitializerTypes = types.Where(t => !t.IsAbstract && typeof(IModuleInitializer).IsAssignableFrom(t));
                foreach (var impType in moduleInitializerTypes)
                {
                    var initailizer = (IModuleInitializer?) Activator.CreateInstance(impType);
                    if (initailizer == null)
                    {
                        throw new ApplicationException($"Cannot create {impType}");
                    }
                    initailizer.Initialize(services);
                }
            }
            return services;
        }
    }
}