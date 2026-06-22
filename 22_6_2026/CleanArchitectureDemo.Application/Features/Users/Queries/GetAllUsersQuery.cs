using MediatR;
using CleanArchitectureDemo.Application.DTOs;

public record GetAllUsersQuery() : IRequest<List<UserDto>>;