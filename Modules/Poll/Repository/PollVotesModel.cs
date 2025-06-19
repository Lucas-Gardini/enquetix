using enquetix.Modules.Application.EntityFramework;
using enquetix.Modules.User.Repository;
using System.ComponentModel.DataAnnotations.Schema;

namespace enquetix.Modules.Poll.Repository
{
    [Table("PollVotes")]
    public class PollVotesModel : GenericModel
    {
        [ForeignKey(nameof(Poll))]
        public Guid PollId { get; set; }

        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }

        [ForeignKey(nameof(Option))]
        public Guid OptionId { get; set; }

        public PollModel? Poll { get; set; }
        public UserModel? User { get; set; }
        public PollOptionsModel? Option { get; set; }
    }
}