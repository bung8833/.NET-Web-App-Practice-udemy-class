using System.Security.Claims;
using AutoMapper;
using dotnet_rpg.Data;
using dotnet_rpg.Dtos.Character;
using dotnet_rpg.Repositories.CharacterRepository;
using dotnetrpg.Migrations;
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

        private int GetCurrentUserId() => int.Parse(_httpContextAccessor.HttpContext!.User
            .FindFirstValue(ClaimTypes.NameIdentifier)!);


        /// <summary>
        /// Get all characters that belong to the user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<ServiceResponse<List<GetCharacterDto>>> GetYourCharacters()
        {
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
            var dbCharacters = await _characterRepo.GetCharactersByUserId(GetCurrentUserId());

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
                .Include(c => c.Weapon)
                .Include(c => c.Skills)
                .FirstOrDefaultAsync(c => c.Id == id && c.User != null && c.User.Id == GetCurrentUserId());

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
                            && c.User != null && c.User.Id == GetCurrentUserId())
                .Include(c => c.Weapon)
                .Include(c => c.Skills)
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
            newCharacter.User = await _dataContext.Users.FirstOrDefaultAsync(u => u.Id == GetCurrentUserId());
            
            await _characterRepo.AddCharacter(newCharacter);

            // return all characters that belong to the user
            serviceResponse.Data = await _dataContext.Characters
                .Where(c => c.User != null && c.User.Id == GetCurrentUserId())
                .Include(c => c.Weapon)
                .Include(c => c.Skills)
                .Select(c => _mapper.Map<GetCharacterDto>(c)).ToListAsync();
            return serviceResponse;
        }


        public async Task<ServiceResponse<GetCharacterDto>> 
            UpdateYourCharacter(UpdateCharacterDto updateDto)
        {
            var serviceResponse = new ServiceResponse<GetCharacterDto>();
            
            var character = await _dataContext.Characters
                // 之後沒有context，所以要include必要資訊
                .Include(c => c.User)
                .Include(c => c.Weapon)
                .Include(c => c.Skills)
                .FirstOrDefaultAsync(c => c.Id == updateDto.Id);

            // Check if the character exists && the user is logged in
            if (character is null || character.User is null || character.User.Id != GetCurrentUserId()) {
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
                .FirstOrDefaultAsync(c => c.Id == id && c.User != null && c.User.Id == GetCurrentUserId());

            if (character is null){
                // character not found or does not belong to user
                serviceResponse.Success = false;
                serviceResponse.Message = $"Character with Id '{id}' not found.";
                return serviceResponse;
            }

            await _characterRepo.DeleteCharacter(id);

            serviceResponse.Data = await _dataContext.Characters
                .Where(c => c.User != null && c.User.Id == GetCurrentUserId())
                .Include(c => c.Weapon)
                .Include(c => c.Skills)
                .Select(c => _mapper.Map<GetCharacterDto>(c)).ToListAsync();
            serviceResponse.Message = $"Successfully deleted Character with Id '{id}'.";

            return serviceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> AddCharaterSkill(AddCharacterSkillDto addSkillDto)
        {
            var response = new ServiceResponse<GetCharacterDto>();

            try
            {
                // Check if the character exists && user is logged in
                var character = await _dataContext.Characters
                    .Include(c => c.Weapon)
                    .Include(c => c.Skills)
                    .FirstOrDefaultAsync(c => c.Id == addSkillDto.CharacterId
                                         && c.User != null && c.User.Id == GetCurrentUserId());
                if (character is null)
                {
                    response.Success = false;
                    response.Message = $"Character with Id '{addSkillDto.CharacterId}' not found.";
                    return response;
                }

                // Check if the skill is valid
                var skill = await _dataContext.Skills
                    .FirstOrDefaultAsync(s => s.Id == addSkillDto.SkillId);
                if (skill is null)
                {
                    response.Success = false;
                    response.Message = $"Skill with Id '{addSkillDto.SkillId}' not found.";
                    return response;
                }

                // Check if the character already has the skill
                if (character.Skills!.Any(s => s.Id == addSkillDto.SkillId))
                {
                    response.Success = false;
                    response.Message = $"Character already has the skill '{skill.Name}'.";
                    return response;
                }

                // Add the skill to the character
                character.Skills!.Add(skill);
                await _dataContext.SaveChangesAsync();
                response.Data = _mapper.Map<GetCharacterDto>(character);
                response.Message = $"Successfully added skill '{skill.Name}' to Character.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }
    }
}
