using Microsoft.AspNetCore.Mvc;
using Mango.Web.Models;
using Mango.Web.Service;
using Mango.Web.Service.IService;
using Newtonsoft.Json;

namespace Mango.Web.Controllers;

public class CouponController : Controller
{
    private readonly ICouponService _couponService;
    public CouponController(ICouponService couponService)
    {
        _couponService = couponService;
    }

    public async Task<IActionResult> CouponIndex()
    {
        var coupons = new List<CouponDto>();
        var response = await _couponService.GetAllCouponsAsync();

        if (response != null && response.IsSuccess)
        {
            coupons = JsonConvert.DeserializeObject<List<CouponDto>>(Convert.ToString(response.Result));
        }
        else
        {
            TempData["error"] = response?.DisplayMessage;
        }

        return View(coupons);
    }

    public async Task<IActionResult> CouponCreate()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CouponCreate(CouponDto couponDto)
    {
        if (ModelState.IsValid)
        {
            var response = await _couponService.CreateCouponAsync(couponDto);
            if (response != null && response.IsSuccess)
            {
                return RedirectToAction(nameof(CouponIndex));
            }
            else
            {
                TempData["error"] = response?.DisplayMessage;
            }
        }

        return View(couponDto);
    }

    public async Task<IActionResult> CouponDelete(int couponId)
    {
        var response = await _couponService.GetCouponAsync(couponId);

        if (response != null && response.IsSuccess)
        {
            var couponDto = JsonConvert.DeserializeObject<CouponDto>(Convert.ToString(response.Result));
            return View(couponDto);
        }
        else
        {
            TempData["error"] = response?.DisplayMessage;
        }

        return NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> CouponDelete(CouponDto couponDto)
    {
        var response = await _couponService.DeleteCouponAsync(couponDto.CouponId);
        if (response != null && response.IsSuccess)
        {
            return RedirectToAction(nameof(CouponIndex));
        }
        else
        {
            TempData["error"] = response?.DisplayMessage;
        }

        return View(couponDto);
    }
}
