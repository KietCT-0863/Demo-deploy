namespace RestApiPractical.Core.Models;

public class UserResponseModel
{
    public string Id { get; set; }
    public string Username { get; set; }
    public List<string> Roles { get; set; }
    public DateTime CreatedAt { get; set; }

    public UserResponseModel(string id, string username, List<string> roles, DateTime createdAt)
    {
        Id = id;
        Username = username;
        Roles = roles;
        CreatedAt = createdAt;
    }
}
