using OrderService.Protos;

namespace OrderService.Repositories
{
    public interface IOrderRepository
    {
        public OrderDetails PlaceOrder(OrderDetails orderDetails);
        public OrderDetails UpdateOrder(OrderDetails orderDetails);
    }
}
