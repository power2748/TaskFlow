using Dapper;
using Npgsql;
using Wolverine.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Wolverine; // Добавили для IMessageBus
using Contracts.Events; // Добавили для TaskDeleted

namespace TaskService.Handlers;

public class DeleteTaskHandler
{
    [WolverineDelete("/api/tasks/{id}")]
    public static async Task<IResult> Handle(
        Guid id,
        IConfiguration config,
        IMessageBus bus) // 1. Внедряем шину сообщений Wolverine
    {
        using var connection = new NpgsqlConnection(
            config.GetConnectionString("taskdb"));

        // 2. Сначала вытягиваем данные задачи, пока они еще есть в базе
        var selectSql = @"SELECT ""Title"", ""CreatedBy"" FROM write.tasks WHERE ""Id"" = @TaskId";
        var task = await connection.QuerySingleOrDefaultAsync<dynamic>(selectSql, new { TaskId = id });

        if (task == null)
        {
            return Results.NotFound(new { message = "Задача не найдена или уже удалена" });
        }

        string taskTitle = task.Title;
        string createdBy = task.CreatedBy?.ToString() ?? "Unknown";

        // 3. Выполняем физическое удаление строки
        var deleteSql = @"DELETE FROM write.tasks WHERE ""Id"" = @TaskId";
        var affectedRows = await connection.ExecuteAsync(deleteSql, new { TaskId = id });

        if (affectedRows == 0)
        {
            return Results.NotFound(new { message = "Задача не найдена или уже удалена" });
        }

        // 4. Публикуем событие удаления в шину
        // Пакет мгновенно летит по TCP в NotificationService
        await bus.PublishAsync(new TaskDeleted(id, createdBy, taskTitle));

        return Results.Ok();
    }
}