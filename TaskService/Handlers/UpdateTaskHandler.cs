using Dapper;
using Npgsql;
using Wolverine.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace TaskService.Handlers;

public class UpdateTaskHandler
{
    // Этот эндпоинт будет обрабатывать обычный PUT /api/tasks/{id}
    [WolverinePut("/api/tasks/{id}")]
    public static async Task<IResult> Handle(
        Guid id,
        UpdateTaskCommand command,
        IConfiguration config)
    {
        using var connection = new NpgsqlConnection(
            config.GetConnectionString("taskdb"));

        // Обновляем все три поля: Title, Description и IsCompleted
        var sql = @"
            UPDATE write.tasks 
            SET ""Title"" = @Title, 
                ""Description"" = @Description, 
                ""IsCompleted"" = @IsCompleted 
            WHERE ""Id"" = @TaskId";

        var affectedRows = await connection.ExecuteAsync(sql, new
        {
            TaskId = id,
            Title = command.Title,
            Description = command.Description,
            IsCompleted = command.IsCompleted
        });

        if (affectedRows == 0)
        {
            return Results.NotFound(new { message = "Задача не найдена" });
        }

        return Results.Ok();
    }
}

// Модель для приема данных с фронтенда
public record UpdateTaskCommand(string Title, string Description, bool IsCompleted);