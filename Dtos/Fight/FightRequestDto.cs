using System.ComponentModel;
using System.Runtime.InteropServices;
using AutoMapper.Configuration.Annotations;

namespace dotnet_rpg.Dtos.Fight
{
    public class FightRequestDto
    {
        [DefaultValue("4, 5, 11")]
        public string CharacterIds { get; set; } = "4, 5, 11";
        [DefaultValue(80)]
        public int UseWeaponRateForKnights { get; set; } = 80;
        [DefaultValue(35)]
        public int UseWeaponRateForMages { get; set; } = 35;
        [DefaultValue(10)]
        public int UseWeaponRateForClerics { get; set; } = 10;

        public List<int> GetCharacterIds()
        {
            return CharacterIds.Split(',').Select(id => Convert.ToInt32(id)).ToList();
        }
    }
}
