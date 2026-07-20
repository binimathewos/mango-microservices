using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mango.Service.CouponApi.Data;
using Mango.Service.CouponApi.Models.Dto;
using Mango.Service.CouponApi.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Mango.Service.CouponApi.Utility;

namespace Mango.Service.CouponApi.Controllers;

[Route("api/coupon")]
[ApiController]
[Authorize]
public class CouponApiController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private ResponseDto _response;
    private readonly IMapper _mapper;

    public CouponApiController(AppDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _response = new ResponseDto();
    }


    [HttpGet]
    public ResponseDto Get()
    {
        try
        {
            var coupons = _dbContext.Coupons.ToList();
            _response.Result = _mapper.Map<IEnumerable<CouponDto>>(coupons);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.DisplayMessage = $"An error occurred while retrieving coupons: {ex.Message}";
        }

        return _response;
    }


    [HttpGet]
    [Route("{id:int}")]
    public ResponseDto Get(int id)
    {
        try
        {
            var coupon = _dbContext.Coupons.FirstOrDefault(c => c.CouponId == id);
            _response.Result = _mapper.Map<CouponDto>(coupon);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.DisplayMessage = $"An error occurred while retrieving coupon: {ex.Message}";
        }

        return _response;
    }

    [HttpGet]
    [Route("GetByCode/{code}")]
    public ResponseDto GetByCode(string code)
    {
        try
        {
            var coupon = _dbContext.Coupons.FirstOrDefault(c => c.CouponCode.ToLower() == code.ToLower());
            _response.Result = _mapper.Map<CouponDto>(coupon);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.DisplayMessage = $"An error occurred while retrieving coupon: {ex.Message}";
        }

        return _response;
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public ResponseDto Post([FromBody] CouponDto couponDto)
    {
        try
        {
            var coupon = _mapper.Map<Coupon>(couponDto);
            _dbContext.Coupons.Add(coupon);
            _dbContext.SaveChanges();

            var options = new Stripe.CouponCreateOptions
            {
                Id = coupon.CouponCode,
                AmountOff = (long)(coupon.DiscountAmount * 100),
                Currency = "usd"
            };

            var client = new Stripe.StripeClient(SD.StripeApiKey);
            var service = client.V1.Coupons;

            service.Create(options);

            _response.Result = _mapper.Map<CouponDto>(coupon);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.DisplayMessage = $"An error occurred while creating coupon: {ex.Message}";
        }

        return _response;
    }


    [HttpPut]
    [Authorize(Roles = "ADMIN")]
    public ResponseDto Put([FromBody] CouponDto couponDto)
    {
        try
        {
            var coupon = _mapper.Map<Coupon>(couponDto);
            _dbContext.Coupons.Update(coupon);
            _dbContext.SaveChanges();
            _response.Result = _mapper.Map<CouponDto>(coupon);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.DisplayMessage = $"An error occurred while updating coupon: {ex.Message}";
        }

        return _response;
    }


    [HttpDelete]
    [Route("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public ResponseDto Delete(int id)
    {
        try
        {
            var coupon = _dbContext.Coupons.FirstOrDefault(c => c.CouponId == id);
            if (coupon == null)
            {
                _response.IsSuccess = false;
                _response.DisplayMessage = "Coupon not found";
                return _response;
            }

            _dbContext.Coupons.Remove(coupon);
            _dbContext.SaveChanges();

            var client = new Stripe.StripeClient(SD.StripeApiKey);
            var service = client.V1.Coupons;

            service.Delete(coupon.CouponCode);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.DisplayMessage = $"An error occurred while deleting coupon: {ex.Message}";
        }

        return _response;
    }
}

