
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mango.Service.AuthApi.Models;
using Mango.Service.AuthApi.Models.Dto;
using Mango.Service.AuthApi.Service.IServices;
using Microsoft.AspNetCore.Authorization;
using Mango.MessageBus;

namespace Mango.Service.AuthApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthApiController : ControllerBase
{
    private readonly ResponseDto _response;
    private readonly IAuthService _authService;
    private readonly IMessageBus _messageBus;
    private IConfiguration _configuration;

    public AuthApiController(IAuthService authService, IMessageBus messageBus, IConfiguration configuration)
    {
        _authService = authService;
        _messageBus = messageBus;
        _configuration = configuration;

        _response = new ResponseDto();
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegistrationRequestDto registrationRequest)
    {
        var errorMessage = await _authService.RegisterAsync(registrationRequest);
        if (!string.IsNullOrEmpty(errorMessage))
        {
            _response.IsSuccess = false;
            _response.DisplayMessage = errorMessage;
            return BadRequest(_response);
        }

        string topicAndQueueName = _configuration.GetValue<string>("TopicAndQueueNames:RegisterUserQueue")!;
        await _messageBus.PublishMessage(registrationRequest.Email!, topicAndQueueName);

        return Ok(_response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
    {
        var loginResponse = await _authService.LoginAsync(loginRequest);
        if (loginResponse.User == null)
        {
            _response.IsSuccess = false;
            _response.DisplayMessage = "Username or password is incorrect";
            return BadRequest(_response);
        }

        _response.Result = loginResponse;
        return Ok(_response);
    }

    [HttpPost("assign-role")]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequestDto assignRoleRequest)
    {
        var success = await _authService.AssignRolesAsync(assignRoleRequest.Email, assignRoleRequest.RoleName);
        if (!success)
        {
            _response.IsSuccess = false;
            _response.DisplayMessage = "Failed to assign role";
            return BadRequest(_response);
        }

        return Ok(_response);
    }
}