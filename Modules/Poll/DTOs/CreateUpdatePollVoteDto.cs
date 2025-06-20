namespace enquetix.Modules.Poll.DTOs
{
    public class CreateUpdatePollVoteInputDto
    {
        public Guid? OptionId { get; set; }
        // Só o campo que o usuário PODE preencher
    }

    public class CreateUpdatePollVoteDto : CreateUpdatePollVoteInputDto
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid PollId { get; set; }
        public Guid UserId { get; set; }
    }
}
