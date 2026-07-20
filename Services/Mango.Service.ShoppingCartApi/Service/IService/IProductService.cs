using Mango.Service.ShoppingCartApi.Models.Dto;

namespace Mango.Service.ShoppingCartApi.Sevice.IService;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetProductsAsyc();
}