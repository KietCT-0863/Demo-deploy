using RestApiPractical.Api.Constants;
using RestApiPractical.Core.Models;
using RestApiPractical.Core.Entities;

namespace RestApiPractical.Core.Persistence;

public class InMemoryDatabase
{
    private static readonly List<PostEntity> _posts = new();
    private static readonly List<CommentEntity> _comments = new();
    private static readonly List<UserEntity> _users = new()
    {
        new UserEntity
        {
            Id = Guid.NewGuid().ToString(),
            Username = "admin",
            Password = "admin123",
            Roles = new List<string> { Roles.Admin, Roles.User },
            CreatedAt = DateTime.UtcNow
        },
        new UserEntity
        {
            Id = Guid.NewGuid().ToString(),
            Username = "user",
            Password = "user123",
            Roles = new List<string> { Roles.User },
            CreatedAt = DateTime.UtcNow
        }
    };

    public IReadOnlyList<PostEntity> GetAllPosts(GetAllPostsQueryModel query)
    {
        var createdBy = query.created_by;
        var search = query.search;
        var sortBy = query.sort_by;
        var sortOrder = query.sort_order;
        var page = query.page ?? 1;
        var pageSize = query.page_size ?? 10;

        var posts = _posts.AsEnumerable();
        if (!string.IsNullOrEmpty(createdBy))
            posts = posts.Where(p => p.CreatedByUsername == createdBy);
        if (!string.IsNullOrEmpty(search))
            posts = posts.Where(p => p.Title.Contains(search) || p.Content.Contains(search));

        switch (sortBy)
        {
            case "created_at":
                posts = sortOrder == "asc" ? posts.OrderBy(p => p.CreatedAt) : posts.OrderByDescending(p => p.CreatedAt);
                break;
            case "title":
                posts = sortOrder == "asc" ? posts.OrderBy(p => p.Title) : posts.OrderByDescending(p => p.Title);
                break;
        }

        return posts.Skip((page - 1) * pageSize).Take(pageSize).ToList().AsReadOnly();
    }

    public PostEntity GetPostById(string id)
    {
        return _posts.FirstOrDefault(p => p.Id == id);
    }

    public IReadOnlyList<CommentEntity> GetPostComments(string postId)
    {
        return _comments.Where(c => c.PostId == postId).ToList().AsReadOnly();
    }

    public PostEntity CreatePost(PostEntity post)
    {
        post.Id = Guid.NewGuid().ToString();
        post.CreatedAt = DateTime.UtcNow;
        _posts.Add(post);
        return post;
    }

    public CommentEntity AddComment(string postId, CommentEntity comment)
    {
        var post = GetPostById(postId);
        if (post == null)
            throw new KeyNotFoundException("Post not found");

        comment.Id = Guid.NewGuid().ToString();
        comment.PostId = postId;
        comment.CreatedAt = DateTime.UtcNow;
        _comments.Add(comment);
        return comment;
    }

    public bool DeletePost(string id)
    {
        var post = _posts.FirstOrDefault(p => p.Id == id);
        if (post == null)
            return false;

        _posts.Remove(post);
        var commentsToRemove = _comments.Where(c => c.PostId == id).ToList();
        foreach (var comment in commentsToRemove)
        {
            _comments.Remove(comment);
        }
        return true;
    }

    public PostEntity UpdatePost(string id, UpdatePostModel model)
    {
        var post = _posts.FirstOrDefault(p => p.Id == id);
        if (post == null)
            throw new KeyNotFoundException("Post not found");

        post.Title = model.Title;
        post.Content = model.Content;
        return post;
    }

    public void PatchPost(string id, PatchPostModel model)
    {
        var post = _posts.FirstOrDefault(p => p.Id == id);
        if (post == null)
            throw new KeyNotFoundException("Post not found");

        post.Title = model.Title;
    }

    public CommentEntity UpdateComment(string postId, string commentId, UpdateCommentModel model)
    {
        var comment = _comments.FirstOrDefault(c => c.Id == commentId && c.PostId == postId);
        if (comment == null)
            throw new KeyNotFoundException("Comment not found");

        comment.Content = model.Content;
        return comment;
    }

    public CommentEntity GetCommentById(string id)
    {
        return _comments.FirstOrDefault(c => c.Id == id);
    }

    public CommentEntity UpdateComment(string id, UpdateCommentModel model)
    {
        var comment = _comments.FirstOrDefault(c => c.Id == id);
        if (comment == null)
            throw new KeyNotFoundException("Comment not found");

        comment.Content = model.Content;
        return comment;
    }

    public bool DeleteComment(string id)
    {
        var comment = _comments.FirstOrDefault(c => c.Id == id);
        if (comment == null)
            return false;

        _comments.Remove(comment);
        return true;
    }

    public UserEntity ValidateUser(string username, string password)
    {
        return _users.FirstOrDefault(u =>
            u.Username.Equals(username) &&
            u.Password.Equals(password));
    }

    public IEnumerable<UserEntity> GetAllUsers()
    {
        return _users.ToList();
    }
}