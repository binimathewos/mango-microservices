using Microsoft.AspNetCore.Mvc;
using Mango.Web.Models;
using Mango.Web.Service;
using Mango.Web.Service.IService;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;

namespace Mango.Web.Controllers;


public class CartController : Controller
{
    private readonly ICartService _cartService;
    private readonly IOrderService _orderService;

    public CartController(ICartService cartService, IOrderService orderService)
    {
        _cartService = cartService;
        _orderService = orderService;
    }

    [Authorize]
    public async Task<IActionResult> CartIndex()
    {
        return View(await LoadLoggedInUserCart());
    }

    [Authorize]
    [HttpGet("CartCheckout")]
    public async Task<IActionResult> CartCheckout()
    {
        return View(await LoadLoggedInUserCart());
    }

    [Authorize]
    [HttpPost("CartCheckout")]
    public async Task<IActionResult> CartCheckout(CartDto cartDto)
    {
        try
        {
            CartDto dbCart = await LoadLoggedInUserCart();
            dbCart.CartHeader?.Name = cartDto.CartHeader?.Name;
            dbCart.CartHeader?.Email = cartDto.CartHeader?.Email;
            dbCart.CartHeader?.Phone = cartDto.CartHeader?.Phone;

            ResponseDto? responseDto = await _orderService.CreateOrderAsync(dbCart);
            if (responseDto != null && responseDto.IsSuccess)
            {
                TempData["success"] = "Order Created Successfully!";
                OrderHeaderDto? orderHeaderDto = JsonConvert.DeserializeObject<OrderHeaderDto>(Convert.ToString(responseDto.Result)!);
                string domain = $"{Request.Scheme}://{Request.Host.Value}";
                StripeRequestDto stripeRequestDto = new StripeRequestDto
                {
                    ApprovedUrl = $"{domain}/cart/orderconfirmation?orderId={orderHeaderDto?.OrderHeaderId}",
                    CancelUrl = $"{domain}/cart/cartcheckout",
                    OrderHeader = orderHeaderDto
                };

                ResponseDto? stripeResponseDto = await _orderService.CreateStripeSessionAsync(stripeRequestDto);
                if (stripeResponseDto == null || !stripeResponseDto.IsSuccess)
                {
                    TempData["error"] = stripeResponseDto?.DisplayMessage
                        ?? "Unable to create Stripe session. Verify the Order API's Stripe:SecretKey is a real Stripe test key.";
                    return View(await LoadLoggedInUserCart());
                }

                StripeRequestDto? stripeResponse = JsonConvert.DeserializeObject<StripeRequestDto>(Convert.ToString(stripeResponseDto.Result)!);
                if (stripeResponse == null || string.IsNullOrEmpty(stripeResponse.StripeSessionUrl))
                {
                    TempData["error"] = "Stripe session was created but no redirect URL was returned.";
                    return View(await LoadLoggedInUserCart());
                }

                Response.Headers.Add("Location", stripeResponse.StripeSessionUrl);

                return new StatusCodeResult(303);
            }
        }
        catch (System.Exception ex)
        {
            TempData["error"] = ex.Message;
        }

        return View(await LoadLoggedInUserCart());
    }

    [Authorize]
    public async Task<IActionResult> OrderConfirmation(int orderId)
    {
        ResponseDto? response = await _orderService.ValidateStripeSessionAsync(orderId!);

        if (response != null && response.IsSuccess)
        {
            OrderHeaderDto? orderHeader = JsonConvert.DeserializeObject<OrderHeaderDto>(Convert.ToString(response.Result));
            if (orderHeader != null && orderHeader.OrderStatus == SD.Status_Approved)
            {
                TempData["success"] = "Payment Completed Successfully!";
                return View(orderId);
            }
        }
        else
        {
            TempData["error"] = response?.DisplayMessage;
            Console.WriteLine("Error:" + response?.DisplayMessage);
        }

        return View(orderId);
    }

    [Authorize]
    public async Task<IActionResult> Remove(int cartDetailsId)
    {
        ResponseDto? response = await _cartService.RemmoveFromCartAsync(cartDetailsId!);
        if (response != null && response.IsSuccess)
        {
            return RedirectToAction(nameof(CartIndex));
        }

        return View();
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> ApplyCoupon(CartDto cartDto)
    {
        ResponseDto? response = await _cartService.ApplyCouponAsync(cartDto);
        if (response != null && response.IsSuccess)
        {
            return RedirectToAction(nameof(CartIndex));
        }

        return View();
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> EmailCart(CartDto cartDto)
    {
        string? email = User.Claims.Where(c => c.Type == JwtRegisteredClaimNames.Email)?.FirstOrDefault()?.Value;

        CartDto dbCart = await LoadLoggedInUserCart();
        dbCart.CartHeader?.Email = email;

        ResponseDto? response = await _cartService.EmailCartAsync(dbCart);
        if (response != null && response.IsSuccess)
        {
            TempData["success"] = "You cart request email has been sent.";
            return RedirectToAction(nameof(CartIndex));
        }
        else
        {
            TempData["error"] = response?.DisplayMessage;
        }

        return View();
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> RemoveCoupon(CartDto cartDto)
    {
        cartDto?.CartHeader?.CouponCode = "";
        ResponseDto? response = await _cartService.ApplyCouponAsync(cartDto);
        if (response != null && response.IsSuccess)
        {
            return RedirectToAction(nameof(CartIndex));
        }

        return View();
    }

    private async Task<CartDto> LoadLoggedInUserCart()
    {
        string? userId = User.Claims.Where(c => c.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;
        ResponseDto? response = await _cartService.GetCartByUserIdAsync(userId!);

        CartDto? cartDto = null;
        if (response != null && response.IsSuccess)
        {
            cartDto = JsonConvert.DeserializeObject<CartDto>(Convert.ToString(response.Result)!);
        }

        return cartDto ?? new CartDto();
    }
}
