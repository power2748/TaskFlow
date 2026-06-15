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
            IsCompleted = false,
            CreatedBy = command.CreatedBy,
            CreatedAt = DateTime.UtcNow
        };

        db.Tasks.Add(task);
        await db.SaveChangesAsync();
    }
}