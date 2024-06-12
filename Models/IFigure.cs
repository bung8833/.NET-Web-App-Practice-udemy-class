using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnet_rpg.Models
{
    public interface IFigure
    {
        int Id { get; set; }
        string Name { get; set; }
        int HP { get; set; }
        int Strength { get; set; }
        int Defense { get; set; }
        int Intelligence { get; set; }
        RpgClass Class { get; set; }
        Weapon? Weapon { get; set; }
        List<Skill>? Skills { get; set; }
        int Fights { get; set; }
        int Victories { get; set; }
        int Defeats { get; set; }
    }
}