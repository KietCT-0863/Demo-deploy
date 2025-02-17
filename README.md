# REST API Practical

A practical demonstration of building RESTful APIs using .NET 8.

## Project Setup

- Target Framework: .NET 8.0
- API Style: REST

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Your preferred IDE (Visual Studio 2022, VS Code, or JetBrains Rider)

## Getting Started

1. Clone the repository
2. Ensure .NET 8 SDK is installed
3. Build the solution:
   ```bash
   dotnet build
   ```
4. Run the API:
   ```bash
   cd src/RestApiPractical.Api
   dotnet run
   ```

## Practice tasks

1. Generate a new project using the `dotnet new webapi` command.
```bash
dotnet new webapi -n RestApiPractical.PracticeApi -o src/RestApiPractical.PracticeApi
```
2. Add reference to the `RestApiPractical.Core` project.
```bash
dotnet add src/RestApiPractical.PracticeApi/RestApiPractical.PracticeApi.csproj reference src/RestApiPractical.Core/RestApiPractical.Core.csproj
```
3. Configure services in `Program.cs` file.
   - Add `InMemoryDatabase` as a singleton service.
4. Add a new endpoint to the API that returns a list of posts.
5. Add endpoint metadata, description and summary to the new endpoint.
6. Add response information and status code to the new endpoint.
7. Add query parameters to the new endpoint.
8. Add more endpoints for posts.
   - Get post by id
   - Create post
   - Update post
   - Delete post
9. Add endpoints for posts' comments.
   - Get all comments for a post
   - Create comment for a post
10. Group endpoints by tags.
11. Add more endpoints for comments.
    - Get comment by id
    - Update comment
    - Delete comment
12. Add authentication and authorization.
    - Add middlewares
    - Add JWT authentication
    - Add authorization requirements to the endpoints
13. Add endpoint for authentication.
14. Add endpoints for admin.
    - Get all users
    - Lock user
    - Unlock user
15. Setup CI/CD.
    - Setup Azure Web App.
    - Setup deployment pipeline.
16. Advanced topics
