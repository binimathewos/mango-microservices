using AutoMapper;
using Mango.Service.CouponApi.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Mango.Service.CouponApi.Models.Dto;

namespace Mango.Service.CouponApi;

public class MappingConfig
{
    public static MapperConfiguration RegisterMaps()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Coupon, CouponDto>();
            cfg.CreateMap<CouponDto, Coupon>();
        }, NullLoggerFactory.Instance);

        return config;
    }
}