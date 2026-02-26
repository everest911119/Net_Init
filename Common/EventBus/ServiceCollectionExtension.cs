using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
namespace EventBus
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddEventBus(this IServiceCollection services,string queueName,
            params Assembly[] assemblies)
        {
            return AddEventBus(services,queueName,assemblies.ToList());
        }

        public static IServiceCollection AddEventBus(this IServiceCollection services,
            string queueName,IEnumerable<Assembly> assemblies)
        {
            List<Type> eventHandlers = new List<Type>();
            foreach (Assembly assembly in assemblies)
            {
                var type = assembly.GetTypes().Where(t => t.IsAbstract == false
                && t.IsAssignableTo(typeof(IIntergrationEventHandler)));
                eventHandlers.AddRange(type);
            }
            return AddEventBus(services, queueName, eventHandlers);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="queueName">如果多个消费者订阅同一个Queue，这时Queue中的消息会被平均分摊给多个消费者进行处理，而不是每个消费者都收到所有的消息并处理。为了确保一个应用监听到所有的领域事件，所以不同前端项目的queueName需要不一样。
        /// 因此，对于同一个应用，这个queueName需要保证在多个集群实例和多次运行保持一致，这样可以保证应用重启后仍然能收到没来得及处理的消息。而且这样同一个应用的多个集群实例只有一个能收到一条消息，不会同一条消息被一个应用的多个实例处理。这样消息的处理就被平摊到多个实例中。
        ///</param>
        /// <param name="eventHandlerTypes"></param>
        /// <returns></returns>
        /// 
        public static IServiceCollection AddEventBus(this IServiceCollection services,
            string queueName,IEnumerable<Type> eventHandlerTypes)
        {
            foreach (var type in eventHandlerTypes)
            {
                services.AddScoped(type, type);
            }
            //Singleton：IServiceProvider对象创建的服务实例保存在作为根容器的IServiceProvider对象上，
            //所以多个同根的IServiceProvider对象提供的针对同一类型的服务实例都是同一个对象。
            services.AddSingleton<IEventBus>(sp =>
            {

                var rabbitMQ =
                sp.GetRequiredService<IOptions<IntegrationEventRabbitMQOptions>>().Value;
                var factory = new ConnectionFactory()
                {
                    HostName = rabbitMQ.HostName,
                    Port = rabbitMQ.Port,
                    DispatchConsumersAsync = true
                };
                if (rabbitMQ.UserName != null)
                {
                    factory.UserName = rabbitMQ.UserName;
                }
                if (rabbitMQ.Password != null)
                {
                    factory.Password = rabbitMQ.Password;
                }
                //eventBus归DI管理，释放的时候会调用Dispose
                //eventbus的Dispose中会销毁RabbitMQConnection
                RabbitMQConnection mqConnection = new RabbitMQConnection(factory);
                IServiceScopeFactory serviceScopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
                RabbitMQEventBus eventBus = new RabbitMQEventBus(mqConnection,
                    serviceScopeFactory, rabbitMQ.ExchangeName, queueName);
                foreach (var type in eventHandlerTypes)
                {
                    var eventNameAttrs = type.GetCustomAttributes<EventNameAttribure>();
                    if (eventNameAttrs.Any() == false)
                    {
                        throw new ApplicationException($"There shoule be at least one EventNameAttribute on {type}");
                    }
                    foreach (var eventName in eventNameAttrs)
                    {
                        eventBus.Subscribe(eventName.Name, type);
                    }
                }
                return eventBus;

            });
            return services;
        }
    }
}
