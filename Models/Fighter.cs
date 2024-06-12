namespace dotnet_rpg.Models
{
    public class Fighter : IFigure
    {
        public int Id { get; set; }
        public string Name { get; set; } = "Frodo";
        public int HP { get; set; } = 200;
        public int MaxHP { get; set; } = 200;
        public int HPToChange { get; set; } = 0;
        public int Strength { get; set; } = 10;
        public int Defense { get; set; } = 10;
        public int Intelligence { get; set; } = 10;
        public RpgClass Class { get; set; } = RpgClass.Knight;
        public Weapon? Weapon { get; set; }
        public int UseWeaponRate { get; set; } = 70;
        public List<Skill>? Skills { get; set; }
        public int Fights { get; set; } = 0;
        public int Victories { get; set; } = 0;
        public int Defeats { get; set; } = 0;
        public Character character { get; set; } = new Character();
    }
}
