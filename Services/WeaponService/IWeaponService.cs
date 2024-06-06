using dotnet_rpg.Dtos.Character;
using dotnet_rpg.Dtos.Weapon;

namespace dotnet_rpg.Services.WeaponService
{
    public interface IWeaponService
    {
        Task<ServiceResponse<List<GetWeaponAndCharacterDto>>> GetYourWeapons();
        Task<ServiceResponse<GetCharacterDto>> AddWeapon(AddWeaponDto addDto);
        Task<ServiceResponse<GetCharacterDto>> UpdateWeapon(UpdateWeaponDto updateDto);
        Task<ServiceResponse<GetCharacterDto>> DeleteWeapon(int characterId);
    }
}
