using AutoMapper;
using dotnet_rpg.Data;
using dotnet_rpg.Dtos.Character;
using dotnet_rpg.Dtos.Weapon;
using Microsoft.EntityFrameworkCore;
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


        public async Task<ServiceResponse<GetCharacterDto>> AddWeapon(AddWeaponDto addDto)
        {
            var response = new ServiceResponse<GetCharacterDto>();

            try
            {
                // Check if the character exists && the user is logged in
                var character = await _dataContext.Characters
                    .FirstOrDefaultAsync(c => c.Id == addDto.CharacterId
                                         && c.User != null && c.User.Id == GetCurrentUserId());

                if (character is null)
                {
                    throw new Exception("Character not found.");
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
