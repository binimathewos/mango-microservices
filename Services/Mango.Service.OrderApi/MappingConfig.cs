using AutoMapper;
using Mango.Service.OrderApi.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Mango.Service.OrderApi.Models.Dto;

namespace Mango.Service.OrderApi;

public class MappingConfig
{
    public static MapperConfiguration RegisterMaps()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<CartHeaderDto, OrderHeaderDto>()
                .ForMember(dest => dest.OrderTotal, m => m.MapFrom(src => src.CartTotal)).ReverseMap();

            cfg.CreateMap<CartDetailsDto, OrderDetailsDto>()
                .ForMember(dest => dest.ProductName, m => m.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.Price, m => m.MapFrom(src => src.Product.Price));

            cfg.CreateMap<OrderDetailsDto, CartDetailsDto>();

            cfg.CreateMap<OrderHeader, OrderHeaderDto>().ReverseMap();
            cfg.CreateMap<OrderDetails, OrderDetailsDto>().ReverseMap();
        }, NullLoggerFactory.Instance);

        return config;
    }
}