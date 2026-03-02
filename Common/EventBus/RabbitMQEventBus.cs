using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EventBus
{
    public class RabbitMQEventBus : IEventBus, IDisposable
    {
        private IModel _consumerChannel;

        private readonly RabbitMQConnection _connection;
        private readonly IServiceProvider _serviceProvider;
        private string _queueName;
        private readonly string _exchangeName;
        private readonly IServiceScope serviceScope;
        private readonly SubscriptionsManager _subManager;

        public RabbitMQEventBus(RabbitMQConnection persistentConnection, IServiceScopeFactory serviceFactory, string exchangeName,
            string queueName)
        {
            this._queueName = queueName;
            _connection = persistentConnection;
            this._exchangeName = exchangeName;
            this._subManager = new SubscriptionsManager();
            //因为RabbitMQEventBus是Singleton对象，
            //而它创建的IIntegrationEventHandler以及用到的IIntegrationEventHandler用到的服务
            //大部分是Scoped，
            //因此必须这样显式创建一个scope，           
            //否则在getservice的时候会报错：Cannot resolvefrom root provider because it requires scoped service
            //任何一个IServiceProvider对象都可以通过CreateScope这个扩展方法创建一个IServiceScope对象，
            //相当于创建了一个子容器，每个新建的子容器中都有自己的IServiceProvider对象，
            //值得注意的是，每个新建的子容器中的IServiceProvider对象，都保留着对根容器的引用。
            this.serviceScope = serviceFactory.CreateScope();
            this._serviceProvider = this.serviceScope.ServiceProvider;
            this._consumerChannel = CreateConsumerModel();
            this._subManager.OnEventRemoved += SubsManger_OnEventRemoved;
        }

        private void SubsManger_OnEventRemoved(object? sender, string eventName)
        {
            if (!this._connection.IsConnected)
            {
                _connection.TryConnect();
            }
            using (var channel = _connection.CreateModel())
            {
                channel.QueueUnbind(queue: this._queueName,
                    exchange: _exchangeName,
                    routingKey: eventName);
                if (_subManager.IsEmpty)
                {
                    _queueName = string.Empty;
                    _consumerChannel.Close();
                }
            }
        }


        // 创建通道

        private IModel CreateConsumerModel()
        {
            if (!this._connection.IsConnected)
            {
                _connection.TryConnect();
            }
            var channel = _connection.CreateModel();
            channel.ExchangeDeclare(exchange: _exchangeName, type: "direct");
            var arguments = new Dictionary<string, object>
{
    { "x-max-length", 10 }, // Limit to 10 messages
    { "x-overflow", "reject-publish" } // Reject new publishes if the limit is reached
};
            channel.QueueDeclare(queue: _queueName, durable: true, exclusive: true, autoDelete: false,
                arguments: null);
            channel.CallbackException += (sender, ex) =>
            {
                Debug.Fail(ex.ToString());
            };
            return channel;
        }


        // 是否继承IIntegrationEventHandler
        private void CheckHandlerType(Type handlerType)
        {
            if (!typeof(IIntergrationEventHandler).IsAssignableFrom(handlerType))
            {
                throw new ArgumentException($"{handlerType} doesn't inherit from IIntegrationEventHandler",
                    nameof(handlerType));
            }
        }

        public void Dispose()
        {
            if (_consumerChannel != null)
            {
                _consumerChannel.Dispose();
            }
            _subManager.Clear();
            this._connection.Dispose();
            this.serviceScope.Dispose();
        }

        public void Publish(string eventName, object? eventData)
        {
            if (!_connection.IsConnected)
            {
                _connection.TryConnect();
            }
            //Channel 是建立在 Connection 上的虚拟连接
            //创建和销毁 TCP 连接的代价非常高，
            //Connection 可以创建多个 Channel ，Channel 不是线程安全的所以不能在线程间共享。
            using (var channel = _connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: this._exchangeName, type: "direct");
                byte[] body;
                if (eventData == null)
                {
                    body = new byte[0];
                }
                else
                {
                    JsonSerializerOptions options = new JsonSerializerOptions()
                    {
                        WriteIndented = true,
                    };
                    body = JsonSerializer.SerializeToUtf8Bytes(eventData, eventData.GetType(), options);

                }
                var properties = channel.CreateBasicProperties();
                properties.DeliveryMode = 2;
                properties.Persistent = true;

                channel.BasicPublish(
                    exchange: this._exchangeName,
                    routingKey: eventName,
                    mandatory: true,
                    basicProperties: properties,
                    body: body
                    );

            }
        }


        public void DoInternalSubscription(string eventName)
        {
            var containsKey = _subManager.HasSsubScritpiontForEvent(eventName);
            if (!containsKey)
            {
                if (!this._connection.IsConnected)
                {
                    _connection.TryConnect();
                }
                _consumerChannel.QueueBind(queue: this._queueName,
                    exchange: this._exchangeName,
                    routingKey: eventName);
            }
        }



        // 接受信息
        private async Task Consumer_Received(object sender, BasicDeliverEventArgs args)
        {
            var eventName = args.RoutingKey;
            var message = Encoding.UTF8.GetString(args.Body.Span);
            try
            {
                await ProcessEvent(eventName, message);
                //如果在获取消息时采用不自动应答，但是获取消息后不调用basicAck，
                //RabbitMQ会认为消息没有投递成功，不仅所有的消息都会保留到内存中，
                //而且在客户重新连接后，会将消息重新投递一遍。这种情况无法完全避免，因此EventHandler的代码需要幂等
                _consumerChannel.BasicAck(args.DeliveryTag, multiple: false);
                //multiple：批量确认标志。如果值为true，则执行批量确认，
                //此deliveryTag之前收到的消息全部进行确认;
                //如果值为false，则只对当前收到的消息进行确认
            }
            catch (Exception ex)
            {
                //requeue：表示如何处理这条消息，如果值为true，则重新放入RabbitMQ的发送队列，如果值为false，则通知RabbitMQ销毁这条消息
                _consumerChannel.BasicReject(args.DeliveryTag, true);
                Debug.Fail(ex.ToString());
            }
        }

        // 处理message

        private async Task ProcessEvent(string eventName, string message)
        {
            if (_subManager.HasSsubScritpiontForEvent(eventName))
            {
                var subScriptions = this._subManager.GetHandlerFromEvent(eventName);
                foreach (var subScription in subScriptions)
                {
                    //各自在不同的Scope中，避免DbContext等的共享造成如下问题：
                    //The instance of entity type cannot be tracked because another instance
                    using (var scope = this._serviceProvider.CreateScope())
                    {
                        // 叶serviceProvider基础 根
                        IIntergrationEventHandler? handler = scope.ServiceProvider.GetRequiredService(subScription) as IIntergrationEventHandler;
                        if (handler == null)
                        {
                            throw new ApplicationException($"无法创建{subScription}类型的服务");
                        }
                        await handler.Handle(eventName, message);
                    }
                }
            }
        }

        // 监听开始
        private void StartBasicConsume()
        {
            if (_consumerChannel != null)
            {
                var consumer = new AsyncEventingBasicConsumer(_consumerChannel);
                consumer.Received += Consumer_Received;
                _consumerChannel.BasicConsume(
                    queue: _queueName,
                    autoAck: false,
                    consumer: consumer
                    );
            }
        }


        public void Subscribe(string eventName, Type handlerType)
        {
            CheckHandlerType(handlerType);
            DoInternalSubscription(eventName);
            _subManager.AddSubscription(eventName, handlerType);
            StartBasicConsume();

        }

        public void Unsubscribe(string eventName, Type handlerType)
        {
            CheckHandlerType(handlerType);
            _subManager.RemoveSubscription(eventName, handlerType);
        }
    }
}
