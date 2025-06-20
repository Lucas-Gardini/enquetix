using enquetix.Modules.Application.EntityFramework;
using enquetix.Modules.Poll.Repository;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace enquetix.Modules.User.Repository
{
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(Username), IsUnique = true)]
    [Table("Users")]
    public class UserModel : GenericModel
    {
        [Required]
        public string Username { get; set; } = null!;

        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;

        public ICollection<PollModel> PollsCreated { get; set; } = [];
    }
}
