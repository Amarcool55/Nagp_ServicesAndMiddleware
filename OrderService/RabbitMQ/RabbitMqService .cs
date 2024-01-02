using RabbitMQ.Client;

namespace OrderService.RabbitMQ
{
    public class RabbitMqService : IRabbitMqService
    {
        public IConnection Connection { get; set; }

        public RabbitMqService() 
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            Connection = factory.CreateConnection();
        }
    }
}
