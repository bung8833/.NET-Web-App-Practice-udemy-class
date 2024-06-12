using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnet_rpg.Dtos.Skill
{
    public class GetSkillDto
    {
        public string Name { get; set; } = String.Empty;
        public SkillType Type { get; set; }
    }
}