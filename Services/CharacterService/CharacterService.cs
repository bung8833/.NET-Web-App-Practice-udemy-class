using AutoMapper;
using dotnet_rpg.Dtos.Character;
using Microsoft.AspNetCore.Http.HttpResults;

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
            // turn AddCharacterDto type into Character type
            var character = _mapper.Map<Character>(newCharacter);
            // set a proper Id
            character.Id = _characters.Max(c => c.Id) + 1;
            _characters.Add(character);

            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();

            // turn Character type into GetCharacterDto type
            serviceResponse.Data = _characters.Select(c => _mapper.Map<GetCharacterDto>(c)).ToList();;
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> GetAllCharacters()
        {
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();

            // turn Character type into GetCharacterDto type
            serviceResponse.Data = _characters.Select(c => _mapper.Map<GetCharacterDto>(c)).ToList();
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> GetCharacterById(int id)
        {
            var character = _characters.FirstOrDefault(c => c.Id == id);
            
            var serviceResponse = new ServiceResponse<GetCharacterDto>();

            // turn Character type into GetCharacterDto type
            serviceResponse.Data = _mapper.Map<GetCharacterDto>(character);
            return serviceResponse;
        }
    }
}
