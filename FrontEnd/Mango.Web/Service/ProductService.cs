using Mango.Web.Models;
using Mango.Web.Service.IService;
using System.Threading.Tasks;

namespace Mango.Web.Service;

public class ProductService : IProductService
{
    private readonly IBaseService _baseService;
    public ProductService(IBaseService baseService)
    {
        _baseService = baseService;
    }

    public async Task<ResponseDto?> CreateProductAsync(ProductDto productDto)
    {
        return await _baseService.SendAsync(new RequestDto()
        {
            ApiType = SD.ApiType.POST,
            Url = SD.ProductAPIBase + "/api/product",
            Data = productDto,
            ContentType = SD.ContentType.MultipartFormData
        });
    }

    public async Task<ResponseDto?> DeleteProductAsync(int productId)
    {
        return await _baseService.SendAsync(new RequestDto()
        {
            ApiType = SD.ApiType.DELETE,
            Url = SD.ProductAPIBase + "/api/product/" + productId
        });
    }

    public async Task<ResponseDto?> GetAllProductsAsync()
    {
        return await _baseService.SendAsync(new RequestDto()
        {
            ApiType = SD.ApiType.GET,
            Url = SD.ProductAPIBase + "/api/product"
        });
    }

    public async Task<ResponseDto?> GetProductAsync(int productId)
    {
        return await _baseService.SendAsync(new RequestDto()
        {
            ApiType = SD.ApiType.GET,
            Url = SD.ProductAPIBase + "/api/product/" + productId
        });
    }

    public async Task<ResponseDto?> UpdateProductAsync(ProductDto productDto)
    {
        return await _baseService.SendAsync(new RequestDto()
        {
            ApiType = SD.ApiType.PUT,
            Url = SD.ProductAPIBase + "/api/product",
            Data = productDto,
            ContentType = SD.ContentType.MultipartFormData
        });
    }
}