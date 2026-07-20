
using System.ComponentModel.DataAnnotations;

namespace Mango.Service.ProductApi.Models;

public class Product
{
    [Required]
    public int ProductId { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty;
    [Range(1, 1000)]
    public double Price { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? ImageLocalPath { get; set; }
}