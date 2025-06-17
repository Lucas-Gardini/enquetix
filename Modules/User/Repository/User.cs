using enquetix.Modules.Application.EntityFramework;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace enquetix.Modules.User.Repository
{
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(Username), IsUnique = true)]
    public class UserModel : GenericModel
    {
        [Required]
        public string Username { get; set; } = null!;

        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;

    }
}
