using AutoMapper;
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

            int criticalHitRate = request.criticalHitRate; // percentage
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
                // test only
                // await ClearFightResults(request.GetCharacterIds());

                // random attack order
                characters = characters.OrderBy(c => Guid.NewGuid()).ToList();

                int namePadding = characters.Max(c => c.Name.Length);
                int weaponPadding = characters.Max(c => c.Weapon?.Name.Length ?? 5);
                Dictionary<int, int> characterHealths = new Dictionary<int, int>();
                characters.ForEach(c => characterHealths.Add(c.Id, c.HitPoints));

                int round = 1;
                List<Character> winners = new();
                int winnerHPPercents = 100;
                Character loser = new Character();
                List<string> gameoverMessage = new List<string>();

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
                        bool counterAttack = false;
                        string attackUsed = string.Empty;
                        var attackResultMessage = new List<string>();

                        // decide attack type
                        bool useWeapon = new Random().Next(100) < useWeaponRate;

                        if (useWeapon && attacker.Weapon != null)
                        {
                            // weapon attack
                            attackUsed = attacker.Weapon.Name;
                            damage = DoWeaponAttack(attacker, opponent, out counterAttack);
                        }
                        else if (attacker.Skills != null && attacker.Skills.Any())
                        {
                            // skill attack
                            Skill skill = GetRandomSkill(attacker.Skills);

                            attackUsed = skill.Name;
                            damage = DoSkillAttack(attacker, opponent, skill, out counterAttack);
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
                                // todo 尾刀 if opponent HP < 10

                                
                                // punch attack
                                damage = new Random().Next(1, 6); // 1 to 5 damage
                            }

                            // do damage
                            opponent.HitPoints -= damage;
                        }

                        // Record single attack result
                        if (attackResultMessage.Count > 0)
                        {
                            response.Data.Log.AddRange(attackResultMessage);
                        }
                        else if (!counterAttack)
                        {
                            response.Data.Log
                                .Add($"{attacker.Name.PadRight(namePadding)} attacks {opponent.Name.PadRight(namePadding)}"
                                   + $" with {attackUsed.PadLeft(weaponPadding)},"
                                   + $" dealing {damage.ToString().PadLeft(2)} damage.");
                        }
                        else
                        {
                            response.Data.Log
                                .Add($"{attacker.Name.PadRight(namePadding)} attacks {opponent.Name.PadRight(namePadding)}"
                                   + $" with {attackUsed.PadLeft(weaponPadding)},");
                            response.Data.Log
                                .Add($"but {opponent.Name} defends and counter-attacks,"
                                   + $" dealing {damage} damage to {attacker.Name}!");
                        }

                        // check if someone has been defeated
                        var dead = characters.FirstOrDefault(c => c.HitPoints <= 0);
                        if (dead != null)
                        {
                            defeated = true;

                            dead.HitPoints = 0;
                            loser = dead;
                            GetWinnersByHPRatio(characters, characterHealths, 
                                out winners, out winnerHPPercents);
                            break;
                        }
                        // Single attack finishes
                    }

                    // Round ends
                    response.Data.Log.AddRange(new List<string> { "Round ends:" });
                    characters.ForEach(c =>
                    {
                        response.Data.Log.Add($"           "
                            + $"{c.Name.PadRight(namePadding)} {c.HitPoints} HP");
                    });
                    response.Data.Log.Add("-----------------------------------------------------");
                }
                // Someone has been defeated
                winners.ForEach(w => w.Victories++);
                string winnerNames = string.Join(", ", winners.Select(w => w.Name));
                loser.Defeats++;

                gameoverMessage.Add($"{loser.Name} has been defeated!");
                gameoverMessage.Add($"{winnerNames} wins with {winnerHPPercents}% HP left!");
                response.Data.Log.AddRange(gameoverMessage);
                
                characters.ForEach(c =>
                {
                    c.Fights++;
                    c.HitPoints = characterHealths[c.Id];
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
            out bool counterAttack)
        {
            counterAttack = false;
            // calculate damage
            int damage = attacker.Weapon.Damage * attacker.Strength / opponent.Defense;

            // defense and counter-attack by chance
            if (opponent.Strength > attacker.Strength)
            {
                int diff = opponent.Strength - attacker.Strength;
                counterAttack = new Random()
                    .Next(opponent.Strength * opponent.Strength) < diff * diff;
            }
            // do damage
            if (!counterAttack)
                opponent.HitPoints -= damage;
            else
                attacker.HitPoints -= damage;
            
            return damage;
        }


        private static int DoSkillAttack(Character attacker, Character opponent, Skill skill, 
            out bool counterAttack)
        {
            counterAttack = false;
            // calculate damage
            int damage = skill.Damage * attacker.Intelligent / opponent.Defense;

            // defense and counter-attack by chance
            if (opponent.Intelligent > attacker.Intelligent)
            {
                int diff = opponent.Intelligent - attacker.Intelligent;
                counterAttack = new Random()
                    .Next(opponent.Intelligent * opponent.Intelligent) < diff * diff;
            }
            // do damage
            if (!counterAttack)
                opponent.HitPoints -= damage;
            else
                attacker.HitPoints -= damage;

            return damage;
        }


        private static Skill GetRandomSkill(List<Skill> skills)
        {
            int randomNum = new Random().Next(skills.Sum(s => s.SkillActivationRate));
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
    }
}
