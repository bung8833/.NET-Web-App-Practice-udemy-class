using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotnet_rpg.Data;
using dotnet_rpg.Dtos.Character;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rpg.Repositories.CharacterRepository
{
    public class CharacterRepository : ICharacterRepository
    {
        private readonly DataContext _dataContext;

        public CharacterRepository(DataContext dataContext)
        {
            _dataContext = dataContext;
        }


        public async Task<List<Character>> GetCharactersByUserId(int userId)
        {
            return await _dataContext.Characters
                .Include(c => c.Weapon)
                .Include(c => c.Skills)
                .Where(c => c.User != null && c.User.Id == userId).ToListAsync();
        }


        /// <summary>
        /// Add the specified character to the database.
        /// </summary>
        /// <param name="newCharacter"></param>
        /// <returns>The id of the new character in the database</returns>
        public async Task<int> AddCharacter(Character newCharacter)
        {
            var entry = _dataContext.Characters.Add(newCharacter);
            await _dataContext.SaveChangesAsync();
            return entry.Entity.Id;
        }


        /// <summary>
        /// Update the specified character in the database.
        /// </summary>
        /// <param name="updateDto"></param>
        /// <returns>
        /// The updated character in the database.
        /// Null if character not found.
        /// </returns>
        public async Task<Character?> UpdateCharacter(UpdateCharacterDto updateDto)
        {
            // check if character exists
            Character? dbCharacter = await _dataContext.Characters
                .FirstOrDefaultAsync(c => c.Id == updateDto.Id);
            if (dbCharacter is null) return null;

            // update the character using ChangeTracking
            _dataContext.Entry(dbCharacter).CurrentValues.SetValues(updateDto);
            
            await _dataContext.SaveChangesAsync();
            return dbCharacter;
        }


        /// <summary>
        /// Delete the character with the specified id from the database.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>true if character successfully deleted</returns>
        public async Task<bool> DeleteCharacter(int id)
        {
            var dbCharacter = await _dataContext.Characters.FirstOrDefaultAsync(c => c.Id == id);
            if (dbCharacter is null) return false;

            _dataContext.Characters.Remove(dbCharacter);
            await _dataContext.SaveChangesAsync();
            return true;
        }
    }
}