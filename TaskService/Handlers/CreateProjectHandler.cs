using Contracts.Projects;
using Microsoft.AspNetCore.Mvc;
using TaskService.Data;
using TaskService.Models;
using Wolverine.Http;

namespace TaskService.Handlers
{
    public class CreateProjectHandler
    {
        [WolverinePost("/api/projects")]
        public async Task<ProjectDto> Handle(CreateProject command, [FromServices] AppDbContext db)
        {
            var project = new Project
            {
                Id = Guid.NewGuid(),
                Name = command.Name,
                OwnerId = command.OwnerId,
                CreatedAt = DateTime.UtcNow
            };

            db.Projects.Add(project);
            await db.SaveChangesAsync();

            return new ProjectDto(project.Id, project.Name, project.OwnerId, new());
        }
    }
}
