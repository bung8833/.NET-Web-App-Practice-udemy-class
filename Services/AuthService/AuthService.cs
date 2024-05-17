
using dotnet_rpg.Data;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rpg.Services
{
    public class AuthService : IAuthService
    {
        private readonly DataContext _dataContext;

        public AuthService(DataContext dataContext)
        {
            _dataContext = dataContext;
        }


        public async Task<ServiceResponse<string>> Login(string username, string password)
        {
            ServiceResponse<string> response = new();
            // Check if a user with the same username already exists
            var dbUser = await QueryExistingUser(username);
            if (dbUser is null)
            {
                response.Success = false;
                response.Message = $"User '{username}' not found.";
            }
            // check if the password is correct
            else if (!VerifyPasswordHash(password, dbUser.PasswordHash, dbUser.PasswordSalt))
            {
                response.Success = false;
                response.Message = "Wrong password.";
            }
            else
            {
                response.Data = dbUser.Id.ToString();
                response.Message = $"{dbUser.Username} logged in successfully.";
            }

            return response;
        }


        public async Task<ServiceResponse<int>> Register(User user, string password)
        {
            ServiceResponse<int> response = new();
            // Check if a user with the same username already exists
            var existingUser = await QueryExistingUser(user.Username);
            if (existingUser is not null)
            {
                response.Success = false;
                response.Message = $"User with username '{existingUser.Username}' already exists." +
                                   $"/r/nPlease try again with another username.";
                return response;
            }

            CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            _dataContext.Users.Add(user);
            await _dataContext.SaveChangesAsync();
            // When adding the user to the database, 
            // the Id will be generated automatically
            response.Data = user.Id;
            response.Message = $"'{user.Username}' registered successfully with Id '{user.Id}'.";

            return response;
        }


        /// <summary>
        /// Check if a user with the given username already exists.
        /// </summary>
        /// <param name="username"></param>
        /// <returns>
        /// A User entity that represents the existing user with the given username;
        /// if no such user, returns null.
        /// </returns>
        public async Task<User?> QueryExistingUser(string username)
        {
            var dbUser = await _dataContext.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

            return dbUser;
        }


        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }


        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt) 
        {
            using(var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
    }
}
