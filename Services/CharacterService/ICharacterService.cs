using dotnet_rpg.Dtos.Character;

namespace dotnet_rpg.Services.CharacterService
{
    public interface ICharacterService
    {
        Task<ServiceResponse<List<GetCharacterDto>>> GetYourCharacters();

        Task<ServiceResponse<GetCharacterDto>> GetYourCharacterById(int id);

        Task<ServiceResponse<List<GetCharacterDto>>> 
            GetYourCharactersByName(string name);

        Task<ServiceResponse<List<GetCharacterDto>>> 
            AddCharacter(AddCharacterDto newCharacter);
        
        Task<ServiceResponse<GetCharacterDto>> 
            UpdateYourCharacter(UpdateCharacterDto updateDto);
        
        Task<ServiceResponse<List<GetCharacterDto>>> 
            DeleteYourCharacter(int id);
        
        Task<ServiceResponse<GetCharacterDto>> 
            AddCharaterSkill(AddCharacterSkillDto addSkillDto);
    }
}
