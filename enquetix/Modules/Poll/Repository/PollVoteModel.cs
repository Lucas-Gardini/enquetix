using enquetix.Modules.Application.EntityFramework;
using enquetix.Modules.User.Repository;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace enquetix.Modules.Poll.Repository
{
    [Table("PollVotes")]
    [Index(nameof(PollId), nameof(UserId), IsUnique = true)]
    public class PollVoteModel : GenericModel
    {
        [ForeignKey(nameof(Poll))]
        public Guid PollId { get; set; }

        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }

        [ForeignKey(nameof(Option))]
        public Guid OptionId { get; set; }

        public PollModel? Poll { get; set; }
        public UserModel? User { get; set; }
        public PollOptionModel? Option { get; set; }
    }
}