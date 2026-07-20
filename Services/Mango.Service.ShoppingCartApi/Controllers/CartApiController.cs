using AutoMapper;
using Mango.MessageBus;
using Mango.Service.ShoppingCartApi.Data;
using Mango.Service.ShoppingCartApi.Models;
using Mango.Service.ShoppingCartApi.Models.Dto;
using Mango.Service.ShoppingCartApi.Sevice.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Mango.Service.ShoppingCartApi.Controllers;

[ApiController]
[Route("/api/cart")]
public class CartApiController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IProductService _productService;
    private readonly ICouponService _couponService;
    private readonly IMessageBus _messageBus;
    private readonly IConfiguration _configuration;
    private ResponseDto _response;

    public CartApiController(AppDbContext dbContext, IMapper mapper, IProductService productService,
        ICouponService couponService, IMessageBus messageBus, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _productService = productService;
        _couponService = couponService;
        _messageBus = messageBus;
        _configuration = configuration;

        _response = new ResponseDto();
    }

    [HttpPost("email-cart")]
    public async Task<ResponseDto> EmailCart([FromBody] CartDto cartDto)
    {
        try
        {
            string? topicAndQueueName = _configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCartQueue");
            await _messageBus.PublishMessage(cartDto, topicAndQueueName!);
            _response.Result = true;

        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.DisplayMessage = GetAllExceptionMessages(ex);
        }

        return _response;
    }

    [HttpPost("apply-coupon")]
    public async Task<ResponseDto> ApplyCoupone([FromBody] CartDto cartDto)
    {
        try
        {
            CartHeader dbCartHeader = await _dbContext.CartHeaders.FirstOrDefaultAsync(c => c.UserId == cartDto.CartHeader.UserId);
            dbCartHeader.CouponCode = cartDto.CartHeader.CouponCode;

            _dbContext.Update(dbCartHeader);
            await _dbContext.SaveChangesAsync();

            _response.Result = true;

        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.DisplayMessage = GetAllExceptionMessages(ex);
        }

        return _response;
    }

    [HttpPost("upsert")]
    public async Task<ResponseDto> CartUpsert([FromBody] CartDto cartDto)
    {
        try
        {
            CartHeader dbCartHeader = await _dbContext.CartHeaders.AsNoTracking().FirstOrDefaultAsync(c => c.UserId == cartDto.CartHeader.UserId);
            if (dbCartHeader == null)
            {
                CartHeader cartHeader = _mapper.Map<CartHeader>(cartDto.CartHeader);
                _dbContext.CartHeaders.Add(cartHeader);

                await _dbContext.SaveChangesAsync();

                cartDto.CartHeader?.CartHeaderId = cartHeader.CartHeaderId;
                cartDto.CartDetails?.First().CartHeaderId = cartHeader.CartHeaderId;

                _dbContext.CartDetails.Add(_mapper.Map<CartDetails>(cartDto.CartDetails?.First()));
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                var dbCartDetails = await _dbContext.CartDetails.AsNoTracking().FirstOrDefaultAsync(
                    c => c.ProductId == cartDto.CartDetails.First().ProductId && c.CartHeaderId == dbCartHeader.CartHeaderId);

                if (dbCartDetails == null)
                {
                    cartDto.CartDetails?.First().CartHeaderId = dbCartHeader.CartHeaderId;
                    _dbContext.CartDetails.Add(_mapper.Map<CartDetails>(cartDto.CartDetails?.First()));
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    cartDto.CartDetails?.First().Count += dbCartDetails.Count;
                    cartDto.CartDetails?.First().CartHeaderId = dbCartDetails.CartHeaderId;
                    cartDto.CartDetails?.First().CartDetailsId = dbCartDetails.CartDetailsId;

                    _dbContext.CartDetails.Update(_mapper.Map<CartDetails>(cartDto.CartDetails?.First()));
                    await _dbContext.SaveChangesAsync();
                }
            }

            _response.Result = cartDto;

        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.DisplayMessage = GetAllExceptionMessages(ex);
        }

        return _response;

    }

    [HttpPost("remove")]
    public async Task<ResponseDto> CartRemove([FromBody] int cartDetailsId)
    {
        try
        {
            CartDetails? cartDetails = _dbContext.CartDetails.FirstOrDefault(c => c.CartDetailsId == cartDetailsId);
            if (cartDetails != null)
            {
                _dbContext.Remove(cartDetails);

                int itemCount = _dbContext.CartDetails.Where(c => c.CartHeaderId == cartDetails.CartHeaderId).Count();
                if (itemCount == 1)
                {
                    CartHeader? cartHeader = _dbContext.CartHeaders.FirstOrDefault(c => c.CartHeaderId == cartDetails.CartHeaderId);
                    if (cartHeader != null) _dbContext.Remove(cartHeader);
                }

                await _dbContext.SaveChangesAsync();

                _response.Result = true;
            }
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.DisplayMessage = GetAllExceptionMessages(ex);
        }

        return _response;
    }

    [HttpGet("GetCart/{userId}")]
    public async Task<ResponseDto> GetCart(string userId)
    {
        try
        {
            CartDto cart = new CartDto
            {
                CartHeader = _mapper.Map<CartHeaderDto>(_dbContext.CartHeaders.FirstOrDefault(c => c.UserId == userId))
            };

            cart.CartDetails = _mapper.Map<IEnumerable<CartDetailsDto>>(_dbContext.CartDetails.Where(c => c.CartHeaderId == cart.CartHeader.CartHeaderId));

            IEnumerable<ProductDto> productList = await _productService.GetProductsAsyc();

            foreach (CartDetailsDto item in cart.CartDetails)
            {
                item.Product = productList.FirstOrDefault(p => p.ProductId == item.ProductId);
                cart.CartHeader.CartTotal += (item.Count * item.Product.Price);
            }

            if (!string.IsNullOrEmpty(cart.CartHeader.CouponCode))
            {
                CouponDto couponDto = await _couponService.GetCouponAsyc(cart.CartHeader.CouponCode);
                if (couponDto != null && cart.CartHeader.CartTotal > couponDto.MinimumAmount)
                {
                    cart.CartHeader.CartTotal -= couponDto.DiscountAmount;
                    cart.CartHeader.Discount = couponDto.DiscountAmount;
                }
            }

            _response.Result = cart;
        }
        catch (Exception ex)
        {
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