using System.ComponentModel;
using System.Runtime.InteropServices;
using AutoMapper.Configuration.Annotations;

namespace dotnet_rpg.Dtos.Fight
{
    public class FightRequestDto
    {
        [DefaultValue("1, 2, 3, 21, 22")]
        public string CharacterIds { get; set; } = "1, 2, 3, 21, 22";

        [DefaultValue(70)]
        public int UseWeaponRateForKnights { get; set; } = 70; // percentage
        [DefaultValue(20)]
        public int UseWeaponRateForMages { get; set; } = 20; // percentage
        [DefaultValue(50)]
        public int UseWeaponRateForClerics { get; set; } = 50; // percentage

        [DefaultValue(20)]
        public int criticalPunchRate { get; set; } = 20; // percentage
        [DefaultValue(40)]
        public int criticalPunchDamage { get; set; } = 40;
        [DefaultValue(90)]
        public int onePunchRate { get; set; } = 90; // percentage

        public List<int> GetCharacterIds()
        {
            return CharacterIds.Split(',').Select(id => Convert.ToInt32(id)).ToList();
        }
    }
}
