namespace dotnet_rpg.Services
{
    public interface IAuthService
    {
        Task<ServiceResponse<int>> Register(User user, string password);
        Task<ServiceResponse<string>> Login(string username, string password);
        Task<User?> QueryExistingUser(string username);
    }
}
