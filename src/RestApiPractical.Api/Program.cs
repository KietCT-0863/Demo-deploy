using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using RestApiPractical.Api.Constants;
using RestApiPractical.Core.Models;
using RestApiPractical.Core.Constants;
using RestApiPractical.Core.Entities;
using RestApiPractical.Core.Persistence;

var builder = WebApplication.CreateBuilder(args);

#region Configure services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Posts API",
        Version = "v1",
        Description = "A simple REST API for managing posts and comments"
    });

    // Add security definition
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Add security requirement
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddSingleton<InMemoryDatabase>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]))
        };

        options.TokenValidationParameters.NameClaimType = Claims.Username;
        options.TokenValidationParameters.RoleClaimType = Claims.Role;
    });

builder.Services.AddAuthorization();
#endregion

var app = builder.Build();

#region Configure middleware

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

MapEndpoints(app);

#endregion

app.Run();

#region Map endpoints

void MapEndpoints(WebApplication app)
{
    MapAuthGroup(app);
    MapPostsGroup(app);
    MapCommentsGroup(app);
    MapAdminGroup(app);
}

void MapPostsGroup(IEndpointRouteBuilder app)
{
    var posts = app.MapGroup("/api/posts")
        .WithTags("Posts")
        .RequireAuthorization();

    // Public endpoints with AllowAnonymous
    posts.MapGet("/", GetAllPosts)
        .WithName("GetAllPosts")
        .AllowAnonymous()
        .Produces<IEnumerable<PostResponseModel>>(StatusCodes.Status200OK)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Gets all posts";
            operation.Description = "Retrieves a list of all blog posts";
            operation.Responses.Add("200 (custom)", new OpenApiResponse
            {
                Description = "This is my custom 200 response",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "array",
                            Items = new OpenApiSchema
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.Schema,
                                    Id = "PostResponseModel"
                                }
                            }
                        }
                    }
                }
            });
            return operation;
        });

    posts.MapGet("/{id}", GetPost)
        .WithName("GetPost")
        .AllowAnonymous()
        .Produces<PostResponseModel>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Gets a post by ID";
            operation.Description = "Retrieves a specific blog post by its unique identifier";
            return operation;
        });

    posts.MapGet("/{postId:guid}/comments", GetPostComments)
        .WithName("GetPostComments")
        .AllowAnonymous()
        .Produces<IEnumerable<CommentResponseModel>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Gets post comments";
            operation.Description = "Retrieves all comments for a specific blog post";
            return operation;
        });

    // Protected endpoints (inherit group authorization)
    posts.MapPost("/", CreatePost)
        .WithName("CreatePost")
        .Produces<PostResponseModel>(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .WithOpenApi(operation =>
        {
            operation.Summary = "Creates a post";
            operation.Description = "Creates a new blog post";
            return operation;
        });

    posts.MapPut("/{id:guid}", UpdatePost)
        .WithName("UpdatePost")
        .Produces<PostResponseModel>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status403Forbidden)
        .ProducesValidationProblem()
        .WithOpenApi(operation =>
        {
            operation.Summary = "Updates a post";
            operation.Description = "Updates an existing blog post's content";
            return operation;
        });

    posts.MapPatch("/{id:guid}", PatchPost)
        .WithName("PatchPost")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status403Forbidden)
        .ProducesValidationProblem()
        .WithOpenApi(operation =>
        {
            operation.Summary = "Patches a post";
            operation.Description = "Patches an existing blog post";
            return operation;
        });

    posts.MapDelete("/{id:guid}", DeletePost)
        .WithName("DeletePost")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status403Forbidden)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Deletes a post";
            operation.Description = "Removes a blog post and all its associated comments";
            return operation;
        });

    posts.MapPost("/{postId:guid}/comments", CreateComment)
        .WithName("CreateComment")
        .Produces<CommentResponseModel>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status404NotFound)
        .ProducesValidationProblem()
        .WithOpenApi(operation =>
        {
            operation.Summary = "Creates a comment";
            operation.Description = "Adds a new comment to a specific blog post";
            return operation;
        });
}

void MapCommentsGroup(IEndpointRouteBuilder app)
{
    var comments = app.MapGroup("/api/comments")
        .WithTags("Comments")
        .RequireAuthorization();

    // Get comment by id (public)
    comments.MapGet("/{id:guid}", GetComment)
        .WithName("GetComment")
        .AllowAnonymous()
        .Produces<CommentResponseModel>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Gets a comment by ID";
            operation.Description = "Retrieves a specific comment by its unique identifier";
            return operation;
        });

    // Update comment
    comments.MapPut("/{id:guid}", UpdateComment)
        .WithName("UpdateComment")
        .Produces<CommentResponseModel>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status403Forbidden)
        .ProducesValidationProblem()
        .WithOpenApi(operation =>
        {
            operation.Summary = "Updates a comment";
            operation.Description = "Updates an existing comment's content";
            return operation;
        });

    // Delete comment
    comments.MapDelete("/{id:guid}", DeleteComment)
        .WithName("DeleteComment")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status403Forbidden)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Deletes a comment";
            operation.Description = "Removes a comment from its associated post";
            return operation;
        });
}

void MapAuthGroup(IEndpointRouteBuilder app)
{
    var auth = app.MapGroup("/api/auth")
        .WithTags("Authentication & Authorization");

    auth.MapPost("/token", GetToken)
        .WithName("GetToken")
        .Produces<TokenResponseModel>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Get access token";
            operation.Description = "Authenticates user and returns an access token using ROPC grant type";
            return operation;
        });
}

void MapAdminGroup(IEndpointRouteBuilder app)
{
    var admin = app.MapGroup("/api/admin")
        .WithTags("Administration")
        .RequireAuthorization(policy => policy.RequireRole(Roles.Admin));

    admin.MapGet("/users", GetAllUsers)
        .WithName("GetAllUsers")
        .Produces<IEnumerable<UserResponseModel>>(StatusCodes.Status200OK)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Gets all users";
            operation.Description = "Retrieves a list of all registered users (Admin only)";
            return operation;
        });

    admin.MapPost("/users/{userId}/lock", LockUser)
        .WithName("LockUser")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Lock user";
            operation.Description = "Locks a user account preventing them from accessing the system";
            return operation;
        });

    admin.MapPost("/users/{userId:guid}/unlock", UnlockUser)
        .WithName("UnlockUser")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Unlock user";
            operation.Description = "Unlocks a previously locked user account";
            return operation;
        });
}

static IResult LockUser(Guid userId, InMemoryDatabase db)
{
    // TODO: Implement user locking logic
    return Results.NoContent();
}

static IResult UnlockUser(Guid userId, InMemoryDatabase db)
{
    // TODO: Implement user unlocking logic
    return Results.NoContent();
}

#endregion

#region Handler Methods
static IResult GetAllPosts([AsParameters] GetAllPostsQueryModel query, InMemoryDatabase db)
{
    var posts = db.GetAllPosts(query);
    return Results.Ok(posts.Select(p => new PostResponseModel(
        p.Id,
        p.Title,
        p.Content,
        p.CreatedAt,
        p.CreatedByUsername
    )));
}

static IResult GetPost(string id, InMemoryDatabase db)
{
    var post = db.GetPostById(id);
    if (post == null)
        return Results.NotFound();

    return Results.Ok(new PostResponseModel(
        post.Id,
        post.Title,
        post.Content,
        post.CreatedAt,
        post.CreatedByUsername
    ));
}

static IResult CreatePost(CreatePostModel model, InMemoryDatabase db, ClaimsPrincipal user)
{
    var userId = user.FindFirstValue(Claims.Id);
    var username = user.FindFirstValue(Claims.Username);

    var post = new PostEntity
    {
        Title = model.Title,
        Content = model.Content,
        CreatedById = userId,
        CreatedByUsername = username
    };

    var created = db.CreatePost(post);
    var response = new PostResponseModel(
        created.Id,
        created.Title,
        created.Content,
        created.CreatedAt,
        created.CreatedByUsername
    );

    return Results.CreatedAtRoute("GetPost", new { id = created.Id }, response);
}

static IResult UpdatePost(string id, UpdatePostModel model, InMemoryDatabase db, ClaimsPrincipal user)
{
    try
    {
        var userId = user.FindFirstValue(Claims.Id);
        var post = db.GetPostById(id);

        if (post == null)
            return Results.NotFound();

        if (post.CreatedById != userId)
            return Results.Forbid();

        var updated = db.UpdatePost(id, model);
        var response = new PostResponseModel(
            updated.Id,
            updated.Title,
            updated.Content,
            updated.CreatedAt,
            updated.CreatedByUsername
        );
        return Results.Ok(response);
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound();
    }
}

static IResult PatchPost(string id, PatchPostModel model, InMemoryDatabase db, ClaimsPrincipal user)
{
    try
    {
        var userId = user.FindFirstValue(Claims.Id);
        var post = db.GetPostById(id);

        if (post == null)
            return Results.NotFound();

        if (post.CreatedById != userId)
            return Results.Forbid();

        db.PatchPost(id, model);
        return Results.NoContent();
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound();
    }
}

static IResult DeletePost(string id, InMemoryDatabase db)
{
    var result = db.DeletePost(id);
    return result ? Results.NoContent() : Results.NotFound();
}

static IResult GetPostComments(string postId, InMemoryDatabase db)
{
    var post = db.GetPostById(postId);
    if (post == null)
        return Results.NotFound();

    var comments = db.GetPostComments(postId);
    var response = comments.Select(c => new CommentResponseModel(
        c.Id,
        c.Content,
        c.CreatedAt,
        c.CreatedByUsername
    ));

    return Results.Ok(response);
}

static IResult CreateComment(string postId, CreateCommentModel model, InMemoryDatabase db, ClaimsPrincipal user)
{
    try
    {
        var userId = user.FindFirstValue(Claims.Id);
        var username = user.FindFirstValue(Claims.Username);

        var comment = new CommentEntity
        {
            Content = model.Content,
            CreatedById = userId,
            CreatedByUsername = username
        };

        var created = db.AddComment(postId, comment);
        var response = new CommentResponseModel(
            created.Id,
            created.Content,
            created.CreatedAt,
            created.CreatedByUsername
        );
        return Results.CreatedAtRoute("GetComment", new { id = created.Id }, response);
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound();
    }
}

static IResult GetComment(string id, InMemoryDatabase db)
{
    var comment = db.GetCommentById(id);
    if (comment == null)
        return Results.NotFound();

    return Results.Ok(new CommentResponseModel(
        comment.Id,
        comment.Content,
        comment.CreatedAt,
        comment.CreatedByUsername
    ));
}

static IResult UpdateComment(string id, UpdateCommentModel model, InMemoryDatabase db)
{
    try
    {
        var updated = db.UpdateComment(id, model);
        var response = new CommentResponseModel(updated.Id, updated.Content, updated.CreatedAt, updated.CreatedByUsername);
        return Results.Ok(response);
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound();
    }
}

static IResult DeleteComment(string id, InMemoryDatabase db)
{
    var result = db.DeleteComment(id);
    return result ? Results.NoContent() : Results.NotFound();
}

static IResult GetToken(TokenRequestModel request, InMemoryDatabase db, IConfiguration configuration)
{
    if (request.grant_type != "password")
    {
        return Results.BadRequest(new { error = "unsupported_grant_type" });
    }

    var user = db.ValidateUser(request.username, request.password);
    if (user == null)
    {
        return Results.BadRequest(new { error = "invalid_grant" });
    }

    var token = GenerateJwtToken(user, configuration);
    var response = new TokenResponseModel
    {
        access_token = token,
        token_type = "Bearer",
        expires_in = int.Parse(configuration.GetSection("Jwt")["ExpiryInHours"]) * 3600
    };

    return Results.Ok(response);
}

static string GenerateJwtToken(UserEntity user, IConfiguration configuration)
{
    var jwtSettings = configuration.GetSection("Jwt");
    var securityKey = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>
    {
        new Claim(Claims.Id, user.Id.ToString()),
        new Claim(Claims.Username, user.Username),
        new Claim(Claims.TokenId, Guid.NewGuid().ToString()),
    };

    // Add role claims using our custom claim type
    claims.AddRange(user.Roles.Select(role => new Claim(Claims.Role, role)));

    var token = new JwtSecurityToken(
        issuer: jwtSettings["Issuer"],
        audience: jwtSettings["Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddHours(int.Parse(jwtSettings["ExpiryInHours"])),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}

static IResult GetAllUsers(InMemoryDatabase db)
{
    var users = db.GetAllUsers();
    var response = users.Select(u => new UserResponseModel(
        u.Id,
        u.Username,
        u.Roles,
        u.CreatedAt
    ));
    return Results.Ok(response);
}

#endregion
