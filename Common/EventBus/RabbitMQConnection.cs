
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IConnectionFactory = RabbitMQ.Client.IConnectionFactory;

namespace EventBus
{
    public class RabbitMQConnection :IDisposable
    {
        private readonly IConnectionFactory _connectionFactory;
        private bool _disposed;
        private IConnection _connection;
        private readonly object _lock = new object();
        public RabbitMQConnection(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public bool IsConnected
        {
            get
            {
                return _connection != null && _connection.IsOpen && !_disposed;
            }
        }

        public IModel CreateModel()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("No Rabbit Connection avaiable");
            }
            return _connection.CreateModel();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _connection.Dispose();
        }

        public bool TryConnect()
        {
            lock(_lock)
            {
                _connection = _connectionFactory.CreateConnection();
            }
            if (IsConnected)
            {
                _connection.ConnectionShutdown += OnConnectionShutDown;
                _connection.ConnectionBlocked += OnConnectionBlocked;
                _connection.CallbackException += OnCallBackException;
                return true;
            }else
            {
                return false;
            }
        }

        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs eventArgs)
        {
            if (_disposed) return;
            TryConnect();
        }

        private void OnConnectionShutDown(object sender, ShutdownEventArgs eventArgs)
        {
            if (_disposed) return; 
            TryConnect();
        }
        private void OnCallBackException(object sender, CallbackExceptionEventArgs eventArgs)
        {
            if (_disposed) return;
            TryConnect();
        }

    }
}
