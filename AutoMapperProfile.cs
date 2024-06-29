using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using dotnet_rpg.Dtos.Character;
using dotnet_rpg.Dtos.Skill;
using dotnet_rpg.Dtos.Weapon;

namespace dotnet_rpg
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Character, GetCharacterDto>();
            CreateMap<AddCharacterDto, Character>();
            CreateMap<Weapon, GetWeaponDto>();
            CreateMap<Skill, GetSkillDto>();
            // Will not reverse map
            // After mapping, still need to set some properties for Fighter
            CreateMap<Character, Fighter>()
                .ForMember(dest => dest.MaxHP, opt => opt.MapFrom(src => src.HP))
                .ForMember(dest => dest.character, opt => opt.MapFrom(src => src));
        }
    }
}