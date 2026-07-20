
using Mango.Service.AuthApi.Models;

namespace Mango.Service.AuthApi.Service.IServices;

public interface IJwtTokenGenerator
{
    string GenerateToken(ApplicationUser user, IEnumerable<string> roles);
}
