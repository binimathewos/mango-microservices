using System.Runtime.CompilerServices;
using Mango.Web.Models;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace Mango.Web.Controllers;

public class ProductController : Controller
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<IActionResult> ProductIndex()
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

    public async Task<IActionResult> ProductCreate()
    {
        return View();
    }

    [HttpPost]
    public async Task<ActionResult> ProductCreate(ProductDto productDto)
    {
        if (ModelState.IsValid)
        {
            var response = await _productService.CreateProductAsync(productDto);
            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Product Added Successfully!";
                return RedirectToAction(nameof(ProductIndex));
            }
            else
            {
                TempData["Error"] = response?.DisplayMessage;
            }
        }

        return View(productDto);
    }

    public async Task<ActionResult> ProductDelete(int productId)
    {
        var response = await _productService.GetProductAsync(productId);
        if (response != null && response.IsSuccess)
        {
            var productDto = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(response.Result));
            return View(productDto);
        }

        return NotFound();

    }

    [HttpPost]
    public async Task<ActionResult> ProductDelete(ProductDto productDto)
    {
        var response = await _productService.DeleteProductAsync(productDto.ProductId);
        if (response != null && response.IsSuccess)
        {
            return RedirectToAction(nameof(ProductIndex));
        }
        else
        {
            TempData["Error"] = response?.DisplayMessage;
        }

        return View(productDto);
    }

    public async Task<IActionResult> ProductEdit(int productId)
    {
        var response = await _productService.GetProductAsync(productId);
        if (response != null && response.IsSuccess)
        {
            var productDto = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(response.Result));
            return View(productDto);
        }

        return NotFound();
    }

    [HttpPost]
    public async Task<ActionResult> ProductEdit(ProductDto productDto)
    {
        if (ModelState.IsValid)
        {
            var response = await _productService.UpdateProductAsync(productDto);
            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Product Updated Successfully!";
                return RedirectToAction(nameof(ProductIndex));
            }
            else
            {
                TempData["Error"] = response?.DisplayMessage;
            }
        }

        return View(productDto);
    }
}