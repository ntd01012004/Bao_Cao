# Mini DDD CQRS JWT API

Mini project ASP.NET Core minh hoa 3 y tuong:

- DDD: Tach `Domain`, `Application`, `Infrastructure`, `Api`.
- CQRS: Tach command/query va handler rieng.
- JWT Authentication: Dang nhap de lay token, dung token truy cap API todo.

> May hien tai chi co .NET SDK 10 template/targeting pack, nen project dung `net10.0`. Neu cai .NET 8 SDK, co the doi `TargetFramework` ve `net8.0`.

## Cau truc

```text
MiniDddCqrsJwt.Api
  Program.cs                      Minimal APIs, auth middleware, endpoints
MiniDddCqrsJwt.Application
  Abstractions/CQRS               ICommand, IQuery, handlers, dispatcher
  Auth/Login                      LoginCommand
  Todos/CreateTodo                CreateTodoCommand
  Todos/CompleteTodo              CompleteTodoCommand
  Todos/GetTodos                  GetTodosQuery
MiniDddCqrsJwt.Domain
  Todos                           Todo aggregate va value object
  Users                           User entity demo
MiniDddCqrsJwt.Infrastructure
  Authentication                  JWT generator/validator/middleware
  Persistence                     In-memory repositories
```

## Chay project

```powershell
cd C:\Users\Lenovo\DemoNet8Api\23_6_2026
dotnet run --project MiniDddCqrsJwt.Api
```

## Tai khoan demo

```text
username: admin
password: Admin@123
```

## Test nhanh

Mo Swagger UI:

```text
http://localhost:5161/swagger
```

Trong Swagger, bam `POST /api/auth/login` de lay `accessToken`, sau do bam nut `Authorize` va nhap:

```text
Bearer <accessToken>
```

Lay token:

```http
POST http://localhost:5161/api/auth/login
Content-Type: application/json

{
  "userName": "admin",
  "password": "Admin@123"
}
```

Tao todo:

```http
POST http://localhost:5161/api/todos
Authorization: Bearer <accessToken>
Content-Type: application/json

{
  "title": "Hoc DDD CQRS JWT"
}
```

Lay danh sach todo:

```http
GET http://localhost:5161/api/todos
Authorization: Bearer <accessToken>
```

Hoan thanh todo:

```http
PUT http://localhost:5161/api/todos/<todoId>/complete
Authorization: Bearer <accessToken>
```

## Ghi chu

- Repository dang dung in-memory storage de project gon va khong can database.
- JWT duoc generate/validate bang HMAC SHA256 khong dung package ngoai, de build duoc offline.
- Trong production nen hash password, luu secret bang user-secrets/environment variable, va co the thay middleware demo bang `Microsoft.AspNetCore.Authentication.JwtBearer`.
