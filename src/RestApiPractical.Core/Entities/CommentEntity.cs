namespace RestApiPractical.Core.Entities;

public class CommentEntity
{
    public string Id { get; set; }
    public string PostId { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedById { get; set; }
    public string CreatedByUsername { get; set; }
}