using AutoMapper;
using Azure;
using Azure.Identity;
using dotnet_rpg.Data;
using dotnet_rpg.Dtos.Character;
using dotnet_rpg.Dtos.Fight;
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

            //await ClearFightResults(request.GetCharacterIds());
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

                // all attack at same time
                // this is just for showing the fight log, not real attack order
                characters = characters.OrderBy(c => c.Class).ThenBy(c => c.Name).ToList();

                Dictionary<int, int> characterHealths 
                    = characters.ToDictionary(c => c.Id, c => c.HitPoints);

                Dictionary<RpgClass, int> useWeaponRates = new Dictionary<RpgClass, int>
                {
                    { RpgClass.Knight, request.UseWeaponRateForKnights },
                    { RpgClass.Mage, request.UseWeaponRateForMages },
                    { RpgClass.Cleric, request.UseWeaponRateForClerics }
                };
                
                int round = 1;
                List<Character> winners = new();
                int winnerHP = -1;
                List<Character> losers = new();

                // Each round Characters randomly attack others, until someone is defeated
                bool defeated = false;
                while (!defeated)
                {
                    response.Data.Log.Add($"           Round {round++}");

                    // Round starts
                    foreach (var attacker in characters)
                    {
                        // a single attack
                        var useWeaponRate = useWeaponRates[attacker.Class];

                        var opponents = characters.Where(c => c.Id != attacker.Id).ToList();
                        var opponent = opponents[new Random(Guid.NewGuid().GetHashCode()).Next(opponents.Count)];

                        var attackResultMessage = new List<string>();

                        // decide attack type
                        bool useWeapon = new Random(Guid.NewGuid().GetHashCode()).Next(100) < useWeaponRate;

                        if (useWeapon && attacker.Weapon != null)
                        {
                            // use weapon
                            DoWeaponAttack(attacker, opponent, ref attackResultMessage);
                        }
                        else if (attacker.Skills != null && attacker.Skills.Any())
                        {
                            // use skill
                            Skill skill = GetRandomSkill(attacker.Skills);
                            // todo implement skill healing method
                            // todo implement skilltypes
                            if (skill.Name == "Heal")
                            {
                                int heal = Math.Abs(skill.Damage);
                                attacker.HitPoints += heal;
                                attackResultMessage
                                    .Add($"{attacker.Name} uses {skill.Name} to heal themselves,"
                                       + $" restoring {heal} HP.");
                            }
                            else
                            {
                                DoSkillAttack(attacker, opponent, skill, ref attackResultMessage);
                            }
                        }
                        else
                        {
                            // has no weapon or skill, use punch
                            Punch(attacker, opponent, 
                                request.criticalHitRate, request.criticalHitDamage, 
                                ref attackResultMessage);
                        }

                        // Record single attack result
                        response.Data.Log.AddRange(attackResultMessage);

                        // Single attack finishes
                    }

                    // Round ends
                    // check if someone has been defeated
                    var deads = characters.Where(c => c.HitPoints <= 0).ToList();
                    if (deads.Count > 0)
                    {
                        defeated = true;
                        deads.ForEach(d =>
                        {
                            d.HitPoints = 0;
                            losers.Add(d);
                        });
                        GetWinnersByHP(characters, out winners, out winnerHP);
                    }
                    // Record Round results
                    response.Data.Log.AddRange(new List<string> { "Round ends:" });
                    characters.ForEach(c =>
                    {
                        response.Data.Log.Add($"           "
                            + $"{c.Name.PadRight(10)} {c.HitPoints.ToString().PadLeft(3)} HP");
                    });
                    response.Data.Log.Add("-----------------------------------------------------");
                }
                // Game ends, i.e. someone has been defeated
                winners.ForEach(w => w.Victories++);
                string winnerNames = string.Join(", ", winners.Select(w => w.Name));
                losers.ForEach(l => l.Defeats++);
                string loserNames = string.Join(", ", losers.Select(l => l.Name));

                response.Data.Log.Add($"{loserNames} has been defeated!");
                response.Data.Log.Add($"{winnerNames} wins with {winnerHP} HP left!");

                characters.ForEach(c =>
                {
                    c.Fights++;
                    c.HitPoints = characterHealths[c.Id];
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


        private static void GetWinnersByHPRatio(List<Character> characters, Dictionary<int, int> characterHealths,
            out List<Character> winners, out int winnerHPPercents)
        {
            int precision = 2;

            Dictionary<int, double> HPRatios =
                characters.ToDictionary(c => c.Id, c =>
                    Math.Round((double)c.HitPoints / characterHealths[c.Id], precision, MidpointRounding.AwayFromZero));
            double maxHPRatio = HPRatios.Values.Max();

            winners = HPRatios.Keys.Where(k => HPRatios[k] == maxHPRatio)
                .Select(k => characters.First(c => c.Id == k)).ToList();

            winnerHPPercents = (int)Math.Round(maxHPRatio * 100, 0, MidpointRounding.AwayFromZero);

            //KeyValuePair<int, double> winnerData = HPRatios.MaxBy(kvp => kvp.Value);
            //winners = new List<Character> { characters.First(c => c.Id == winnerData.Key) };
            //winnerHPRatio = winnerData.Value;
        }


        private static void GetWinnersByHP(List<Character> characters,
            out List<Character> winners, out int winnerHP)
        {
            int maxHP = characters.Max(c => c.HitPoints);

            winners = characters.Where(c => c.HitPoints == maxHP).ToList();
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

        
        private static int DoWeaponAttack(Character attacker, Character opponent,
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
                opponent.HitPoints -= damage;
                attackResultMessage
                    .Add($"{attacker.Name} attacks {opponent.Name}"
                       + $" with {attacker.Weapon.Name},"
                       + $" dealing {damage} damage.");
            }
            else
            {
                attacker.HitPoints -= damage;
                attackResultMessage
                    .Add($"{attacker.Name} attacks {opponent.Name}"
                       + $" with {attacker.Weapon.Name},");
                attackResultMessage
                    .Add($"    but {opponent.Name} defends and counter-attacks,"
                       + $" dealing {damage} damage to {attacker.Name}!");
            }
            return damage;
        }

        
        private static int DoSkillAttack(Character attacker, Character opponent, Skill skill,
            ref List<string> attackResultMessage)
        {
            bool counterAttack = false;
            // calculate damage
            double originalDamage = 1.0 * skill.Damage * attacker.Intelligent / opponent.Defense;
            int damage = (int)Math.Round(originalDamage, 0, MidpointRounding.AwayFromZero);

            // defense and counter-attack by chance
            if (opponent.Intelligent > attacker.Intelligent)
            {
                int diff = opponent.Intelligent - attacker.Intelligent;
                counterAttack = new Random(Guid.NewGuid().GetHashCode())
                    .Next(opponent.Intelligent * opponent.Intelligent) < diff * diff;
            }
            // do damage
            if (!counterAttack)
            {
                opponent.HitPoints -= damage;
                attackResultMessage
                    .Add($"{attacker.Name} attacks {opponent.Name}"
                       + $" with {skill.Name},"
                       + $" dealing {damage} damage.");
            }
            else
            {
                attacker.HitPoints -= damage;
                attackResultMessage
                    .Add($"{attacker.Name} attacks {opponent.Name}"
                       + $" with {skill.Name},");

                attackResultMessage
                    .Add($"    but {opponent.Name} defends and counter-attacks,"
                       + $" dealing {damage} damage to {attacker.Name}!");
            }
            return damage;
        }


        private static int Punch(Character attacker, Character opponent, int criticalHitRate,
            int criticalHitDamage, ref List<string> attackResultMessage)
        {
            int damage = 0;

            Random rand = new Random(Guid.NewGuid().GetHashCode());
            int critical = rand.Next(100);
            int onePun = rand.Next(100);
            int regular = rand.Next(1, 6);

            if (critical < criticalHitRate)
            {
                // a Critical Hit!
                damage = criticalHitDamage;
                attackResultMessage.Add($"    CRITIAL HIT!! {attacker.Name} gives {opponent.Name} a solid punch"
                      + $", dealing {criticalHitDamage} damage!!");
            }
            else if (opponent.HitPoints < 10 && onePun < 50)
            {
                // 尾刀
                damage = opponent.HitPoints;
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
            opponent.HitPoints -= damage;

            return damage;
        }
    }
}
