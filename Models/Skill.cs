using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnet_rpg.Models
{
    public class Skill
    {
        public int Id { get; set; }
        public string Name { get; set; } = String.Empty;
        public int Damage { get; set; }
        public int SkillActivationRate { get; set; } // percentage
        public List<Character>? Characters { get; set; }
    }
}