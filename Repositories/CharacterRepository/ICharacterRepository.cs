using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotnet_rpg.Dtos.Character;

namespace dotnet_rpg.Repositories.CharacterRepository
{
    public interface ICharacterRepository
    {
        Task<int> AddCharacter(AddCharacterDto newCharacter);
        Task<Character?> UpdateCharacter(UpdateCharacterDto updateDto);
    }
}