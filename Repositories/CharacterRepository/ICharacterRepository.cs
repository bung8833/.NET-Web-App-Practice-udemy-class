using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotnet_rpg.Dtos.Character;

namespace dotnet_rpg.Repositories.CharacterRepository
{
    public interface ICharacterRepository
    {
        Task<List<Character>> GetCharactersByUserId(int userId);
        Task<int> AddCharacter(Character newCharacter);
        Task<Character?> UpdateCharacter(UpdateCharacterDto updateDto);
        Task<bool> DeleteCharacter(int id);
    }
}