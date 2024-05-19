using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnet_rpg.Repositories.UserRepository
{
    public interface IUserRepository
    {
        Task<User?> QueryExistingUser(string username);
        Task<User> AddUser(User user);
    }
}