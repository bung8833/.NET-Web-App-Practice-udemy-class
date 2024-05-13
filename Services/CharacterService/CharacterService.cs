using AutoMapper;
using dotnet_rpg.Data;
using dotnet_rpg.Dtos.Character;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32.SafeHandles;

namespace dotnet_rpg.Services.CharacterService
{
    public class CharacterService : ICharacterService
    {
        private readonly IMapper _mapper;
        private readonly DataContext _dataContext;

        public CharacterService(IMapper mapper, DataContext dataContext)
        {
            _dataContext = dataContext;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> 
            AddCharacter(AddCharacterDto newCharacter)
        {
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();

            // turn AddCharacterDto type into Character type
            var character = _mapper.Map<Character>(newCharacter);

            _dataContext.Characters.Add(character);
            // save the changes
            await _dataContext.SaveChangesAsync();

            // turn Character type into GetCharacterDto type
            serviceResponse.Data = await _dataContext.Characters
                .Select(c => _mapper.Map<GetCharacterDto>(c)).ToListAsync();
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> GetAllCharacters()
        {
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
            var dbCharacters = await _dataContext.Characters.ToListAsync();

            // turn Character type into GetCharacterDto type
            serviceResponse.Data = dbCharacters
                .Select(c => _mapper.Map<GetCharacterDto>(c)).ToList();
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> 
            GetCharacterById(int id)
        {
            var serviceResponse = new ServiceResponse<GetCharacterDto>();
            var dbCharacter = await _dataContext.Characters.FirstOrDefaultAsync(c => c.Id == id);

            // check if dbCharacter is null
            if (dbCharacter is null) {
                serviceResponse.Data = null;
                serviceResponse.Success = false;
                serviceResponse.Message = $"Character with Id '{id}' not found.";

                return serviceResponse;
            }
            
            // turn Character type into GetCharacterDto type
            serviceResponse.Data = _mapper.Map<GetCharacterDto>(dbCharacter);
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> 
            UpdateCharacter(UpdateCharacterDto updatedCharacter)
        {
            var serviceResponse = new ServiceResponse<GetCharacterDto>();
            var dbCharacter = await _dataContext.Characters
                .FirstOrDefaultAsync(c => c.Id == updatedCharacter.Id);

            // check if dbCharacter is null
            if (dbCharacter is null) {
                serviceResponse.Data = null;
                serviceResponse.Success = false;
                serviceResponse.Message 
                    = $"Character with Id '{updatedCharacter.Id}' not found.";

                return serviceResponse;
            }

            // dbCharacter not null, update it
            dbCharacter.Name = updatedCharacter.Name;
            dbCharacter.HitPoints = updatedCharacter.HitPoints;
            dbCharacter.Strength = updatedCharacter.Strength;
            dbCharacter.Defense = updatedCharacter.Defense;
            dbCharacter.Intelligent = updatedCharacter.Intelligent;
            dbCharacter.Class = updatedCharacter.Class;

            // save the changes
            await _dataContext.SaveChangesAsync();

            // turn Character type into GetCharacterDto type
            serviceResponse.Data = _mapper.Map<GetCharacterDto>(dbCharacter);
            serviceResponse.Message = $"Successfully updated your Character.";

            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> 
            DeleteCharacter(int id)
        {
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
            var dbCharacter = await _dataContext.Characters.FirstOrDefaultAsync(c => c.Id == id);

            // check if dbCharacter is null
            if (dbCharacter is null) {
                serviceResponse.Data = await _dataContext.Characters
                    .Select(c => _mapper.Map<GetCharacterDto>(c)).ToListAsync();
                serviceResponse.Success = false;
                serviceResponse.Message = $"Character with Id '{id}' not found.";

                return serviceResponse;
            }

            // dbCharacter not null, delete it
            _dataContext.Characters.Remove(dbCharacter);

            // save the changes
            await _dataContext.SaveChangesAsync();

            serviceResponse.Data = await _dataContext.Characters
                .Select(c => _mapper.Map<GetCharacterDto>(c)).ToListAsync();
            serviceResponse.Message = $"Successfully deleted Character with Id '{id}'.";

            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> 
            GetCharactersByName(string name)
        {
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
            var dbCharacters = await _dataContext.Characters
                .Where(c => c.Name.ToLower().Contains(name.ToLower())).ToListAsync();

            // check if dbCharacters is empty
            if (dbCharacters.Count == 0) {
                serviceResponse.Data = null;
                serviceResponse.Success = false;
                serviceResponse.Message = $"No characters found with name '{name}'.";

                return serviceResponse;
            }

            // turn Character type into GetCharacterDto type
            serviceResponse.Data = dbCharacters
                .Select(c => _mapper.Map<GetCharacterDto>(c)).ToList();
            
            return serviceResponse;
        }
        
        

        
    }
}
