using enquetix.Modules.Poll.Repository;

namespace enquetix.Modules.Poll.DTOs
{
    public class GetPollsDto
    {
        public int Total { get; set; }
        public List<PollModel> Polls { get; set; } = [];
    }
}
