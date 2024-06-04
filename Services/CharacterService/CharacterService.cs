using System.Security.Claims;
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
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICharacterRepository _characterRepo;

        public CharacterService(IMapper mapper, DataContext dataContext, IHttpContextAccessor httpContextAccessor, ICharacterRepository characterRepo)
        {
            _mapper = mapper;
            _dataContext = dataContext;
            _httpContextAccessor = httpContextAccessor;
            _characterRepo = characterRepo;
        }


        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext!.User
            .FindFirstValue(ClaimTypes.NameIdentifier)!);


        /// <summary>
        /// Get all characters that belong to the user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<ServiceResponse<List<GetCharacterDto>>> GetYourCharacters()
        {
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
            var dbCharacters = await _characterRepo.GetCharactersByUserId(GetUserId());

            // turn Character type into GetCharacterDto type
            serviceResponse.Data = dbCharacters
                .Select(c => _mapper.Map<GetCharacterDto>(c)).ToList();
            return serviceResponse;
        }


        public async Task<ServiceResponse<GetCharacterDto>> 
            GetYourCharacterById(int id)
        {
            var serviceResponse = new ServiceResponse<GetCharacterDto>();
            var dbCharacter = await _dataContext.Characters
                .FirstOrDefaultAsync(c => c.Id == id && c.User != null && c.User.Id == GetUserId());

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
            GetYourCharactersByName(string name)
        {
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
            var dbCharacters = await _dataContext.Characters
                .Where(c => c.Name.ToLower().Contains(name.ToLower()) 
                            && c.User != null && c.User.Id == GetUserId())
                .ToListAsync();

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
            AddCharacter(AddCharacterDto addDto)
        {
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
            // Set the user of the new character to current user
            var newCharacter = _mapper.Map<Character>(addDto);
            newCharacter.User = await _dataContext.Users.FirstOrDefaultAsync(u => u.Id == GetUserId());
            
            await _characterRepo.AddCharacter(newCharacter);

            // return all characters that belong to the user
            serviceResponse.Data = await _dataContext.Characters
                .Where(c => c.User != null && c.User.Id == GetUserId())
                .Select(c => _mapper.Map<GetCharacterDto>(c)).ToListAsync();
            return serviceResponse;
        }


        public async Task<ServiceResponse<GetCharacterDto>> 
            UpdateYourCharacter(UpdateCharacterDto updateDto)
        {
            var serviceResponse = new ServiceResponse<GetCharacterDto>();
            
            var character = await _dataContext.Characters
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == updateDto.Id /*&& c.User != null && c.User.Id == GetUserId()*/);

            // check if updated successfully
            if (character is null || character.User!.Id != GetUserId()) {
                serviceResponse.Data = null;
                serviceResponse.Success = false;
                serviceResponse.Message 
                    = $"Character with Id '{updateDto.Id}' not found.";
                return serviceResponse;
            }

            // update character properties
            await _characterRepo.UpdateCharacter(updateDto);

            // turn Character type into GetCharacterDto type
            serviceResponse.Data = _mapper.Map<GetCharacterDto>(character);
            serviceResponse.Message = $"Successfully updated your Character.";

            return serviceResponse;
        }


        public async Task<ServiceResponse<List<GetCharacterDto>>> 
            DeleteYourCharacter(int id)
        {
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
            var character = await _dataContext.Characters
                .FirstOrDefaultAsync(c => c.Id == id && c.User != null && c.User.Id == GetUserId());

            if (character is null){
                // character not found or does not belong to user
                serviceResponse.Success = false;
                serviceResponse.Message = $"Character with Id '{id}' not found.";
                return serviceResponse;
            }

            await _characterRepo.DeleteCharacter(id);

            serviceResponse.Data = await _dataContext.Characters
                .Where(c => c.User != null && c.User.Id == GetUserId())
                .Select(c => _mapper.Map<GetCharacterDto>(c)).ToListAsync();
            serviceResponse.Message = $"Successfully deleted Character with Id '{id}'.";

            return serviceResponse;
        }
    }
}
