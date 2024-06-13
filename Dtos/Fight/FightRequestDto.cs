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

        [DefaultValue(25)]
        public int criticalPunchRate { get; set; } = 25; // percentage
        [DefaultValue(50)]
        public int criticalPunchDamage { get; set; } = 50;
        [DefaultValue(75)]
        public int onePunchRate { get; set; } = 75; // percentage

        public List<int> GetCharacterIds()
        {
            return CharacterIds.Split(',').Select(id => Convert.ToInt32(id)).ToList();
        }
    }
}
