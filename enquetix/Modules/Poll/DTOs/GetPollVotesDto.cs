namespace enquetix.Modules.Poll.DTOs
{
    public class GetPollVotesDto
    {
        public List<GetPollVotesWithQuantityDto> Votes { get; set; } = [];
    }

    public class GetPollVotesWithQuantityDto : GetPollVoteDto
    {
        public int TotalVotes { get; set; } = 0;
    }

    public class GetPollVoteDto
    {
        public Guid OptionId { get; set; }
        public string OptionText { get; set; } = null!;
    }
}
