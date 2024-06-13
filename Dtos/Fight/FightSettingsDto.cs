using System.ComponentModel;

namespace dotnet_rpg.Dtos.Fight
{
    public class FightSettingsDto
    {
        public int criticalPunchRate { get; set; } = 25; // percentage
        public int criticalPunchDamage { get; set; } = 50;
        public int onePunchRate { get; set; } = 75; // percentage
    }
}
