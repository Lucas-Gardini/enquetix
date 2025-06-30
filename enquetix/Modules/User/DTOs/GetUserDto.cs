using System.ComponentModel.DataAnnotations;

namespace enquetix.Modules.User.DTOs
{
    public class GetUserDto
    {
        public Guid? Id { get; set; } = null!;

        [Required]
        public string Email { get; set; } = null!;

        [Required]
        public string Username { get; set; } = null!;
    }
}
