

namespace Mango.Service.ProductApi.Models.Dto;

public class ProductDto
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Price { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? ImageLocalPath { get; set; }
    public IFormFile? Image { get; set; }

}