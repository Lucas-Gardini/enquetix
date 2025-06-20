namespace enquetix.Modules.User.DTOs
{
    public class GetUserDto
    {
        public Guid? Id { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Username { get; set; } = null!;
    }
}
