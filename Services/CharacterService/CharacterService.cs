using AutoMapper;
using dotnet_rpg.Data;
using dotnet_rpg.Dtos.Character;
using dotnet_rpg.Repositories.CharacterRepository;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32.SafeHandles;

namespace dotnet_rpg.Services.CharacterService
{
    public class CharacterService : ICharacterService
    {
        private readonly IMapper _mapper;
        private readonly DataContext _dataContext;
        private readonly ICharacterRepository _characterRepo;

        public CharacterService(IMapper mapper, DataContext dataContext, ICharacterRepository characterRepo)
        {
            _mapper = mapper;
            _dataContext = dataContext;
            _characterRepo = characterRepo;
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


        public async Task<ServiceResponse<List<GetCharacterDto>>> 
            AddCharacter(AddCharacterDto newCharacter)
        {
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
            await _characterRepo.AddCharacter(newCharacter);

            // turn Character type into GetCharacterDto type
            serviceResponse.Data = await _dataContext.Characters
                .Select(c => _mapper.Map<GetCharacterDto>(c)).ToListAsync();
            return serviceResponse;
        }


        public async Task<ServiceResponse<GetCharacterDto>> 
            UpdateCharacter(UpdateCharacterDto updateDto)
        {
            var serviceResponse = new ServiceResponse<GetCharacterDto>();
            
            var updatedCharacter = await _characterRepo.UpdateCharacter(updateDto);

            // check if updated successfully
            if (updatedCharacter is null) {
                serviceResponse.Data = null;
                serviceResponse.Success = false;
                serviceResponse.Message 
                    = $"Character with Id '{updateDto.Id}' not found.";

                return serviceResponse;
            }

            // has updated successfully!
            // turn Character type into GetCharacterDto type
            serviceResponse.Data = _mapper.Map<GetCharacterDto>(updatedCharacter);
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
    }
}
