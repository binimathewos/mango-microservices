using AutoMapper;
using Mango.Service.ProductApi.Data;
using Mango.Service.ProductApi.Models;
using Mango.Service.ProductApi.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Service.ProductApi.Controllers;

[ApiController]
[Route("api/product")]
public class ProductController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private ResponseDto _response;

    public ProductController(AppDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _response = new ResponseDto();
    }

    [HttpGet]
    public ResponseDto Get()
    {
        try
        {
            var products = _dbContext.Products.ToList();
            _response.Result = _mapper.Map<IEnumerable<ProductDto>>(products);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.DisplayMessage = $"An error occured while retrieving products: {ex.Message}";
        }

        return _response;
    }

    [HttpGet]
    [Route("{id:int}")]
    public ResponseDto Get(int id)
    {
        try
        {
            var product = _dbContext.Products.FirstOrDefault(p => p.ProductId == id);
            _response.Result = product;
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.DisplayMessage = $"An error occured while retrieving product: {ex.Message}";
        }

        return _response;
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public ResponseDto Post(ProductDto productDto)
    {
        try
        {
            var product = _mapper.Map<Product>(productDto);
            _dbContext.Products.Add(product);
            _dbContext.SaveChanges();

            if (productDto.Image != null)
            {
                string fileName = $"{product.ProductId}{Path.GetExtension(productDto.Image.FileName)}";
                string filePath = @"wwwroot/ProductImages/" + fileName;
                string filePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), filePath);

                using (var stream = new FileStream(filePathDirectory, FileMode.Create))
                {
                    productDto.Image.CopyTo(stream);
                }

                string baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
                product.ImageUrl = $"{baseUrl}/ProductImages/{fileName}";
                product.ImageLocalPath = filePath;
            }
            else
            {
                product.ImageUrl = "https://placeholder.co/600x400";
            }

            _dbContext.Update(product);
            _dbContext.SaveChanges();

            _response.Result = _mapper.Map<ProductDto>(product);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.DisplayMessage = $"An error occured while creating product: {ex.Message}";
        }

        return _response;
    }

    [HttpPut]
    [Authorize(Roles = "ADMIN")]
    public ResponseDto Put(ProductDto productDto)
    {
        try
        {
            var product = _mapper.Map<Product>(productDto);

            if (productDto.Image != null)
            {
                if (!string.IsNullOrEmpty(product.ImageLocalPath))
                {
                    string oldfilePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), product.ImageLocalPath);
                    FileInfo oldfile = new FileInfo(oldfilePathDirectory);
                    if (oldfile.Exists)
                    {
                        oldfile.Delete();
                    }
                }

                string fileName = $"{product.ProductId}{Path.GetExtension(productDto.Image.FileName)}";
                string filePath = @"wwwroot/ProductImages/" + fileName;
                string filePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), filePath);

                using (var stream = new FileStream(filePathDirectory, FileMode.Create))
                {
                    productDto.Image.CopyTo(stream);
                }

                string baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
                product.ImageUrl = $"{baseUrl}/ProductImages/{fileName}";
                product.ImageLocalPath = filePath;
            }
            else
            {
                product.ImageUrl = "https://placeholder.co/600x400";
            }

            _dbContext.Products.Update(product);
            _dbContext.SaveChanges();

            _response.Result = _mapper.Map<ProductDto>(product);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.DisplayMessage = $"An error occured while updating product: {ex.Message}";
        }

        return _response;
    }

    [HttpDelete]
    [Route("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public ResponseDto Delete(int id)
    {
        try
        {
            var product = _dbContext.Products.FirstOrDefault(p => p.ProductId == id);
            if (product == null)
            {
                _response.IsSuccess = false;
                _response.DisplayMessage = "Product not found";
                return _response;
            }

            if (!string.IsNullOrEmpty(product.ImageLocalPath))
            {
                string filePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), product.ImageLocalPath);
                FileInfo file = new FileInfo(filePathDirectory);
                if (file.Exists)
                {
                    file.Delete();
                }
            }

            _dbContext.Products.Remove(product);
            _dbContext.SaveChanges();
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.DisplayMessage = $"An error occured while deleting product: {ex.Message}";
        }

        return _response;
    }
}

