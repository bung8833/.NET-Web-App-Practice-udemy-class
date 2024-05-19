using dotnet_rpg.Dtos.Character;

namespace dotnet_rpg.Services.CharacterService
{
    public interface ICharacterService
    {
        Task<ServiceResponse<List<GetCharacterDto>>> GetAllCharacters();

        Task<ServiceResponse<GetCharacterDto>> GetCharacterById(int id);

        Task<ServiceResponse<List<GetCharacterDto>>> 
            GetCharactersByName(string name);

        Task<ServiceResponse<List<GetCharacterDto>>> 
            AddCharacter(AddCharacterDto newCharacter);
        
        Task<ServiceResponse<GetCharacterDto>> 
            UpdateCharacter(UpdateCharacterDto updateDto);
        
        Task<ServiceResponse<List<GetCharacterDto>>> 
            DeleteCharacter(int id);
    }
}
