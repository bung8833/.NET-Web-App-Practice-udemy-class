using dotnet_rpg.Dtos.User;

namespace dotnet_rpg.Services
{
    public interface IAuthService
    {
        Task<ServiceResponse<int>> Register(UserRegisterDto userDto, string password);
        Task<ServiceResponse<string>> Login(string username, string password);
    }
}
