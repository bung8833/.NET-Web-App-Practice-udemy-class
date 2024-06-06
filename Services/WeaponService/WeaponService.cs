using AutoMapper;
using dotnet_rpg.Data;
using dotnet_rpg.Dtos.Character;
using dotnet_rpg.Dtos.Weapon;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Security.Claims;

namespace dotnet_rpg.Services.WeaponService
{
    public class WeaponService : IWeaponService
    {
        private readonly DataContext _dataContext;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WeaponService(DataContext dataContext, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _dataContext = dataContext;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        private int GetCurrentUserId() => int.Parse(_httpContextAccessor.HttpContext!.User
            .FindFirstValue(ClaimTypes.NameIdentifier)!);


        public async Task<ServiceResponse<List<GetWeaponAndCharacterDto>>> GetYourWeapons()
        {
            var response = new ServiceResponse<List<GetWeaponAndCharacterDto>>();

            var weapons = _dataContext.Weapons
                .Include(w => w.Character).ThenInclude(c => c!.User)
                .Where(w => w.Character!.User!.Id == GetCurrentUserId());
            
            response.Data = await weapons.Select(w => new GetWeaponAndCharacterDto() {
                Name = w.Name,
                Damage = w.Damage,
                CharacterId = w.Character.Id,
                Character = w.Character.Name
            }).ToListAsync();
            
            return response;
        }


        public async Task<ServiceResponse<GetCharacterDto>> AddWeapon(AddWeaponDto addDto)
        {
            var response = new ServiceResponse<GetCharacterDto>();

            try
            {
                // Check if the character exists && the user is logged in
                var character = await _dataContext.Characters
                    .Include(c => c.Weapon)
                    .Include(c => c.Skills)
                    .FirstOrDefaultAsync(c => c.Id == addDto.CharacterId
                                         && c.User != null && c.User.Id == GetCurrentUserId());

                if (character is null)
                {
                    response.Success = false;
                    response.Message = "Character not found.";
                    return response;
                }

                // Check if the Character already has a Weapon
                if (character.Weapon != null)
                {
                    response.Success = false;
                    response.Message = $"Character already has the weapon '{character.Weapon.Name}'.";
                    return response;
                }

                // Create the weapon entity
                var weapon = new Weapon
                {
                    Name = addDto.Name,
                    Damage = addDto.Damage,
                    Character = character
                };

                _dataContext.Weapons.Add(weapon);
                await _dataContext.SaveChangesAsync();

                response.Data = _mapper.Map<GetCharacterDto>(character);
                response.Message = $"Successfully added Weapon '{weapon.Name}' to the character.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }


        public async Task<ServiceResponse<GetCharacterDto>> UpdateWeapon(UpdateWeaponDto updateDto)
        {
            var response = new ServiceResponse<GetCharacterDto>();

            try
            {
                // Check if the character exists && the user is logged in
                var character = await _dataContext.Characters
                    .Include(c => c.Weapon)
                    .Include(c => c.Skills)
                    .FirstOrDefaultAsync(c => c.Id == updateDto.CharacterId
                                         && c.User != null && c.User.Id == GetCurrentUserId());

                if (character is null)
                {
                    response.Success = false;
                    response.Message = $"Character with Id '{updateDto.CharacterId}'  not found.";
                    return response;
                }

                // Check if the Character has a Weapon
                if (character.Weapon is null)
                {
                    response.Success = false;
                    response.Message = "Character does not have a weapon. Please create one first.";
                    return response;
                }

                // Update the weapon entity
                character.Weapon.Damage = updateDto.Damage;

                _dataContext.Weapons.Update(character.Weapon);
                await _dataContext.SaveChangesAsync();

                response.Data = _mapper.Map<GetCharacterDto>(character);
                response.Message = $"Successfully updated weapon '{character.Weapon.Name}' of the character.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }


        public async Task<ServiceResponse<GetCharacterDto>> DeleteWeapon(int characterId)
        {
            var response = new ServiceResponse<GetCharacterDto>();

            try
            {
                // Check if character exists && the user is logged in
                var character = await _dataContext.Characters
                    .Include(c => c.Weapon)
                    .Include(c => c.Skills)
                    .FirstOrDefaultAsync(c => c.Id == characterId
                                         && c.User != null && c.User.Id == GetCurrentUserId());
                if (character is null)
                {
                    response.Success = false;
                    response.Message = $"Character with Id '{characterId}' not found.";
                    return response;
                }

                // Check if the Character has a Weapon
                var weapon = character.Weapon;
                if (weapon is null)
                {
                    response.Success = false;
                    response.Message = "Character does not have a weapon.";
                    return response;
                }

                // Delete the weapon
                _dataContext.Weapons.Remove(weapon);
                await _dataContext.SaveChangesAsync();

                response.Data = _mapper.Map<GetCharacterDto>(character);
                response.Message = $"Successfully deleted weapon '{weapon.Name}' from the character.";
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
