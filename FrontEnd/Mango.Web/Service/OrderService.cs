
using Mango.Web.Models;
using Mango.Web.Service.IService;
using System.Threading.Tasks;

namespace Mango.Web.Service
{
    public class OrderService : IOrderService
    {
        private readonly IBaseService _baseService;

        public OrderService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseDto?> CreateOrderAsync(CartDto cartDto)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.POST,
                Url = SD.OrderAPIBase + "/api/order/create-order",
                Data = cartDto
            });
        }

        public async Task<ResponseDto?> CreateStripeSessionAsync(StripeRequestDto stripeRequestDto)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.POST,
                Url = SD.OrderAPIBase + "/api/order/stripe-session",
                Data = stripeRequestDto
            });
        }

        public async Task<ResponseDto?> GetAllOrdersAsync(string? userId)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.GET,
                Url = SD.OrderAPIBase + "/api/order/get-orders/" + userId,
            });
        }

        public async Task<ResponseDto?> GetOrderAsync(int orderId)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.GET,
                Url = SD.OrderAPIBase + "/api/order/get-order/" + orderId,
            });
        }

        public async Task<ResponseDto?> UpdateOrderAsync(int orderId, string orderStatus)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.POST,
                Url = SD.OrderAPIBase + "/api/order/update-order/" + orderId,
                Data = orderStatus
            });
        }

        public async Task<ResponseDto?> ValidateStripeSessionAsync(int orderHeaderId)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.POST,
                Url = SD.OrderAPIBase + "/api/order/stripe-validate",
                Data = orderHeaderId
            });
        }
    }
}