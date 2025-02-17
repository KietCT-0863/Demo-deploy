namespace RestApiPractical.Core.Models;

public class CommentResponseModel
{
    public string Id { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }

    public CommentResponseModel(string id, string content, DateTime createdAt, string createdBy)
    {
        Id = id;
        Content = content;
        CreatedAt = createdAt;
        CreatedBy = createdBy;
    }
}
