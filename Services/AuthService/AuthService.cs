using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using dotnet_rpg.Data;
using dotnet_rpg.Dtos.User;
using dotnet_rpg.Repositories.UserRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace dotnet_rpg.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IConfiguration _config;

        public AuthService(IUserRepository userRepo, IConfiguration config)
        {
            _userRepo = userRepo;
            _config = config;
        }


        public async Task<ServiceResponse<string>> Login(string username, string password)
        {
            ServiceResponse<string> response = new();
            // Check if a user with the same username already exists
            var dbUser = await _userRepo.QueryExistingUser(username);
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
                response.Data = CreateToken(dbUser);
                response.Message = $"{dbUser.Username} logged in successfully.";
            }

            return response;
        }


        public async Task<ServiceResponse<int>> Register(UserRegisterDto userDto, string password)
        {
            ServiceResponse<int> response = new();

            // Check if username is valid
            if (userDto.Username.IsNullOrEmpty())
            {
                response.Success = false;
                response.Message = $"Username cannot be empty";
                return response;
            }
            // Check if password is valid
            if (password.IsNullOrEmpty())
            {
                response.Success = false;
                response.Message = $"Password cannot be empty";
                return response;
            }
            
            // Check if a user with the same username already exists
            var existingUser = await _userRepo.QueryExistingUser(userDto.Username);
            if (existingUser is not null)
            {
                response.Success = false;
                response.Message = $"User with username '{existingUser.Username}' already exists." +
                                   $" Please try again with another username.";
                return response;
            }

            User user = new()
            {
                Username = userDto.Username,
            };
            // Set user properties and add to database
            CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            var registeredUser = await _userRepo.AddUser(user);
            response.Data = registeredUser.Id;
            response.Message = $"{registeredUser.Username} registered successfully with Id '{registeredUser.Id}'.";

            return response;
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


        private string CreateToken(User user) 
        {
            var claims = new List<Claim> 
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };      
            
            var appsettingsToken = _config.GetSection("AppSettings:Token").Value;
            if (appsettingsToken is null) 
                throw new Exception("Appsettings Token is null!");
            
            SymmetricSecurityKey key = 
                new(System.Text.Encoding.UTF8.GetBytes(appsettingsToken));

            SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha512Signature);

            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            JwtSecurityTokenHandler tokenHandler = new();
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
        
    }
}
