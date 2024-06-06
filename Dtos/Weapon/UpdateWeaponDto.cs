using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace dotnet_rpg.Dtos.Weapon
{
    public class UpdateWeaponDto
    {
        [DefaultValue(10)]
        public int Damage { get; set; } = 10;
        [DefaultValue(0)]
        public int CharacterId { get; set; }
    }
}