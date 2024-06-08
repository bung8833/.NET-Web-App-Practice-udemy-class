using System.ComponentModel;

namespace dotnet_rpg.Dtos.Fight
{
    public class SkillAttackDto
    {
        public int AttackerId { get; set; }
        public int OpponentId { get; set; }
        [DefaultValue(3)]
        public int SkillId { get; set; } = 3;
    }
}
