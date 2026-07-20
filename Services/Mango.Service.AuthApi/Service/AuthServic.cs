
using Mango.Service.AuthApi.Models;
using Mango.Service.AuthApi.Models.Dto;
using System.Threading.Tasks;
using Mango.Service.AuthApi.Service.IServices;
using Microsoft.AspNetCore.Identity;
using Mango.Service.AuthApi.Data;

namespace Mango.Service.AuthApi.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(
        AppDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<string> RegisterAsync(RegistrationRequestDto registrationRequest)
    {
        ApplicationUser user = new ApplicationUser()
        {
            UserName = registrationRequest.Email,
            Email = registrationRequest.Email,
            NormalizedEmail = registrationRequest.Email.ToUpper(),
            Name = registrationRequest.Name,
            PhoneNumber = registrationRequest.PhoneNumber
        };

        try
        {
            var result = await _userManager.CreateAsync(user, registrationRequest.Password);
            if (result.Succeeded)
            {
                return string.Empty;
            }
            else
            {
                return result.Errors.Select(e => e.Description).FirstOrDefault() ?? "User registration failed.";
            }
        }
        catch (Exception ex)
        {
            return $"An error occurred while registering the user: {ex.Message}";
        }
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequest)
    {
        var user = _dbContext.Users.FirstOrDefault(u => u.UserName == loginRequest.UserName);
        if (user == null || !await _userManager.CheckPasswordAsync(user, loginRequest.Password))
        {
            return new LoginResponseDto
            {
                User = null,
                Token = string.Empty
            };
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtTokenGenerator.GenerateToken(user, roles);

        return new LoginResponseDto
        {
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber
            },
            Token = token
        };

    }

    public async Task<bool> AssignRolesAsync(string email, string roleName)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return false; // User not found
        }

        if (string.IsNullOrEmpty(roleName))
        {
            return false; // Role name is null or empty
        }

        if (!await _roleManager.RoleExistsAsync(roleName.ToLower() == "admin" ? "ADMIN" : "CUSTOMER"))
        {
            await _roleManager.CreateAsync(new IdentityRole(roleName));
        }

        var result = await _userManager.AddToRoleAsync(user, roleName);
        return result.Succeeded;
    }
}
