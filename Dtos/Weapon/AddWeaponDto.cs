namespace dotnet_rpg.Dtos.Weapon
{
    public class AddWeaponDto
    {
        public string Name { get; set; } = String.Empty;
        public int Damage { get; set; }
        public int CharacterId { get; set; }
    }
}
