using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Reflection;
using System.Text;

namespace NotificationService1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.QueueDeclare("OrderCreationQueue1", durable: true, exclusive: false, autoDelete: false);
            channel.ExchangeDeclare("FanoutExchange", ExchangeType.Fanout, durable: true, autoDelete: false);
            channel.QueueBind("OrderCreationQueue1", "FanoutExchange", string.Empty);

            Console.WriteLine(" [*] Waiting for messages.");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($" Order Creation Event Received {message}");
            };

            channel.BasicConsume(queue: "OrderCreationQueue1",
                     autoAck: true,
                     consumer: consumer);

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}