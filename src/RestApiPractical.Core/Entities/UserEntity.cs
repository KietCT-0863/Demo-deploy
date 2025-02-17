namespace RestApiPractical.Core.Entities;

public class UserEntity
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public List<string> Roles { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}