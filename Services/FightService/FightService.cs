using dotnet_rpg.Data;
using dotnet_rpg.Dtos.Fight;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace dotnet_rpg.Services.FightService
{
    public class FightService : IFightService
    {
        private readonly DataContext _context;

        public FightService(DataContext dataContext)
        {
            _context = dataContext;
        }


        public async Task<ServiceResponse<FightResultDto>> Fight(FightRequestDto request)
        {
            var response = new ServiceResponse<FightResultDto>
            {
                Data = new FightResultDto()
            };

            int criticalHitRate = request.criticalHitRate; // in percents
            int criticalHitDamage = request.criticalHitDamage;

            try
            {
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

                int round = 1;
                List<string> gameoverMessage = new List<string>();
                int namePadding = characters.Max(c => c.Name.Length);
                int weaponPadding = characters.Max(c => c.Weapon?.Name.Length ?? 5);

                // Characters take turns to attack, until someone is defeated
                bool defeated = false;
                while (!defeated)
                {
                    response.Data.Log.Add($"Round {round++}");

                    // Round starts
                    foreach (var attacker in characters)
                    {
                        // a single attack
                        var characterClass = attacker.Class;
                        var useWeaponRate = characterClass switch
                        {
                            RpgClass.Knight => request.UseWeaponRateForKnights,
                            RpgClass.Mage => request.UseWeaponRateForMages,
                            _ => request.UseWeaponRateForClerics
                        };

                        var opponents = characters.Where(c => c.Id != attacker.Id).ToList();
                        var opponent = opponents[new Random().Next(opponents.Count)];

                        int damage = 0;
                        string attackUsed = string.Empty;
                        var attackResultMessage = new List<string>();

                        // decide attack type
                        bool useWeapon = new Random().Next(100) < useWeaponRate;

                        if (useWeapon && attacker.Weapon != null)
                        {
                            // weapon attack
                            attackUsed = attacker.Weapon.Name;
                            damage = DoWeaponAttack(attacker, opponent);
                        }
                        else if (attacker.Skills != null && attacker.Skills.Any())
                        {
                            // skill attack
                            var skill = attacker.Skills[new Random().Next(attacker.Skills.Count)];

                            attackUsed = skill.Name;
                            damage = DoSkillAttack(attacker, opponent, skill);
                        }
                        else
                        {
                            attackUsed = "punch";

                            int seed = new Random().Next(100);
                            if (seed < criticalHitRate)
                            {
                                // a Critical Hit!
                                damage = criticalHitDamage;
                                attackResultMessage.Add($"CRITIAL HIT!! {attacker.Name} gave {opponent.Name} a nice punch"
                                    + $", dealing {criticalHitDamage} damage!");
                            }
                            else
                            {
                                // punch attack
                                damage = new Random().Next(1, 6); // 1 to 5 damage
                            }

                            // do damage
                            opponent.HitPoints -= damage;
                        }

                        if (attackResultMessage.Count > 0)
                        {
                            response.Data.Log.AddRange(attackResultMessage);
                        }
                        else if (damage > 0)
                        {
                            response.Data.Log
                                .Add($"{attacker.Name.PadRight(namePadding)} attacks {opponent.Name.PadRight(namePadding)}"
                                   + $" with {attackUsed.PadLeft(weaponPadding)},"
                                   + $" dealing {damage.ToString().PadLeft(2)} damage.");
                        }
                        else if (damage < 0)
                        {
                            response.Data.Log
                                .Add($"{attacker.Name.PadRight(namePadding)} attacks {opponent.Name.PadRight(namePadding)}"
                                   + $" with {attackUsed.PadLeft(weaponPadding)},");
                            response.Data.Log
                                .Add($"but {opponent.Name} defends and counter-attacks,"
                                   + $" dealing {-damage} damage to {attacker.Name}!");
                        }

                        if (opponent.HitPoints <= 0)
                        {
                            opponent.HitPoints = 0;
                            defeated = true;

                            attacker.Victories++;
                            opponent.Defeats++;

                            gameoverMessage.Add($"{opponent.Name} has been defeated!");
                            gameoverMessage.Add($"{attacker.Name} wins with {attacker.HitPoints} HP left!");
                            break;
                        }
                        // Single attack finishes
                    }

                    // Round ends
                    response.Data.Log.AddRange(new List<string>{"Round ends:"});
                    characters.ForEach(c =>
                    {
                        response.Data.Log.Add($"           "
                            + $"{c.Name.PadRight(namePadding)} {c.HitPoints} HP");
                    });
                    response.Data.Log.Add("-----------------------------------------------------");
                }
                // Someone has been defeated
                response.Data.Log.AddRange(gameoverMessage);

                characters.ForEach(c =>
                {
                    c.Fights++;
                    c.HitPoints = 100;
                });

                // save fight results
                _context.Characters.UpdateRange(characters);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }


        public async Task<ServiceResponse<AttackResultDto>> WeaponAttack(WeaponAttackDto request)
        {
            var response = new ServiceResponse<AttackResultDto>();
            try
            {
                var attacker = await _context.Characters
                    .Include(c => c.Weapon)
                    .FirstOrDefaultAsync(c => c.Id == request.AttackerId);
                var opponent = await _context.Characters
                    .FirstOrDefaultAsync(c => c.Id == request.OpponentId);

                if (attacker is null || opponent is null)
                    throw new Exception("Something fishy is going on here...");

                if (attacker.Weapon is null)
                {
                    response.Success = false;
                    response.Message = $"{attacker.Name} does not have a weapon!";
                    return response;
                }

                int damage = DoWeaponAttack(attacker, opponent);

                if (opponent.HitPoints <= 0)
                {
                    opponent.HitPoints = 0;
                    response.Message = $"{opponent.Name} has been defeated!";
                }

                await _context.SaveChangesAsync();

                response.Data = new AttackResultDto
                {
                    Attacker = attacker.Name,
                    Opponent = opponent.Name,
                    AttackerHP = attacker.HitPoints,
                    OpponentHP = opponent.HitPoints,
                    Damage = damage
                };
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }


        public async Task<ServiceResponse<AttackResultDto>> SkillAttack(SkillAttackDto request)
        {
            var response = new ServiceResponse<AttackResultDto>();
            try
            {
                var attacker = await _context.Characters
                    .Include(c => c.Skills)
                    .FirstOrDefaultAsync(c => c.Id == request.AttackerId);
                var opponent = await _context.Characters
                    .FirstOrDefaultAsync(c => c.Id == request.OpponentId);

                if (attacker is null || opponent is null)
                    throw new Exception("Something fishy is going on here...");

                if (attacker.Skills is null)
                {
                    response.Success = false;
                    response.Message = $"{attacker.Name} doesn't know the skill with Id '{request.SkillId}'!";
                    return response;
                }

                var skill = attacker.Skills.FirstOrDefault(s => s.Id == request.SkillId);
                if (skill is null)
                {
                    response.Success = false;
                    response.Message = $"{attacker.Name} doesn't know the skill with Id '{request.SkillId}'!";
                    return response;
                }

                int damage = DoSkillAttack(attacker, opponent, skill);

                if (opponent.HitPoints <= 0)
                {
                    opponent.HitPoints = 0;
                    response.Message = $"{opponent.Name} has been defeated!";
                }

                await _context.SaveChangesAsync();

                response.Data = new AttackResultDto
                {
                    Attacker = attacker.Name,
                    Opponent = opponent.Name,
                    AttackerHP = attacker.HitPoints,
                    OpponentHP = opponent.HitPoints,
                    Damage = damage
                };
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }


        private static int DoWeaponAttack(Character attacker, Character opponent)
        {
            // calculate damage
            int damage = attacker.Weapon.Damage * attacker.Strength / opponent.Defense;
            // do damage
            if (damage > 0)
                opponent.HitPoints -= damage;
            return damage;
        }


        private static int DoSkillAttack(Character attacker, Character opponent, Skill skill)
        {
            // calculate damage
            int damage = skill.Damage * attacker.Intelligent / opponent.Defense;
            // do damage
            if (damage > 0)
                opponent.HitPoints -= damage;
            return damage;
        }
    }
}
