using System.ComponentModel;
using System.Runtime.InteropServices;
using AutoMapper.Configuration.Annotations;

namespace dotnet_rpg.Dtos.Fight
{
    public class FightRequestDto
    {
        [DefaultValue("1, 2, 11")]
        public string CharacterIds { get; set; } = "";

        [DefaultValue(70)]
        public int UseWeaponRateForKnights { get; set; } // percentage
        [DefaultValue(30)]
        public int UseWeaponRateForMages { get; set; } // percentage
        [DefaultValue(50)]
        public int UseWeaponRateForClerics { get; set; } // percentage

        [DefaultValue(10)]
        public int criticalHitRate { get; set; } // percentage
        [DefaultValue(50)]
        public int criticalHitDamage { get; set; }

        public List<int> GetCharacterIds()
        {
            return CharacterIds.Split(',').Select(id => Convert.ToInt32(id)).ToList();
        }
    }
}
