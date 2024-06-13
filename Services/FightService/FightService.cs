using AutoMapper;
using Azure;
using Azure.Identity;
using dotnet_rpg.Data;
using dotnet_rpg.Dtos.Character;
using dotnet_rpg.Dtos.Fight;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace dotnet_rpg.Services.FightService
{
    public class FightService : IFightService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public FightService(DataContext dataContext, IMapper mapper)
        {
            _context = dataContext;
            _mapper = mapper;
        }


        public async Task<ServiceResponse<FightResultDto>> Fight(FightRequestDto request)
        {
            var response = new ServiceResponse<FightResultDto>
            {
                Data = new FightResultDto()
            };

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
                    HPToChange = 0,
                    Strength = c.Strength,
                    Defense = c.Defense,
                    Intelligence = c.Intelligence,
                    Class = c.Class,
                    Weapon = c.Weapon,
                    UseWeaponRate = useWeaponRates[c.Class],
                    Skills = c.Skills,
                    character = c,
                }).ToList();

                FightSettingsDto settings = new FightSettingsDto
                {
                    criticalPunchRate = request.criticalPunchRate,
                    criticalPunchDamage = request.criticalPunchDamage,
                    onePunchRate = request.onePunchRate,
                };
                // this is just for showing the fight log, not real attack order
                fighters = fighters.OrderBy(c => c.Class).ThenBy(c => c.Name).ToList();

                // main logic
                response.Data.Log = DoFight(ref fighters, settings);

                // Update fight results for characters
                fighters.ForEach(f =>
                {
                    f.character.Fights += f.Fights;
                    f.character.Victories += f.Victories;
                    f.character.Defeats += f.Defeats;
                });

                // Save fight results
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


        public List<string> DoFight(ref List<Fighter> fighters, FightSettingsDto settings)
        {
            List<string> log = new List<string>();

            int round = 1;
            List<Fighter> winners = new();
            int winnerHP = -1;
            List<Fighter> losers = new();

            // Each round Fighters randomly attack others, until someone is defeated
            bool defeated = false;
            while (!defeated)
            {
                log.Add($"           Round {round++}");

                // Round starts
                foreach (var attacker in fighters)
                {
                    // a single attack

                    var opponents = fighters.Where(c => c.Id != attacker.Id).ToList();
                    var opponent = opponents[new Random().Next(opponents.Count)];

                    var attackResultMessage = new List<string>();

                    // decide attack type
                    bool useWeapon = new Random().Next(100) < attacker.UseWeaponRate;

                    if (useWeapon && attacker.Weapon != null)
                    {
                        // use weapon
                        DoWeaponAttack(attacker, opponent, ref attackResultMessage);
                    }
                    else if (attacker.Skills != null && attacker.Skills.Any())
                    {
                        // use skill
                        Skill skill = GetRandomSkill(attacker.Skills);

                        if (skill.Type == SkillType.Combat)
                        {
                            DoSkillAttack(attacker, opponent, skill, ref attackResultMessage);
                        }
                        else if (skill.Type == SkillType.Heal)
                        {
                            Heal(attacker, skill, ref attackResultMessage);
                        }
                        else if (skill.Type == SkillType.LifeLeech)
                        {
                            LifeLeech(attacker, opponent, skill, ref attackResultMessage);
                        }
                    }
                    else
                    {
                        // has no weapon or skill, use punch
                        Punch(attacker, opponent,
                            settings.criticalPunchRate, settings.criticalPunchDamage, settings.onePunchRate,
                            ref attackResultMessage);
                    }

                    // Record single attack result
                    log.AddRange(attackResultMessage);

                    // Single attack finishes
                }

                // Round ends
                // set HP correctly after all attacks
                fighters.ForEach(c => {
                    c.HP = c.HP + c.HPToChange;
                    c.HPToChange = 0;
                });

                // check if someone has been defeated
                var deads = fighters.Where(c => c.HP <= 0).ToList();
                if (deads.Count > 0)
                {
                    defeated = true;
                    deads.ForEach(d =>
                    {
                        d.HP = 0;
                        losers.Add(d);
                    });
                    GetWinnersByHP(fighters, out winners, out winnerHP);
                }
                // Record Round results
                log.AddRange(new List<string> { "Round ends:" });
                fighters.ForEach(c =>
                {
                    log.Add($"           "
                        + $"{c.Name.PadRight(10)} {c.HP.ToString().PadLeft(3)} HP");
                });
                log.Add("-----------------------------------------------------");
            }
            // Game ends, i.e. someone has been defeated
            winners.ForEach(w => w.Victories++);
            string winnerNames = string.Join(", ", winners.Select(w => w.Name));
            losers.ForEach(l => l.Defeats++);
            string loserNames = string.Join(", ", losers.Select(l => l.Name));

            log.Add($"{loserNames} has been defeated!");
            log.Add($"{winnerNames} wins with {winnerHP} HP left!");

            fighters.ForEach(c =>
            {
                c.Fights++;
                c.HP = c.MaxHP;
            });

            return log;
        }


        private static void GetWinnersByHP(List<Fighter> fighters,
            out List<Fighter> winners, out int winnerHP)
        {
            int maxHP = fighters.Max(c => c.HP);

            winners = fighters.Where(c => c.HP == maxHP).ToList();
            winnerHP = maxHP;
        }


        private static Skill GetRandomSkill(List<Skill> skills)
        {
            int randomNum = new Random(Guid.NewGuid().GetHashCode()).Next(skills.Sum(s => s.SkillActivationRate));
            Skill randomSkill = new Skill();

            foreach (var skill in skills)
            {
                randomNum -= skill.SkillActivationRate;
                if (randomNum < 0)
                {
                    // found the skill
                    randomSkill = skill;
                    break;
                }
            }
            return randomSkill;
        }


        public async Task<ServiceResponse<List<GetCharacterDto>>> ClearFightResults(List<int> characterIds)
        {
            var response = new ServiceResponse<List<GetCharacterDto>>();

            var characters = await _context.Characters
                .Where(c => characterIds.Contains(c.Id))
                .Include(c => c.Weapon)
                .Include(c => c.Skills).ToListAsync();

            if (characters.Count < 1)
            {
                response.Success = false;
                response.Message = "No characters found to clear fight results!";
                return response;
            }

            characters.ForEach(c =>
            {
                c.Fights = 0;
                c.Victories = 0;
                c.Defeats = 0;
            });
            _context.Characters.UpdateRange(characters);
            await _context.SaveChangesAsync();

            response.Data = _mapper.Map<List<GetCharacterDto>>(characters);
            return response;
        }


        private static int DoWeaponAttack(Fighter attacker, Fighter opponent,
            ref List<string> attackResultMessage)
        {
            bool counterAttack = false;
            // calculate damage
            double originalDamage = 1.0 * attacker.Weapon.Damage * attacker.Strength / opponent.Defense;
            int damage = (int)Math.Round(originalDamage, 0, MidpointRounding.AwayFromZero);

            // defense and counter-attack by chance
            if (opponent.Strength > attacker.Strength)
            {
                int diff = opponent.Strength - attacker.Strength;
                counterAttack = new Random(Guid.NewGuid().GetHashCode())
                    .Next(opponent.Strength * opponent.Strength) < diff * diff;
            }
            // do damage
            if (!counterAttack)
            {
                opponent.HPToChange -= damage;
                attackResultMessage
                    .Add($"{attacker.Name} attacks {opponent.Name}"
                       + $" with {attacker.Weapon.Name},"
                       + $" dealing {damage} damage.");
            }
            else
            {
                attacker.HPToChange -= damage;
                attackResultMessage
                    .Add($"{attacker.Name} attacks {opponent.Name}"
                       + $" with {attacker.Weapon.Name},");
                attackResultMessage
                    .Add($"    but {opponent.Name} defends and counter-attacks,"
                       + $" dealing {damage} damage to {attacker.Name}!");
            }
            return damage;
        }


        private static int DoSkillAttack(Fighter attacker, Fighter opponent, Skill skill,
            ref List<string> attackResultMessage)
        {
            bool counterAttack = false;
            // calculate damage
            double originalDamage = 1.0 * skill.Damage * attacker.Intelligence / opponent.Defense;
            int damage = (int)Math.Round(originalDamage, 0, MidpointRounding.AwayFromZero);

            // defense and counter-attack by chance
            if (opponent.Intelligence > attacker.Intelligence)
            {
                int diff = opponent.Intelligence - attacker.Intelligence;
                counterAttack = new Random(Guid.NewGuid().GetHashCode())
                    .Next(opponent.Intelligence * opponent.Intelligence) < diff * diff;
            }
            // do damage
            if (!counterAttack)
            {
                opponent.HPToChange -= damage;
                attackResultMessage
                    .Add($"{attacker.Name} attacks {opponent.Name}"
                       + $" with {skill.Name},"
                       + $" dealing {damage} damage.");
            }
            else
            {
                attacker.HPToChange -= damage;
                attackResultMessage
                    .Add($"{attacker.Name} attacks {opponent.Name}"
                       + $" with {skill.Name},");

                attackResultMessage
                    .Add($"    but {opponent.Name} defends and counter-attacks,"
                       + $" dealing {damage} damage to {attacker.Name}!");
            }
            return damage;
        }


        private static void Heal(Fighter user, Skill skill, ref List<string> attackResultMessage)
        {
            user.HPToChange += skill.Heal;
            attackResultMessage
                .Add($"{user.Name} uses {skill.Name} to heal themselves,"
                   + $" restoring {skill.Heal} HP.");
        }


        private static void LifeLeech(Fighter attacker, Fighter opponent, Skill skill,
            ref List<string> attackResultMessage)
        {
            int leech = (int)Math.Round(opponent.HP * skill.LifeLeechPercentage / 100.0, 0, MidpointRounding.AwayFromZero);
            opponent.HPToChange -= leech;
            attacker.HPToChange += leech;
            attackResultMessage
                .Add($"{attacker.Name} attacks {opponent.Name}"
                   + $" with {skill.Name}, draining and restoring {leech} HP!");
        }


        private static int Punch(Fighter attacker, Fighter opponent, int criticalPunchRate,
            int criticalPunchDamage, int onePunchRate, ref List<string> attackResultMessage)
        {
            int damage = 0;

            Random rand = new Random(Guid.NewGuid().GetHashCode());
            int critical = rand.Next(100);
            int onePun = rand.Next(100);
            int regular = rand.Next(1, 6);

            if (critical < criticalPunchRate)
            {
                // a Critical Hit!
                damage = criticalPunchDamage;
                attackResultMessage.Add($"    CRITIAL HIT!! {attacker.Name} gives {opponent.Name} a solid punch"
                      + $", dealing {criticalPunchDamage} damage!!");
            }
            else if (opponent.HP < 10 && onePun < onePunchRate)
            {
                // 尾刀
                damage = opponent.HP;
                attackResultMessage.Add($"    One punch! {attacker.Name} gives {opponent.Name}"
                      + $" a {damage} damage punch to death!");
            }
            else
            {
                // punch attack
                damage = regular;
                attackResultMessage.Add($"{attacker.Name} punches {opponent.Name}"
                      + $", dealing {damage} damage.");
            }
            // do damage
            opponent.HPToChange -= damage;

            return damage;
        }
    }
}
