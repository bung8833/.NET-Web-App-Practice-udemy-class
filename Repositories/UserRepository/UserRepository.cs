using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotnet_rpg.Data;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rpg.Repositories.UserRepository
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _dataContext;

        public UserRepository(DataContext dataContext)
        {
            _dataContext = dataContext;
        }


        /// <summary>
        /// Check if a user with the given username already exists.
        /// </summary>
        /// <param name="username"></param>
        /// <returns>
        /// A User entity that represents the existing user with the given username.
        /// If no such user, returns null.
        /// </returns>
        public async Task<User?> QueryExistingUser(string username)
        {
            return await _dataContext.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
        }


        /// <summary>
        /// Add the User entity to the database.
        /// </summary>
        /// <param name="user"></param>
        /// <returns>The user in the database</returns>
        public async Task<User> AddUser(User user)
        {
            var entry = _dataContext.Users.Add(user);
            await _dataContext.SaveChangesAsync();

            return entry.Entity;
        }
    }
}