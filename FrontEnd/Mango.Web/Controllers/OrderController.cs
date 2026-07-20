using Microsoft.AspNetCore.Mvc;
using Mango.Web.Models;
using Mango.Web.Service;
using Mango.Web.Service.IService;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;

namespace Mango.Web.Controllers;


public class OrderController : Controller
{
    private readonly IOrderService _orderService;
    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [Authorize]
    public async Task<IActionResult> OrderIndex()
    {
        return View();
    }

    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        IEnumerable<OrderHeaderDto> orders = new List<OrderHeaderDto>();
        string? userId = "";
        if (!User.IsInRole(SD.RoleAdmin))
        {
            userId = User.Claims.Where(c => c.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;
        }

        ResponseDto? responseDto = await _orderService.GetAllOrdersAsync(userId);
        if (responseDto != null && responseDto.IsSuccess)
        {
            orders = JsonConvert.DeserializeObject<IEnumerable<OrderHeaderDto>>(Convert.ToString(responseDto.Result)!)!;
        }

        return Json(new { data = orders });
    }

    [Authorize]
    public async Task<IActionResult> OrderDetails(int orderId)
    {
        OrderHeaderDto? orderHeaderDto = new OrderHeaderDto();
        string? userId = User.Claims.Where(c => c.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;

        ResponseDto? responseDto = await _orderService.GetOrderAsync(orderId);
        if (responseDto != null && responseDto.IsSuccess)
        {
            orderHeaderDto = JsonConvert.DeserializeObject<OrderHeaderDto>(Convert.ToString(responseDto.Result)!)!;
        }

        if (!User.IsInRole(SD.RoleAdmin) && userId != orderHeaderDto.UserId)
        {
            return NotFound();
        }

        return View(orderHeaderDto);
    }

    [Authorize]
    public async Task<IActionResult> OrderReadyForPickup(int orderId)
    {
        ResponseDto? responseDto = await _orderService.UpdateOrderAsync(orderId, SD.Status_ReadyForPickup);
        if (responseDto != null && responseDto.IsSuccess)
        {
            TempData["success"] = "Order Status updated successfully!";
            return RedirectToAction(nameof(OrderDetails), new { orderId = orderId });
        }

        return View();
    }

    [Authorize]
    public async Task<IActionResult> OrderComplete(int orderId)
    {
        ResponseDto? responseDto = await _orderService.UpdateOrderAsync(orderId, SD.Status_Completed);
        if (responseDto != null && responseDto.IsSuccess)
        {
            TempData["success"] = "Order Status updated successfully!";
            return RedirectToAction(nameof(OrderDetails), new { orderId = orderId });
        }

        return View();
    }

    [Authorize]
    public async Task<IActionResult> OrderCancel(int orderId)
    {
        ResponseDto? responseDto = await _orderService.UpdateOrderAsync(orderId, SD.Status_Cancelled);
        if (responseDto != null && responseDto.IsSuccess)
        {
            TempData["success"] = "Order Status updated successfully!";
            return RedirectToAction(nameof(OrderDetails), new { orderId = orderId });
        }

        return View();
    }
}
