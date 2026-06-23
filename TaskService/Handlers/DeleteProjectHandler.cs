using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskService.Data;
using Wolverine.Http;

namespace TaskService.Handlers
{
    public class DeleteProjectHandler
    {
        [WolverineDelete("/api/projects/{projectId}")]
        public async Task<IResult> Handle(Guid projectId, [FromServices] AppDbContext db)
        {
            var project = await db.Projects
                .Include(p => p.Members)
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project is null)
                return Results.NotFound();

            // Удаляем задачи проекта
            db.Tasks.RemoveRange(project.Tasks);

            // Удаляем участников
            db.ProjectMembers.RemoveRange(project.Members);

            db.Projects.Remove(project);
            await db.SaveChangesAsync();

            return Results.Ok();
        }
    }
}
