
using Microsoft.AspNetCore.Identity;

namespace Mango.Service.AuthApi.Models;

public class ApplicationUser : IdentityUser
{
    public string Name { get; set; }
}