using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace RabbitMqProducer.RabbitMq
{
  public class RabbitMqService : IRabbitMqService
    {
        private const string QueueName = "queue-test";

        private readonly IConnection _connection;
        private readonly IChannel _channel;

        public RabbitMqService()
        {
            var factory = new ConnectionFactory
            {
                HostName = "rabbitmq",
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };

            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

            _channel.QueueDeclareAsync(
                queue: QueueName,
                durable: false,
                exclusive: false,
                autoDelete: false
            ).GetAwaiter().GetResult();
        }

        public async Task SendMessageAsync(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);

            await _channel.BasicPublishAsync(
                exchange: "",
                routingKey: QueueName,
                body: body
            );
        }

        public async Task SendMessageAsync<T>(T message)
        {
            var json = JsonSerializer.Serialize(message);
            await SendMessageAsync(json);
        }
    }
}














