using System.ComponentModel;
using System.Runtime.InteropServices;
using AutoMapper.Configuration.Annotations;

namespace dotnet_rpg.Dtos.Fight
{
    public class FightRequestDto
    {
        [DefaultValue("1, 2, 11, 12")]
        public string CharacterIds { get; set; } = "1, 2, 11, 12";

        [DefaultValue(70)]
        public int UseWeaponRateForKnights { get; set; } = 70; // percentage
        [DefaultValue(30)]
        public int UseWeaponRateForMages { get; set; } = 30; // percentage
        [DefaultValue(50)]
        public int UseWeaponRateForClerics { get; set; } = 50; // percentage

        [DefaultValue(10)]
        public int criticalHitRate { get; set; } = 10; // percentage
        [DefaultValue(50)]
        public int criticalHitDamage { get; set; } = 40;

        public List<int> GetCharacterIds()
        {
            return CharacterIds.Split(',').Select(id => Convert.ToInt32(id)).ToList();
        }
    }
}
