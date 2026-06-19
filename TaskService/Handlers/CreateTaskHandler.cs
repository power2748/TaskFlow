using Contracts.Tasks;
using TaskService.Data;
using TaskService.Models;
using Wolverine.Http;

namespace TaskService.Handlers;

public class CreateTaskHandler
{
    [WolverinePost("/api/tasks")]
    public async Task Handle(CreateTask command, AppDbContext db)
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = command.Title,
            Description = command.Description,
            Status = 0,
            CreatedBy = command.CreatedBy,
            CreatedAt = DateTime.UtcNow,
            ProjectId = command.ProjectId
        };

        db.Tasks.Add(task);
        await db.SaveChangesAsync();
    }
}