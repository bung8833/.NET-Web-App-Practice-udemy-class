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
        public async Task<ActionResult<ServiceResponse<int>>> 
            Register(UserRegisterDto request, string password)
        {
            var response = await _authService.Register(request, password);
            // check if the response is successful, 
            // i.e., if no other user with the same username exists
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }


        [HttpPost]
        [Route("Login")]
        public async Task<ActionResult<ServiceResponse<int>>> 
            Login(UserLoginDto request)
        {
            var response = await _authService.Login(request.Username, request.Password);
            // check if the response is successful, 
            // i.e., if username & password are correct
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}
