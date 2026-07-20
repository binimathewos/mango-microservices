using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Mango.Web.Models;
using Mango.Web.Service.IService;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;

namespace Mango.Web.Controllers;

public class HomeController : Controller
{
    private readonly IProductService _productService;
    private readonly ICartService _cartService;

    public HomeController(IProductService productService, ICartService cartService)
    {
        _productService = productService;
        _cartService = cartService;
    }

    public async Task<IActionResult> Index()
    {
        var products = new List<ProductDto>();
        var response = await _productService.GetAllProductsAsync();

        if (response != null && response.IsSuccess)
        {
            products = JsonConvert.DeserializeObject<List<ProductDto>>(
                Convert.ToString(response.Result)!);
        }

        return View(products);
    }

    [Authorize]
    public async Task<IActionResult> ProductDetails(int productId)
    {
        var response = await _productService.GetProductAsync(productId);

        if (response != null && response.IsSuccess)
        {
            var productDto = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(response.Result)!);
            return View(productDto);
        }

        return NotFound();
    }

    [HttpPost]
    [Authorize]
    [ActionName("ProductDetails")]
    public async Task<IActionResult> ProductDetails(ProductDto productDto)
    {
        string? userId = User.Claims.Where(c => c.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;

        CartDto cartDto = new CartDto
        {
            CartHeader = new CartHeaderDto
            {
                UserId = userId
            }
        };

        CartDetailsDto cartDetailsDto = new CartDetailsDto
        {
            Count = productDto.Count,
            ProductId = productDto.ProductId
        };

        List<CartDetailsDto> cartDetailsDtos = new() { cartDetailsDto };
        cartDto.CartDetails = cartDetailsDtos;

        var response = await _cartService.UpsertCartAsync(cartDto);

        if (response != null && response.IsSuccess)
        {
            TempData["success"] = "Item has been added to the shopping cart";
            return RedirectToAction(nameof(Index));
        }
        else
        {
            TempData["Error"] = response?.DisplayMessage;
        }

        return View(productDto);
    }
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
