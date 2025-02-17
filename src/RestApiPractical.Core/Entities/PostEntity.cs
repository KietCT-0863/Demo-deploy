namespace RestApiPractical.Core.Entities;

public class PostEntity
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedById { get; set; }
    public string CreatedByUsername { get; set; }
    public List<CommentEntity> Comments { get; set; } = new();
}