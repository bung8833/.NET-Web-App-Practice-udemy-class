using dotnet_rpg.Data;
using dotnet_rpg.Dtos.User;
using dotnet_rpg.Services;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_rpg.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }


        [HttpPost]
        [Route("Register")]
        public async Task<ActionResult<ServiceResponse<int>>> Register(UserRegisterDto request)
        {
            // Create a new User entity, and set its properties
            User user = new()
            {
                Username = request.Username,
            };

            var response = await _authService.Register(user, request.Password);
            // check if the response is successful, 
            // i.e., if no other user with the same username exists
            if (response.Success == false)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}
