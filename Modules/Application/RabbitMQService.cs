using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace enquetix.Modules.Application
{
    public class RabbitMQService : IRabbitMQService
    {
        private readonly Lazy<Task<IConnection>> _lazyConnection;
        private readonly Lazy<Task<IChannel>> _lazyChannel;
        public RabbitMQService(IConfiguration configuration)
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri(configuration.GetConnectionString("RabbitMQ")!),
            };

            _lazyConnection = new Lazy<Task<IConnection>>(async () =>
            {
                return await factory.CreateConnectionAsync();
            });

            _lazyChannel = new Lazy<Task<IChannel>>(async () =>
            {
                var connection = await _lazyConnection.Value;
                return await connection.CreateChannelAsync();
            });
        }

        public async Task DeclareQueueAsync(string queueName, bool durable = true, bool exclusive = false, bool autoDelete = false)
        {
            var channel = await _lazyChannel.Value;
            await channel.QueueDeclareAsync(queue: queueName, durable: durable, exclusive: exclusive, autoDelete: autoDelete, arguments: null);
        }

        public async Task PublishAsync(string exchange, string routingKey, byte[] messageBody)
        {
            var channel = await _lazyChannel.Value;
            await channel.BasicPublishAsync(exchange: exchange, routingKey: routingKey, body: messageBody);
        }

        public async Task Subscribe(string queue, Func<byte[], Task> onMessageAsync)
        {
            var channel = await _lazyChannel.Value;

            await DeclareQueueAsync(queue);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (sender, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    await onMessageAsync(body);
                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                }
                catch
                {
                    await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
                    throw;
                }
            };

            await channel.BasicConsumeAsync(queue: queue, autoAck: false, consumer: consumer);
        }

        public async ValueTask DisposeAsync()
        {
            if (_lazyChannel.IsValueCreated)
            {
                var channel = await _lazyChannel.Value;
                await channel.CloseAsync();
                channel.Dispose();
            }

            if (_lazyConnection.IsValueCreated)
            {
                var connection = await _lazyConnection.Value;
                await connection.CloseAsync();
                connection.Dispose();
            }

            GC.SuppressFinalize(this);
        }
    }

    public interface IRabbitMQService : IAsyncDisposable
    {
        Task PublishAsync(string exchange, string routingKey, byte[] messageBody);
        Task DeclareQueueAsync(string queueName, bool durable = true, bool exclusive = false, bool autoDelete = false);
        Task Subscribe(string queue, Func<byte[], Task> onMessageAsync);
    }
}
