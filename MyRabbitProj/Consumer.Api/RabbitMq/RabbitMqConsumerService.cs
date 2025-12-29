using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Consumer.Api.RabbitMq;

public class RabbitMqConsumerService : BackgroundService
{
    private readonly ILogger<RabbitMqConsumerService> _logger;
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly string _queueName = "queue-test";
    
    public ConsumerState State { get; private set; } = ConsumerState.Stopped;
    public string? LastError { get; private set; }

    public RabbitMqConsumerService(ILogger<RabbitMqConsumerService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Start consumer");
        State = ConsumerState.Starting;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await StartConsumerAsync(stoppingToken);
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Consumer остановлен");
                break;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                _logger.LogError(ex, "Ошибка подключения переподключение, через 10 секунд ");
                State = ConsumerState.Faulted;
                
                await Task.Delay(10000, stoppingToken);
            }
        }
        
        await StopConsumerAsync();
    }

    private async Task StartConsumerAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = "rabbitmq",
            Port = 5672,
            UserName = "guest",
            Password = "guest"
        };

        _logger.LogInformation("Коннект {Host}:{Port}...", 
            factory.HostName, factory.Port);

        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync();
        await _channel.QueueDeclareAsync(
            queue: _queueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken
        );

        _logger.LogInformation("Поключение к RabbitMq: Queue: {Queue}", _queueName);
        var consumer = new AsyncEventingBasicConsumer(_channel);
        
        consumer.ReceivedAsync += async (sender, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                _logger.LogInformation("Отправленно: {Message}", message);
                
                await ProcessMessageAsync(message, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошбика обработки сообщения");
            }
        };
        await _channel.BasicConsumeAsync(
            queue: _queueName,
            autoAck: true,
            consumer: consumer,
            cancellationToken: stoppingToken
        );

        State = ConsumerState.Running;
        _logger.LogInformation("Consumer запущен устпешно");
    }

    private async Task ProcessMessageAsync(string message, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Обработка сообщения: {Message}", message);
        await Task.CompletedTask;
    }

    private async Task StopConsumerAsync()
    {
        _logger.LogInformation("Остановки сервиса RabbitMq");
        State = ConsumerState.Stopped;

        try
        {
            if (_channel?.IsClosed == false)
            {
                await _channel.CloseAsync();
                _logger.LogInformation("Канал приостановлен");
            }

            if (_connection?.IsOpen == true)
            {
                await _connection.CloseAsync();
                _logger.LogInformation("Подключение остановлена");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка остановка RabbitMq");
        }
        
        _channel?.Dispose();
        _connection?.Dispose();
        
        _logger.LogInformation("RabbitMq остановлен");
    }

    public override void Dispose()
    {
        StopConsumerAsync().GetAwaiter().GetResult();
        base.Dispose();
    }
}

public enum ConsumerState
{
    Stopped,
    Starting,
    Running,
    Faulted
}