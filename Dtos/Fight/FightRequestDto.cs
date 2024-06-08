using System.ComponentModel;
using System.Runtime.InteropServices;

namespace dotnet_rpg.Dtos.Fight
{
    public class FightRequestDto
    {
        public List<int> CharacterIds { get; set; } = new List<int>() {4, 5, 11};
        [DefaultValue(80)]
        public int UseWeaponRateForKnights { get; set; } = 80;
        [DefaultValue(35)]
        public int UseWeaponRateForMages { get; set; } = 35;
        [DefaultValue(10)]
        public int UseWeaponRateForClerics { get; set; } = 10;
    }
}
