using System.ComponentModel;

namespace dotnet_rpg.Dtos.Weapon
{
    public class AddWeaponDto
    {
        [DefaultValue("Sword")]
        public string Name { get; set; } = "Sword";
        [DefaultValue(10)]
        public int Damage { get; set; } = 10;
        [DefaultValue(0)]
        public int CharacterId { get; set; }
    }
}
