using AutoMapper;
using Mango.Service.ShoppingCartApi.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Mango.Service.ShoppingCartApi.Models.Dto;

namespace Mango.Service.ShoppingCartApi;

public class MappingConfig
{
    public static MapperConfiguration RegisterMaps()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<CartHeader, CartHeaderDto>().ReverseMap();
            cfg.CreateMap<CartDetails, CartDetailsDto>().ReverseMap();
        }, NullLoggerFactory.Instance);

        return config;
    }
}