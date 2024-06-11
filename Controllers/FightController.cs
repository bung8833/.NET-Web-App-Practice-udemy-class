using Azure.Core;
using dotnet_rpg.Dtos.Character;
using dotnet_rpg.Dtos.Fight;
using dotnet_rpg.Services.FightService;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_rpg.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FightController : ControllerBase
    {
        private readonly IFightService _fightService;

        public FightController(IFightService fightService)
        {
            _fightService = fightService;
        }


        [HttpPost]
        public async Task<ActionResult<ServiceResponse<FightResultDto>>> Fight(FightRequestDto request)
        {
            return Ok(await _fightService.Fight(request));
        }


        [HttpPost("DefaultFight")]
        public async Task<ActionResult<ServiceResponse<FightResultDto>>> DefaultFight()
        {
            return Ok(await _fightService.Fight(new FightRequestDto()));
        }


        [HttpPost("Simulate")]
        public async Task<ActionResult<ServiceResponse<FightResultDto>>> Fights(FightRequestDto request, int times)
        {
            if (times < 1)
            {
                return BadRequest("Times must be at least 1");
            }

            foreach (var time in Enumerable.Range(1, times - 1))
            {
                await _fightService.Fight(request);
            }

            return Ok(await _fightService.Fight(request));
        }


        [HttpPut("Clear")]
        public async Task<ActionResult<ServiceResponse<GetCharacterDto>>> ClearFightResults(List<int> characterIds)
        {
            return Ok(await _fightService.ClearFightResults(characterIds));
        }
    }
}
