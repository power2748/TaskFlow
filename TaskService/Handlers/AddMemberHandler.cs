using Contracts.Projects;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using TaskService.Data;
using TaskService.Models;
using Wolverine.Http;

namespace TaskService.Handlers
{
    public class AddMemberHandler
    {
        [WolverinePost("/api/projects/members")]
        public async Task<IResult> Handle(AddMemberToProject command,
                                            [FromServices] AppDbContext db,
                                            [FromServices] IConfiguration config)
        {
            var project = await db.Projects
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == command.ProjectId);

            if (project is null)
                return Results.NotFound("Проект не найден");

            if (project.OwnerId != command.RequestedBy)
                return Results.Forbid();

            if (project.Members.Any(m => m.UserEmail == command.MemberEmail))
                return Results.Conflict("Пользователь уже в проекте");

            // Получаем реальный UserId из authdb по email
            using var authConnection = new NpgsqlConnection(config.GetConnectionString("authdb"));
            var user = await authConnection.QueryFirstOrDefaultAsync<UserLookup>(
                @"SELECT ""Id"" as id FROM public.""Users"" WHERE ""Email"" = @Email",
                new { Email = command.MemberEmail });

            if (user is null)
                return Results.NotFound("Пользователь с таким email не найден");

            var member = new ProjectMember
            {
                Id = Guid.NewGuid(),
                ProjectId = command.ProjectId,
                UserId = user.Id, // реальный ID из authdb
                UserEmail = command.MemberEmail
            };

            db.ProjectMembers.Add(member);
            await db.SaveChangesAsync();

            return Results.Ok();
        }
    }
    public class UserLookup
    {
        public Guid Id { get; set; }
    }
}
