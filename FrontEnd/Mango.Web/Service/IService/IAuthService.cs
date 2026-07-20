using Mango.Web.Models;

namespace Mango.Web.Service.IService;

public interface IAuthService
{
    Task<ResponseDto?> RegisterAsync(RegistrationRequestDto registrationRequestDto);
    Task<ResponseDto?> LoginAsync(LoginRequestDto loginRequestDto);
    Task<ResponseDto?> AssignRoleAsync(AssignRoleRequestDto assignRoleRequestDto);
}