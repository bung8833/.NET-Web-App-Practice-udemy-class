using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace dotnet_rpg.Dtos.Character
{
    public class AddCharacterDto
    {
        [DefaultValue("Arthur")]
        public string Name { get; set; } = "Arthur";
        [DefaultValue(100)]
        public int HitPoints { get; set; } = 100;
        [DefaultValue(15)]
        public int Strength { get; set; } = 15;
        [DefaultValue(15)]
        public int Defense { get; set; } = 15;
        [DefaultValue(15)]
        public int Intelligent { get; set; } = 15;
        [DefaultValue(RpgClass.Knight)]
        public RpgClass Class { get; set; } = RpgClass.Knight;
    }
}