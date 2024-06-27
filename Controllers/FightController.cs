using Azure;
using Azure.Core;
using dotnet_rpg.Data;
using dotnet_rpg.Dtos.Character;
using dotnet_rpg.Dtos.Fight;
using dotnet_rpg.Services.FightService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rpg.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FightController : ControllerBase
    {
        private readonly IFightService _fightService;
        private readonly DataContext _context;

        public FightController(IFightService fightService, DataContext dataContext)
        {
            _fightService = fightService;
            _context = dataContext;
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
        public async Task<ActionResult<ServiceResponse<List<string>>>> Fights(FightRequestDto request, int times)
        {
            var response = new ServiceResponse<List<string>>();

            if (times < 1)
            {
                return BadRequest("Times must be at least 1");
            }

            var characters = await _context.Characters
                    .Include(c => c.Weapon)
                    .Include(c => c.Skills)
                    .Where(c => request.GetCharacterIds().Contains(c.Id))
                    .ToListAsync();
            if (characters.Count < 2)
            {
                response.Success = false;
                response.Message = "Not enough characters to fight!";
                return response;
            }

            Dictionary<RpgClass, int> useWeaponRates = new Dictionary<RpgClass, int>
                {
                    { RpgClass.Knight, request.UseWeaponRateForKnights },
                    { RpgClass.Mage, request.UseWeaponRateForMages },
                    { RpgClass.Cleric, request.UseWeaponRateForClerics }
                };

            List<Fighter> fighters = characters.Select(c => new Fighter
            {
                Id = c.Id,
                Name = c.Name,
                HP = c.HP,
                MaxHP = c.HP,
                DamageReceived = 0,
                Healed = 0,
                SkillUsed = null,
                Strength = c.Strength,
                Defense = c.Defense,
                Intelligence = c.Intelligence,
                Class = c.Class,
                Weapon = c.Weapon,
                UseWeaponRate = useWeaponRates[c.Class],
                Skills = c.Skills,
                character = c,
                Fights = 0,
                Victories = 0,
                Defeats = 0,
            }).ToList();

            FightSettingsDto settings = new FightSettingsDto
            {
                criticalPunchRate = request.criticalPunchRate,
                criticalPunchDamage = request.criticalPunchDamage,
            };
            // this is just for showing the fight log, not real attack order
            fighters = fighters.OrderBy(c => c.Id).ToList();

            // Time
            var watch = System.Diagnostics.Stopwatch.StartNew();
            foreach (var time in Enumerable.Range(1, times))
            {
                _fightService.DoFight(ref fighters, settings);
            }
            watch.Stop();

            var elapsedMs = watch.ElapsedMilliseconds;
            decimal secs = Math.Round((decimal)elapsedMs / 1000, 1, MidpointRounding.AwayFromZero);
            decimal avg = Math.Round((decimal)elapsedMs / times, 2, MidpointRounding.AwayFromZero);
            response.Message += $"Fought {times} times in {secs} seconds"
                + $", {avg}ms per fight in average.";

            // show stats
            List<string> stats = new List<string>();
            int winRate = -1;
            int loseRate = -1;
            foreach (var fighter in fighters)
            {
                winRate = (int)Math.Round(100.0 * fighter.Victories / fighter.Fights, 0, MidpointRounding.AwayFromZero);
                loseRate = (int)Math.Round(100.0 * fighter.Defeats / fighter.Fights, 0, MidpointRounding.AwayFromZero);

                stats.Add($"{fighter.Name.PadRight(10)}: {winRate.ToString().PadLeft(2)}% win rate, {loseRate.ToString().PadLeft(2)}% lose rate");
            }   
            response.Data = stats;

            return Ok(response);
        }


        [HttpPut("Clear")]
        public async Task<ActionResult<ServiceResponse<GetCharacterDto>>> ClearFightResults(List<int> characterIds)
        {
            return Ok(await _fightService.ClearFightResults(characterIds));
        }
    }
}
