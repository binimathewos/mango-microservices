using Mango.Service.ShoppingCartApi.Models.Dto;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Mango.Service.ShoppingCartApi.Sevice.IService;
using Newtonsoft.Json;

namespace Mango.Service.ShoppingCartApi.Sevice;

public class CouponService : ICouponService
{
    private readonly IHttpClientFactory _httpClientFactory;
    public CouponService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<CouponDto> GetCouponAsyc(string couponCode)
    {
        CouponDto? couponDto = null;

        HttpClient client = _httpClientFactory.CreateClient("Coupon");

        var response = await client.GetAsync("/api/coupon/GetByCode/" + couponCode);
        var apiContent = await response.Content.ReadAsStringAsync();
        var responseDto = JsonConvert.DeserializeObject<ResponseDto>(apiContent);
        if (responseDto != null && responseDto.IsSuccess)
        {
            couponDto = JsonConvert.DeserializeObject<CouponDto>(Convert.ToString(responseDto.Result));
        }

        return couponDto ?? new CouponDto();
    }
}