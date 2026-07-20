using Mango.Service.ShoppingCartApi.Models.Dto;

namespace Mango.Service.ShoppingCartApi.Sevice.IService;

public interface ICouponService
{
    Task<CouponDto> GetCouponAsyc(string couponCode);
}