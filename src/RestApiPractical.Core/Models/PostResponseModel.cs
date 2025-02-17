namespace RestApiPractical.Core.Models;

public class PostResponseModel
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }

    public PostResponseModel(string id, string title, string content, DateTime createdAt, string createdBy)
    {
        Id = id;
        Title = title;
        Content = content;
        CreatedAt = createdAt;
        CreatedBy = createdBy;
    }
}
