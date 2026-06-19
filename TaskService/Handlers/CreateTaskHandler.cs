using Contracts.Tasks;
using TaskService.Data;
using TaskService.Models;
using Wolverine.Http;
using Wolverine;
using Contracts.Events;

namespace TaskService.Handlers;

public class CreateTaskHandler
{
    [WolverinePost("/api/tasks")]
    public async Task Handle(CreateTask command, AppDbContext db, IMessageBus bus)
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
        await bus.PublishAsync(new TaskAssigned(task.Id, task.CreatedBy, task.Title));

    }
}

public class LocalTestHandler
{
    private readonly ILogger<LocalTestHandler> _logger;

    public LocalTestHandler(ILogger<LocalTestHandler> logger)
    {
        _logger = logger;
    }

    public void Handle(TaskAssigned message)
    {
        _logger.LogInformation("🎯 [ТЕСТ] Внутренняя шина TaskService поймала сообщение для задачи: {Title}", message.TaskTitle);
    }
}