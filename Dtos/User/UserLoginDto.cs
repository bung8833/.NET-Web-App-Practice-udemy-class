using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace dotnet_rpg.Dtos.User
{
    public class UserLoginDto
    {
        [DefaultValue("")]
        public string Username { get; set; } = String.Empty;
        [DefaultValue("123456")]
        public string Password { get; set; } = String.Empty;
    }
}