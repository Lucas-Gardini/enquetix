using enquetix.Modules.Application.EntityFramework;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace enquetix.Modules.Poll.Repository
{
    [Table("PollOptions")]
    [Index(nameof(PollId), nameof(OptionText), IsUnique = true)]
    public class PollOptionModel : GenericModel
    {
        [Required]
        public string OptionText { get; set; } = null!;

        [Required]
        public Guid PollId { get; set; }

        [JsonIgnore]
        [ForeignKey(nameof(PollId))]
        public PollModel? Poll { get; set; }

        [NotMapped]
        public int TotalVotes { get; set; } = 0;
    }
}
