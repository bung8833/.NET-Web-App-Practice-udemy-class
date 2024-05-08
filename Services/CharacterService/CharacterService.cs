using AutoMapper;
using dotnet_rpg.Dtos.Character;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Win32.SafeHandles;

namespace dotnet_rpg.Services.CharacterService
{
    public class CharacterService : ICharacterService
    {
        private static List<Character> _characters = new List<Character>() {
            new Character(),
            new Character { Id = 1, Name = "Sam"},
        };

        private readonly IMapper _mapper;

        public CharacterService(IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> 
            AddCharacter(AddCharacterDto newCharacter)
        {
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();

            // turn AddCharacterDto type into Character type
            var character = _mapper.Map<Character>(newCharacter);
            // set a proper Id
            character.Id = _characters.Max(c => c.Id) + 1;
            _characters.Add(character);

            // turn Character type into GetCharacterDto type
            serviceResponse.Data = _characters.Select(c => _mapper.Map<GetCharacterDto>(c)).ToList();;
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> GetAllCharacters()
        {
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();

            // turn Character type into GetCharacterDto type
            serviceResponse.Data = _characters
                .Select(c => _mapper.Map<GetCharacterDto>(c)).ToList();
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> 
            GetCharacterById(int id)
        {
            var serviceResponse = new ServiceResponse<GetCharacterDto>();
            var character = _characters.FirstOrDefault(c => c.Id == id);

            // check if character is null
            if (character is null) {
                serviceResponse.Data = null;
                serviceResponse.Success = false;
                serviceResponse.Message = $"Character with Id '{id}' not found.";

                return serviceResponse;
            }
            
            // turn Character type into GetCharacterDto type
            serviceResponse.Data = _mapper.Map<GetCharacterDto>(character);
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> 
            UpdateCharacter(UpdateCharacterDto updatedCharacter)
        {
            var serviceResponse = new ServiceResponse<GetCharacterDto>();
            var character = _characters.FirstOrDefault(c => c.Id == updatedCharacter.Id);

            // check if character is null
            if (character is null) {
                serviceResponse.Data = null;
                serviceResponse.Success = false;
                serviceResponse.Message 
                    = $"Character with Id '{updatedCharacter.Id}' not found.";

                return serviceResponse;
            }

            // character not null, update it
            character.Name = updatedCharacter.Name;
            character.HitPoints = updatedCharacter.HitPoints;
            character.Strength = updatedCharacter.Strength;
            character.Defense = updatedCharacter.Defense;
            character.Intelligent = updatedCharacter.Intelligent;
            character.Class = updatedCharacter.Class;

            // turn Character type into GetCharacterDto type
            serviceResponse.Data = _mapper.Map<GetCharacterDto>(character);
            serviceResponse.Message = $"Successfully updated your character.";

            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> 
            DeleteCharacter(int id)
        {
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
            var character = _characters.FirstOrDefault(c => c.Id == id);

            // check if character is null
            if (character is null) {
                serviceResponse.Data = _characters
                    .Select(c => _mapper.Map<GetCharacterDto>(c)).ToList();
                serviceResponse.Success = false;
                serviceResponse.Message = $"Character with Id '{id}' not found.";

                return serviceResponse;
            }

            // character not null, delete it
            _characters.Remove(character);
            serviceResponse.Data = _characters
                .Select(c => _mapper.Map<GetCharacterDto>(c)).ToList();
            serviceResponse.Message = $"Successfully deleted character with Id '{id}'.";

            return serviceResponse;
        }
    }
}
