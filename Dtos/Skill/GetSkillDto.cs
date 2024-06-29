using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.OpenApi.Extensions;

namespace dotnet_rpg.Dtos.Skill
{
    public class GetSkillDto
    {
        public string Name { get; set; } = String.Empty;
        public string Type { get; set; } = String.Empty;
        public bool Revive { get; set; } = false;
        public int SkillActivationRate { get; set; } // percentage
    }
}