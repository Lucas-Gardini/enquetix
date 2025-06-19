using enquetix.Modules.Application.EntityFramework;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace enquetix.Modules.Poll.Repository
{
    [Table("PollOptions")]
    [Index(nameof(PollId), nameof(OptionText), IsUnique = true)]
    public class PollOptionsModel : GenericModel
    {
        [Required]
        public string OptionText { get; set; } = null!;

        [Required]
        public Guid PollId { get; set; }

        [ForeignKey(nameof(PollId))]
        public PollModel? Poll { get; set; }
    }
}
