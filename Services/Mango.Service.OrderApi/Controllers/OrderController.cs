using AutoMapper;
using Mango.MessageBus;
using Mango.Service.OrderApi.Data;
using Mango.Service.OrderApi.Models;
using Mango.Service.OrderApi.Models.Dto;
using Mango.Service.OrderApi.Sevice.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mango.Service.OrderApi.Utility;
using Stripe;
using Stripe.Checkout;

namespace Mango.Service.OrderApi.Controllers;

[ApiController]
[Route("api/order")]
public class OrderController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private ResponseDto _response;
    private readonly IMapper _mapper;
    private readonly IProductService _productService;
    private readonly IMessageBus _messageBus;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OrderController> _logger;

    public OrderController(AppDbContext dbContext, IMapper mapper,
        IProductService productService, IMessageBus messageBus, IConfiguration confguration,
        ILogger<OrderController> logger)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _productService = productService;
        _messageBus = messageBus;
        _configuration = confguration;
        _logger = logger;

        _response = new ResponseDto();
    }

    [Authorize]
    [HttpGet("get-orders/{userId?}")]

    public async Task<ResponseDto> GetOrder(string? userId = "")
    {
        try
        {
            IEnumerable<OrderHeader> orderHeaders;
            if (User.IsInRole(SD.RoleAdmin))
            {
                orderHeaders = _dbContext.OrderHeaders.Include(o => o.OrderDetails)
                    .OrderByDescending(o => o.OrderHeaderId).ToList();
            }
            else
            {
                orderHeaders = _dbContext.OrderHeaders.Include(o => o.OrderDetails)
                    .Where(o => o.UserId == userId)
                    .OrderByDescending(o => o.OrderHeaderId).ToList();
            }

            _response.Result = _mapper.Map<IEnumerable<OrderHeaderDto>>(orderHeaders);

        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "OrderApi request failed");
            _response.IsSuccess = false;
            _response.DisplayMessage = GetAllExceptionMessages(ex);
        }

        return _response;
    }

    [Authorize]
    [HttpGet("get-order/{orderId:int}")]
    public async Task<ResponseDto> GetOrder(int orderId)
    {
        try
        {
            OrderHeader? orderHeader = _dbContext.OrderHeaders.Include(o => o.OrderDetails).FirstOrDefault(o => o.OrderHeaderId == orderId);
            if (orderHeader == null)
            {
                throw new NotFoundException($"Order {orderId} was not found.");
            }

            Console.WriteLine("Order Details: " + orderHeader?.OrderDetails.Count());

            _response.Result = _mapper.Map<OrderHeaderDto>(orderHeader);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "OrderApi request failed");
            _response.IsSuccess = false;
            _response.DisplayMessage = GetAllExceptionMessages(ex);
        }

        return _response;
    }

    [Authorize]
    [HttpPost("create-order")]
    public async Task<ResponseDto> CreateOrder([FromBody] CartDto cartDto)
    {
        try
        {
            OrderHeaderDto orderHeaderDto = _mapper.Map<OrderHeaderDto>(cartDto.CartHeader);
            orderHeaderDto.OrderDate = DateTime.UtcNow;
            orderHeaderDto.OrderStatus = SD.Status_Pending;

            orderHeaderDto.OrderDetails = _mapper.Map<IEnumerable<OrderDetailsDto>>(cartDto.CartDetails);

            OrderHeader order = _dbContext.OrderHeaders.Add(_mapper.Map<OrderHeader>(orderHeaderDto)).Entity;
            await _dbContext.SaveChangesAsync();

            orderHeaderDto.OrderHeaderId = order.OrderHeaderId;

            _response.Result = orderHeaderDto;

            _logger.LogInformation(
                "OrderCreated OrderId={OrderId} UserId={UserId} Total={Total} Items={Items}",
                order.OrderHeaderId, orderHeaderDto.UserId, orderHeaderDto.OrderTotal, orderHeaderDto.OrderDetails?.Count());
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "OrderApi request failed");
            _response.IsSuccess = false;
            _response.DisplayMessage = GetAllExceptionMessages(ex);
        }

        return _response;
    }

    [Authorize]
    [HttpPost("stripe-session")]
    public async Task<ResponseDto> CreateStripeSession([FromBody] StripeRequestDto stripeRequestDto)
    {
        try
        {
            var options = new Stripe.Checkout.SessionCreateOptions
            {
                SuccessUrl = stripeRequestDto.ApprovedUrl,
                CancelUrl = stripeRequestDto.CancelUrl,
                LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
                Mode = "payment",
            };

            if (stripeRequestDto.OrderHeader.Discount > 0)
            {
                options.Discounts = new List<SessionDiscountOptions>
                {
                    new SessionDiscountOptions
                    {
                        Coupon = stripeRequestDto.OrderHeader.CouponCode
                    }
                };
            }

            foreach (var orderItem in stripeRequestDto.OrderHeader.OrderDetails)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(orderItem.Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = orderItem.Product.Name
                        }
                    },
                    Quantity = orderItem.Count
                };
                options.LineItems.Add(sessionLineItem);
            }

            var client = new StripeClient(SD.StripeApiKey);

            var sessionService = new SessionService(client);
            Stripe.Checkout.Session session = sessionService.Create(options);

            stripeRequestDto.StripeSessionUrl = session.Url;
            stripeRequestDto.StripeSessionId = session.Id;

            OrderHeader orderHeader = _dbContext.OrderHeaders.FirstOrDefault(o => o.OrderHeaderId == stripeRequestDto.OrderHeader.OrderHeaderId);
            orderHeader.StripeSessionId = session.Id;
            await _dbContext.SaveChangesAsync();

            _response.Result = stripeRequestDto;

            _logger.LogInformation(
                "StripeSessionCreated OrderId={OrderId} SessionId={SessionId}",
                stripeRequestDto.OrderHeader.OrderHeaderId, session.Id);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "OrderApi request failed");
            _response.IsSuccess = false;
            _response.DisplayMessage = GetAllExceptionMessages(ex);
        }

        return _response;
    }


    [Authorize]
    [HttpPost("stripe-validate")]
    public async Task<ResponseDto> ValidateStripeSession([FromBody] int orderHeaderId)
    {
        try
        {
            OrderHeader orderHeader = _dbContext.OrderHeaders.FirstOrDefault(o => o.OrderHeaderId == orderHeaderId);

            var client = new StripeClient(SD.StripeApiKey);

            var sessionService = new SessionService(client);
            Stripe.Checkout.Session session = sessionService.Get(orderHeader.StripeSessionId);

            var paymentIntentService = new PaymentIntentService(client);
            PaymentIntent paymentIntent = paymentIntentService.Get(session.PaymentIntentId);

            if (paymentIntent.Status == "succeeded")
            {
                orderHeader.PaymentIntentId = paymentIntent.Id;
                orderHeader.OrderStatus = SD.Status_Approved;
                await _dbContext.SaveChangesAsync();

                RewardsDto rewardsDto = new RewardsDto
                {
                    OrderId = orderHeader.OrderHeaderId,
                    RewardsActivity = Convert.ToInt32(orderHeader.OrderTotal),
                    UserId = orderHeader.UserId
                };

                string topicName = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic");
                await _messageBus.PublishMessage(rewardsDto, topicName);

                _logger.LogInformation(
                    "PaymentSucceeded OrderId={OrderId} Total={Total}",
                    orderHeader.OrderHeaderId, orderHeader.OrderTotal);
            }
            else
            {
                _logger.LogWarning(
                    "PaymentNotCompleted OrderId={OrderId} Status={Status}",
                    orderHeader.OrderHeaderId, paymentIntent.Status);
            }

            _response.Result = _mapper.Map<OrderHeaderDto>(orderHeader);

        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "OrderApi request failed");
            _response.IsSuccess = false;
            _response.DisplayMessage = GetAllExceptionMessages(ex);
        }

        return _response;
    }

    [Authorize]
    [HttpPost("update-order/{orderId:int}")]
    public async Task<ResponseDto> UpdateOrder(int orderId, [FromBody] string orderStatus)
    {
        try
        {
            OrderHeader? orderHeader = _dbContext.OrderHeaders.FirstOrDefault(o => o.OrderHeaderId == orderId);
            if (orderHeader == null)
            {
                throw new NotFoundException($"Order {orderId} was not found.");
            }

            if (orderStatus == SD.Status_Cancelled)
            {
                //Refund
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };

                var client = new StripeClient(SD.StripeApiKey);
                var refundService = new RefundService(client);

                Refund refund = refundService.Create(options);
            }

            orderHeader.OrderStatus = orderStatus;
            await _dbContext.SaveChangesAsync();

            _response.Result = true;

        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "OrderApi request failed");
            _response.IsSuccess = false;
            _response.DisplayMessage = GetAllExceptionMessages(ex);
        }

        return _response;
    }

    private string GetAllExceptionMessages(Exception ex)
    {
        var messages = new List<string>();

        while (ex != null)
        {
            messages.Add(ex.Message);
            ex = ex.InnerException;
        }

        return string.Join(" --> ", messages);
    }
}