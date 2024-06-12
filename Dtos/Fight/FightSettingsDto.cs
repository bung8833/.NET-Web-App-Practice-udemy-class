using System.ComponentModel;

namespace dotnet_rpg.Dtos.Fight
{
    public class FightSettingsDto
    {
        public int criticalHitRate { get; set; } = 15; // percentage
        public int criticalHitDamage { get; set; } = 40;
    }
}
