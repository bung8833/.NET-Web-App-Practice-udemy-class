using System.ComponentModel;

namespace dotnet_rpg.Dtos.User
{
    public class UserRegisterDto
    {
        [DefaultValue("")]
        public string Username { get; set; } = String.Empty;
    }
}
