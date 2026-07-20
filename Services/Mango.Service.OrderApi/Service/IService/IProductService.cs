using Mango.Service.OrderApi.Models.Dto;

namespace Mango.Service.OrderApi.Sevice.IService;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetProductsAsyc();
}