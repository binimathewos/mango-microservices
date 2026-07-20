using Microsoft.AspNetCore.Mvc;
using Mango.Web.Models;
using Mango.Web.Service;
using Mango.Web.Service.IService;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace Mango.Web.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _authService;
    private readonly ITokenProvider _tokenProvider;

    public AuthController(IAuthService authService, ITokenProvider tokenProvider)
    {
        _authService = authService;
        _tokenProvider = tokenProvider;
    }

    public IActionResult Register()
    {
        var roleList = new List<SelectListItem>()
        {
            new SelectListItem {Text = SD.RoleAdmin, Value = SD.RoleAdmin },
            new SelectListItem {Text = SD.RoleCustomer, Value = SD.RoleCustomer },
        };

        ViewBag.RoleList = roleList;

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegistrationRequestDto registrationRequestDto)
    {
        if (ModelState.IsValid)
        {
            var response = await _authService.RegisterAsync(registrationRequestDto);
            if (response != null && response.IsSuccess)
            {
                AssignRoleRequestDto assignRoleRequest = new AssignRoleRequestDto()
                {
                    Email = registrationRequestDto.Email,
                    RoleName = registrationRequestDto.Role ?? SD.RoleCustomer
                };

                var assignRoleRespons = await _authService.AssignRoleAsync(assignRoleRequest);
                if (assignRoleRespons != null && assignRoleRespons.IsSuccess)
                {
                    TempData["success"] = "User registerd successfully!";
                    return RedirectToAction(nameof(Login));
                }
            }
            else
            {
                TempData["error"] = response?.DisplayMessage;
            }
        }

        return RedirectToAction(nameof(Register));
    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginRequestDto loginRequestDto)
    {
        if (ModelState.IsValid)
        {
            var response = await _authService.LoginAsync(loginRequestDto);
            if (response != null && response.IsSuccess)
            {
                LoginResponseDto loginResponse = JsonConvert.DeserializeObject<LoginResponseDto>(Convert.ToString(response.Result));
                await SignInUser(loginResponse);

                _tokenProvider.SetToken(loginResponse.Token);

                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
        }

        return View(loginRequestDto);
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();

        _tokenProvider.CleanToken();
        return RedirectToAction("Index", "Home");
    }

    private async Task SignInUser(LoginResponseDto loginResponseDto)
    {
        var handler = new JwtSecurityTokenHandler();

        var jwt = handler.ReadJwtToken(loginResponseDto.Token);

        var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);

        identity.AddClaim(new Claim(JwtRegisteredClaimNames.Email,
            jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email).Value));
        identity.AddClaim(new Claim(JwtRegisteredClaimNames.Sub,
            jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub).Value));
        identity.AddClaim(new Claim(JwtRegisteredClaimNames.Name,
            jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Name).Value));

        identity.AddClaim(new Claim(ClaimTypes.Name,
            jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email).Value));

        identity.AddClaim(new Claim(ClaimTypes.Role,
            jwt.Claims.FirstOrDefault(c => c.Type == "role").Value));

        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    }
}