using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Channels;

namespace NotificationService2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            using var connection = factory.CreateConnection();

            using var UpdateOrderChannel = connection.CreateModel();
            UpdateOrderChannel.QueueDeclare("OrderUpdateQueue", durable: true, exclusive: false, autoDelete: false);
            UpdateOrderChannel.ExchangeDeclare("TopicExchange", ExchangeType.Topic, durable: true, autoDelete: false);
            UpdateOrderChannel.QueueBind("OrderUpdateQueue", "TopicExchange", "OrderUpdated");

            using var CreateOrderChannel = connection.CreateModel();
            CreateOrderChannel.QueueDeclare("OrderCreationQueue2", durable: true, exclusive: false, autoDelete: false);
            CreateOrderChannel.ExchangeDeclare("FanoutExchange", ExchangeType.Fanout, durable: true, autoDelete: false);
            CreateOrderChannel.QueueBind("OrderCreationQueue2", "FanoutExchange", string.Empty);

            Console.WriteLine(" [*] Waiting for messages.");

            var updateEventConsumer = new EventingBasicConsumer(UpdateOrderChannel);
            updateEventConsumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($" Order Update Event Received {message}");
            };

            UpdateOrderChannel.BasicConsume(queue: "OrderUpdateQueue",
                     autoAck: true,
                     consumer: updateEventConsumer);

            var createOrderConsumer = new EventingBasicConsumer(CreateOrderChannel);
            createOrderConsumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($" Order Creation Event Received {message}");
            };

            CreateOrderChannel.BasicConsume(queue: "OrderCreationQueue2",
                     autoAck: true,
                     consumer: createOrderConsumer);

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}