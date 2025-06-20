using System.ComponentModel.DataAnnotations;

namespace enquetix.Modules.Poll.DTOs
{
    public class CreatePollOptionDto
    {
        [Required]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Option text must be between 1 and 200 characters long.")]
        public string OptionText { get; set; } = null!;

    }
}
