using Dapper;
using Npgsql;
using Wolverine.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Wolverine; // Добавили для IMessageBus
using Contracts.Events; // Добавили для TaskUpdated

namespace TaskService.Handlers;

public class UpdateTaskHandler
{
    [WolverinePut("/api/tasks/{id}")]
    public static async Task<IResult> Handle(
        Guid id,
        UpdateTaskCommand command,
        IConfiguration config,
        IMessageBus bus) // 1. Внедряем шину Wolverine прямо в параметры метода
    {
        using var connection = new NpgsqlConnection(
            config.GetConnectionString("taskdb"));

        // 2. Сначала запрашиваем старые данные, чтобы узнать, кто автор и как задача называлась
        var selectSql = @"SELECT ""Title"", ""CreatedBy"" FROM write.tasks WHERE ""Id"" = @TaskId";
        var oldTask = await connection.QuerySingleOrDefaultAsync<dynamic>(selectSql, new { TaskId = id });

        if (oldTask == null)
        {
            return Results.NotFound(new { message = "Задача не найдена" });
        }

        // Сохраняем старое название и Id пользователя перед апдейтом
        string oldTitle = oldTask.Title;
        string createdBy = oldTask.CreatedBy?.ToString() ?? "Unknown";

        // 3. Выполняем сам UPDATE
        var updateSql = @"
            UPDATE write.tasks 
            SET ""Title"" = @Title, 
                ""Description"" = @Description
            WHERE ""Id"" = @TaskId";

        var affectedRows = await connection.ExecuteAsync(updateSql, new
        {
            TaskId = id,
            Title = command.Title,
            Description = command.Description
        });

        if (affectedRows == 0)
        {
            return Results.NotFound(new { message = "Задача не найдена" });
        }

        // 4. ОТПРАВЛЯЕМ СОБЫТИЕ В ШИНУ
        // Оно мгновенно улетит по TCP в NotificationService
        await bus.PublishAsync(new TaskUpdated(id, createdBy, oldTitle, command.Title));

        return Results.Ok();
    }
}

public record UpdateTaskCommand(string Title, string Description, bool IsCompleted);