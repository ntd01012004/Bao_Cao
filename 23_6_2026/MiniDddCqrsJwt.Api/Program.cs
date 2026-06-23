using MiniDddCqrsJwt.Application.Abstractions.CQRS;
using MiniDddCqrsJwt.Application.Auth.Login;
using MiniDddCqrsJwt.Application.DependencyInjection;
using MiniDddCqrsJwt.Application.Todos.CompleteTodo;
using MiniDddCqrsJwt.Application.Todos.CreateTodo;
using MiniDddCqrsJwt.Application.Todos.GetTodos;
using MiniDddCqrsJwt.Infrastructure.Authentication;
using MiniDddCqrsJwt.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<JwtAuthenticationMiddleware>();

app.MapGet("/swagger/v1/swagger.json", () => Results.Json(SwaggerSpec.OpenApiDocument));

app.MapGet("/swagger", () => Results.Content(SwaggerSpec.SwaggerUiHtml, "text/html"));

app.MapGet("/", () => Results.Ok(new
{
    Name = "Mini DDD CQRS JWT API",
    Endpoints = new[]
    {
        "POST /api/auth/login",
        "GET /api/todos",
        "POST /api/todos",
        "PUT /api/todos/{id}/complete"
    }
}));

app.MapPost("/api/auth/login", async (
    LoginRequest request,
    ICqrsDispatcher dispatcher,
    CancellationToken cancellationToken) =>
{
    try
    {
        var response = await dispatcher.Send(
            new LoginCommand(request.UserName, request.Password),
            cancellationToken);

        return Results.Ok(response);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
});

var todos = app.MapGroup("/api/todos")
    .AddEndpointFilter(RequireAuthenticatedUser);

todos.MapGet("/", async (ICqrsDispatcher dispatcher, CancellationToken cancellationToken) =>
{
    var response = await dispatcher.Query(new GetTodosQuery(), cancellationToken);
    return Results.Ok(response);
});

todos.MapPost("/", async (
    CreateTodoRequest request,
    ICqrsDispatcher dispatcher,
    CancellationToken cancellationToken) =>
{
    try
    {
        var response = await dispatcher.Send(new CreateTodoCommand(request.Title), cancellationToken);
        return Results.Created($"/api/todos/{response.Id}", response);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

todos.MapPut("/{id:guid}/complete", async (
    Guid id,
    ICqrsDispatcher dispatcher,
    CancellationToken cancellationToken) =>
{
    try
    {
        var response = await dispatcher.Send(new CompleteTodoCommand(id), cancellationToken);
        return Results.Ok(response);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
});

app.Run();

static async ValueTask<object?> RequireAuthenticatedUser(
    EndpointFilterInvocationContext context,
    EndpointFilterDelegate next)
{
    if (context.HttpContext.User.Identity?.IsAuthenticated != true)
    {
        return Results.Unauthorized();
    }

    return await next(context);
}

static class SwaggerSpec
{
private static readonly object TodoSchema = new Dictionary<string, object>
{
    ["id"] = new { type = "string", format = "uuid" },
    ["title"] = new { type = "string" },
    ["isCompleted"] = new { type = "boolean" },
    ["createdAtUtc"] = new { type = "string", format = "date-time" },
    ["completedAtUtc"] = new { type = "string", format = "date-time", nullable = true }
};

private static readonly object[] BearerSecurity =
[
    new Dictionary<string, string[]>
    {
        ["Bearer"] = []
    }
];

public static readonly object OpenApiDocument = new
{
    openapi = "3.0.1",
    info = new
    {
        title = "Mini DDD CQRS JWT API",
        version = "v1",
        description = "Mini API demo DDD, CQRS pattern va JWT authentication."
    },
    servers = new[]
    {
        new { url = "http://localhost:5161" }
    },
    paths = new Dictionary<string, object>
    {
        ["/api/auth/login"] = new
        {
            post = new
            {
                tags = new[] { "Auth" },
                summary = "Dang nhap va lay JWT access token",
                requestBody = JsonBody("Login request", new Dictionary<string, object>
                {
                    ["userName"] = new { type = "string", example = "admin" },
                    ["password"] = new { type = "string", example = "Admin@123" }
                }),
                responses = new Dictionary<string, object>
                {
                    ["200"] = JsonResponse("Login thanh cong", new Dictionary<string, object>
                    {
                        ["accessToken"] = new { type = "string" },
                        ["tokenType"] = new { type = "string", example = "Bearer" },
                        ["expiresInSeconds"] = new { type = "integer", example = 3600 }
                    }),
                    ["401"] = new { description = "Sai username hoac password" }
                }
            }
        },
        ["/api/todos"] = new
        {
            get = new
            {
                tags = new[] { "Todos" },
                summary = "Lay danh sach todo",
                security = BearerSecurity,
                responses = new Dictionary<string, object>
                {
                    ["200"] = JsonArrayResponse("Danh sach todo", TodoSchema),
                    ["401"] = new { description = "Chua dang nhap hoac token khong hop le" }
                }
            },
            post = new
            {
                tags = new[] { "Todos" },
                summary = "Tao todo moi",
                security = BearerSecurity,
                requestBody = JsonBody("Create todo request", new Dictionary<string, object>
                {
                    ["title"] = new { type = "string", example = "Hoc DDD CQRS JWT" }
                }),
                responses = new Dictionary<string, object>
                {
                    ["201"] = JsonResponse("Tao todo thanh cong", TodoSchema),
                    ["400"] = new { description = "Du lieu khong hop le" },
                    ["401"] = new { description = "Chua dang nhap hoac token khong hop le" }
                }
            }
        },
        ["/api/todos/{id}/complete"] = new
        {
            put = new
            {
                tags = new[] { "Todos" },
                summary = "Danh dau todo da hoan thanh",
                security = BearerSecurity,
                parameters = new[]
                {
                    new
                    {
                        name = "id",
                        @in = "path",
                        required = true,
                        schema = new { type = "string", format = "uuid" }
                    }
                },
                responses = new Dictionary<string, object>
                {
                    ["200"] = JsonResponse("Cap nhat thanh cong", TodoSchema),
                    ["401"] = new { description = "Chua dang nhap hoac token khong hop le" },
                    ["404"] = new { description = "Khong tim thay todo" }
                }
            }
        }
    },
    components = new
    {
        securitySchemes = new
        {
            Bearer = new
            {
                type = "http",
                scheme = "bearer",
                bearerFormat = "JWT",
                description = "Nhap JWT token lay tu endpoint /api/auth/login"
            }
        }
    }
};

private static object JsonBody(string description, Dictionary<string, object> properties)
{
    return new
    {
        required = true,
        content = new Dictionary<string, object>
        {
            ["application/json"] = new
            {
                schema = new
                {
                    type = "object",
                    properties
                }
            }
        },
        description
    };
}

private static object JsonResponse(string description, object schema)
{
    return new
    {
        description,
        content = new Dictionary<string, object>
        {
            ["application/json"] = new
            {
                schema = new
                {
                    type = "object",
                    properties = schema
                }
            }
        }
    };
}

private static object JsonArrayResponse(string description, object itemSchema)
{
    return new
    {
        description,
        content = new Dictionary<string, object>
        {
            ["application/json"] = new
            {
                schema = new
                {
                    type = "array",
                    items = new
                    {
                        type = "object",
                        properties = itemSchema
                    }
                }
            }
        }
    };
}

public const string SwaggerUiHtml = """
<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <title>Mini DDD CQRS JWT API - Swagger</title>
  <link rel="stylesheet" href="https://unpkg.com/swagger-ui-dist@5.17.14/swagger-ui.css">
</head>
<body>
  <div id="swagger-ui"></div>
  <script src="https://unpkg.com/swagger-ui-dist@5.17.14/swagger-ui-bundle.js"></script>
  <script>
    window.onload = () => {
      window.ui = SwaggerUIBundle({
        url: '/swagger/v1/swagger.json',
        dom_id: '#swagger-ui',
        presets: [SwaggerUIBundle.presets.apis],
        layout: 'BaseLayout'
      });
    };
  </script>
</body>
</html>
""";
}

public sealed record LoginRequest(string UserName, string Password);

public sealed record CreateTodoRequest(string Title);
