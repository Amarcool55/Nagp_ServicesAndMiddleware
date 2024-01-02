using JsonDb;
using Microsoft.AspNetCore.Mvc;
using ProductService.Models;
using ProductService.Protos;

namespace ProductService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ILogger<ProductController> _logger;
        private readonly OrderProcessing.OrderProcessingClient _orderProcessingClient;
        private readonly IJsonDb db;

        public ProductController(ILogger<ProductController> logger, OrderProcessing.OrderProcessingClient orderProcessingClient, IJsonDbFactory jsonDbFactory)
        {
            _logger = logger;
            _orderProcessingClient = orderProcessingClient;
            db = jsonDbFactory.GetJsonDb();
        }

        [HttpPost("PlaceOrder")]
        public IActionResult PlaceOrder(int productId, string shippingAddress, int quantity)
        {
            var product = db.GetCollectionAsync<Product>("products").Result.SingleOrDefault(x => x.Id == productId);
            if (product != null)
            {
                var orderRequest = new OrderRequest
                {
                    OrderDetails = new OrderDetails
                    {
                        Color = product.color,
                        Description = product.Description,
                        OrderQuantity = quantity,
                        ProductName = product.Name,
                        ShipAddress = shippingAddress,
                        UnitPrice = product.Price
                    }
                };
                var reply = _orderProcessingClient.PlaceOrder(orderRequest);
                return Ok(new { order = reply.OrderDetails, Message = $"Product Order Placed with Id: {reply.OrderDetails.OrderId}" });
            }
            else
            {
                return BadRequest($"The Prodcut with ID:{productId} does not exist.");
            }
        }

        [HttpPost("UpdateOrder")]
        public IActionResult UpdateOrder(int orderId, int productId, string shippingAddress, int quantity)
        {
            var product = db.GetCollectionAsync<Product>("products").Result.SingleOrDefault(x => x.Id == productId);
            if (product != null)
            {
                var orderRequest = new OrderRequest
                {
                    OrderDetails = new OrderDetails
                    {
                        OrderId = orderId,
                        Color = product.color,
                        Description = product.Description,
                        OrderQuantity = quantity,
                        ProductName = product.Name,
                        ShipAddress = shippingAddress,
                        UnitPrice = product.Price
                    }
                };
                var reply = _orderProcessingClient.UpdateOrder(orderRequest);
                return Ok(new { order = reply.OrderDetails, Message = $"Order Updated" });
            }
            else
            {
                return BadRequest($"The Prodcut with ID:{productId} does not exist.");
            }
        }

        [HttpGet("GetAll")]
        public async Task<IEnumerable<Product>> GetProducts()
        {
            var products = await db.GetCollectionAsync<Product>("products");
            return products;
        }
    }
}