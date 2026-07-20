using Mango.Service.ShoppingCartApi.Models.Dto;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Newtonsoft.Json;
using Mango.Service.ShoppingCartApi.Sevice.IService;

namespace Mango.Service.ShoppingCartApi.Sevice;

public class ProductService : IProductService
{
    private readonly IHttpClientFactory _httpClientFactory;
    public ProductService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IEnumerable<ProductDto>> GetProductsAsyc()
    {
        IEnumerable<ProductDto>? productDtoList = null;

        HttpClient client = _httpClientFactory.CreateClient("Product");

        var response = await client.GetAsync("/api/product");
        var apiContent = await response.Content.ReadAsStringAsync();
        var responseDto = JsonConvert.DeserializeObject<ResponseDto>(apiContent);
        if (responseDto != null && responseDto.IsSuccess)
        {
            productDtoList = JsonConvert.DeserializeObject<IEnumerable<ProductDto>>(Convert.ToString(responseDto.Result));
        }

        return productDtoList ?? [];
    }
}