namespace RabbitMqProducer.RabbitMq
{
    public interface IRabbitMqService
    {
        Task SendMessageAsync(string message);
        Task SendMessageAsync<T>(T obj);
    }
}
