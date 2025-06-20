using enquetix.Modules.Application.EntityFramework;
using enquetix.Modules.User.Repository;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace enquetix.Modules.Poll.Repository
{
    [Index(nameof(Title))]
    [Table("Polls")]
    public class PollModel : GenericModel
    {
        [Required]
        public string Title { get; set; } = null!;

        [Required]
        public string Description { get; set; } = null!;

        [Required]
        public DateTimeOffset StartDate { get; set; }

        [Required]
        public DateTimeOffset EndDate { get; set; }

        [Required]
        public Guid CreatedBy { get; set; }

        [ForeignKey(nameof(CreatedBy))]
        public UserModel? Creator { get; set; }

        public ICollection<PollOptionModel> Options { get; set; } = [];
    }
}