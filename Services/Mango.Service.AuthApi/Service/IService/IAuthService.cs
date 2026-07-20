
using Mango.Service.AuthApi.Models.Dto;
using System.Threading.Tasks;

namespace Mango.Service.AuthApi.Service.IServices;

public interface IAuthService
{
    Task<string> RegisterAsync(RegistrationRequestDto registrationRequest);
    Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequest);
    Task<bool> AssignRolesAsync(string email, string roleName);
}