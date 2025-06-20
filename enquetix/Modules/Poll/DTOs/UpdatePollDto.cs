using System.ComponentModel.DataAnnotations;

namespace enquetix.Modules.Poll.DTOs
{
    public class UpdatePollDto
    {
        [StringLength(100, MinimumLength = 5)]
        public string? Title { get; set; } = null;

        [StringLength(500)]
        public string? Description { get; set; } = null;

        [DataType(DataType.DateTime)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}")]
        public DateTimeOffset? StartDate { get; set; } = null;

        [DataType(DataType.DateTime)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-ddTHH:mm:ss}")]
        public DateTimeOffset? EndDate { get; set; } = null;
    }
}
