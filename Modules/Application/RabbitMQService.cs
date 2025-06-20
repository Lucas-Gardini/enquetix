using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

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

        public async Task PublishAsync(string queueName, byte[] messageBody)
        {
            var channel = await _lazyChannel.Value;
            var Headers = new Dictionary<string, object?>()
            {
                { "x-retries", 0 }
            };

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: queueName,
                mandatory: true,
                basicProperties: new BasicProperties
                {
                    Headers = Headers,
                },
                body: messageBody
            );
        }

        public async Task Subscribe(string queue, Func<byte[], Task> onMessageAsync)
        {
            var channel = await _lazyChannel.Value;
            await channel.BasicQosAsync(0, 1, false);

            await DeclareQueueAsync(queue);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (sender, ea) =>
            {
                int maxRetries = 3;
                int retryCount = 0;

                // Tenta pegar o header x-retries
                if (ea.BasicProperties.Headers != null && ea.BasicProperties.Headers.TryGetValue("x-retries", out object? value))
                {
                    if (value is byte[] byteArr)
                    {
                        retryCount = int.Parse(Encoding.UTF8.GetString(byteArr));
                    }
                    else if (value is int i)
                    {
                        retryCount = i;
                    }
                }

                try
                {
                    var body = ea.Body.ToArray();
                    await onMessageAsync(body);
                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                }
                catch
                {
                    retryCount++;
                    if (retryCount <= maxRetries)
                    {
                        BasicProperties props = new()
                        {
                            Headers = ea.BasicProperties.Headers ?? new Dictionary<string, object?>()
                        };
                        props.Headers["x-retries"] = retryCount;

                        await channel.BasicPublishAsync(
                            exchange: string.Empty,
                            routingKey: ea.RoutingKey,
                            mandatory: true,
                            basicProperties: props,
                            body: ea.Body.ToArray()
                        );
                    }

                    await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
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
        Task PublishAsync(string queueName, byte[] messageBody);
        Task DeclareQueueAsync(string queueName, bool durable = true, bool exclusive = false, bool autoDelete = false);
        Task Subscribe(string queue, Func<byte[], Task> onMessageAsync);
    }
}
