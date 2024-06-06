using dotnet_rpg.Dtos.Character;
using dotnet_rpg.Dtos.Weapon;
using dotnet_rpg.Services.WeaponService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_rpg.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class WeaponController : ControllerBase
    {
        private readonly IWeaponService _weaponService;

        public WeaponController(IWeaponService weaponService)
        {
            _weaponService = weaponService;
        }


        [HttpGet]
        public async Task<ActionResult<List<GetWeaponAndCharacterDto>>> GetWeapons() 
        {
            return Ok(await _weaponService.GetYourWeapons());
        }


        [HttpPost]
        public async Task<ActionResult<ServiceResponse<GetCharacterDto>>> 
            AddWeapon(AddWeaponDto addDto)
        {
            return Ok(await _weaponService.AddWeapon(addDto));
        }


        [HttpPut]
        public async Task<ActionResult<ServiceResponse<GetCharacterDto>>> 
            UpdateWeapon(UpdateWeaponDto updateDto)
        {
            return Ok(await _weaponService.UpdateWeapon(updateDto));
        }


        [HttpDelete]
        public async Task<ActionResult<ServiceResponse<GetCharacterDto>>> 
            DeleteWeapon(int characterId)
        {
            return Ok(await _weaponService.DeleteWeapon(characterId));
        }
    }
}
