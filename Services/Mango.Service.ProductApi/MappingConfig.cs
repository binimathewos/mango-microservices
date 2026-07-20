using AutoMapper;
using Mango.Service.ProductApi.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Mango.Service.ProductApi.Models.Dto;

namespace Mango.Service.ProductApi;

public class MappingConfig
{
    public static MapperConfiguration RegisterMaps()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Product, ProductDto>();
            cfg.CreateMap<ProductDto, Product>();
        }, NullLoggerFactory.Instance);

        return config;
    }
}