using RabbitMQ.Client;

namespace OrderService.RabbitMQ
{
    public interface IRabbitMqService
    {
        IConnection Connection { get; }
    }
}
