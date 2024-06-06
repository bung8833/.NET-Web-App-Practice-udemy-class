using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnet_rpg.Dtos.Weapon
{
    public class GetWeaponAndCharacterDto
    {
        public string Name { get; set; } = String.Empty;
        public int Damage { get; set; }
        public int CharacterId { get; set; }
        public string Character { get; set; } = String.Empty;
    }
}