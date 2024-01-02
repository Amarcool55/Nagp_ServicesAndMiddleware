using Grpc.Core;
using OrderService.Protos;
using OrderService.RabbitMQ;
using OrderService.Repositories;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace OrderService.Services
{
    public class OrderService : OrderProcessing.OrderProcessingBase
    {
        private readonly ILogger<OrderService> _logger;
        private readonly IOrderRepository _orderRepository;
        private readonly IConnection _rabbitMqConnection;

        public OrderService(ILogger<OrderService> logger, IOrderRepository orderRepository, IRabbitMqService rabbitMqService)
        {
            _logger = logger;
            _orderRepository = orderRepository;
            _rabbitMqConnection = rabbitMqService.Connection;
        }

        public override Task<OrderReply> PlaceOrder(OrderRequest request, ServerCallContext context)
        {
            _logger.LogInformation($"RPC method: {context.Method} recieved from :{context.Peer}. Host: {context.Host} ");
            var orderDetails = _orderRepository.PlaceOrder(request.OrderDetails);

            // Connect to RabbitMQ and push the message to exchange
            using var model = _rabbitMqConnection.CreateModel();
            model.QueueDeclare("OrderCreationQueue1", durable: true, exclusive: false, autoDelete: false);
            model.QueueDeclare("OrderCreationQueue2", durable: true, exclusive: false, autoDelete: false);
            model.ExchangeDeclare("FanoutExchange", ExchangeType.Fanout, durable: true, autoDelete: false);
            model.QueueBind("OrderCreationQueue1", "FanoutExchange", string.Empty);
            model.QueueBind("OrderCreationQueue2", "FanoutExchange", string.Empty);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(orderDetails));
            model.BasicPublish("FanoutExchange", string.Empty, null, body);

            model.Close();
            // Send reply to GRPC Client
            return Task.FromResult(new OrderReply
            {
                OrderDetails = orderDetails
            });
        }

        public override Task<OrderReply> UpdateOrder(OrderRequest request, ServerCallContext context)
        {
            _logger.LogInformation($"RPC method: {context.Method} recieved from :{context.Peer}. Host: {context.Host} ");
            var orderDetails = _orderRepository.UpdateOrder(request.OrderDetails);

            // Connect to RabbitMQ and push the message to exchange
            using var model = _rabbitMqConnection.CreateModel();
            model.QueueDeclare("OrderUpdateQueue", durable: true, exclusive: false, autoDelete: false);
            model.ExchangeDeclare("TopicExchange", ExchangeType.Topic, durable: true, autoDelete: false);
            model.QueueBind("OrderUpdateQueue", "TopicExchange", "OrderUpdated");

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(orderDetails));
            model.BasicPublish("TopicExchange", "OrderUpdated", null, body);

            model.Close();
            // Send reply to GRPC Client
            return Task.FromResult(new OrderReply
            {
                OrderDetails = orderDetails
            });
        }
    }
}