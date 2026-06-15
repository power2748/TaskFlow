using Dapper;
using Npgsql;
using Wolverine.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace TaskService.Handlers;

public class DeleteTaskHandler
{
    // Будем ловить HTTP DELETE запросы по адресу /api/tasks/{id}
    [WolverineDelete("/api/tasks/{id}")]
    public static async Task<IResult> Handle(Guid id, IConfiguration config)
    {
        using var connection = new NpgsqlConnection(
            config.GetConnectionString("taskdb"));

        // Пишем SQL-запрос на удаление
        var sql = @"DELETE FROM write.tasks WHERE ""Id"" = @TaskId";

        var affectedRows = await connection.ExecuteAsync(sql, new { TaskId = id });

        // ЕслиaffectedRows == 0, значит задачи с таким Id не существовало
        if (affectedRows == 0)
        {
            return Results.NotFound(new { message = "Задача не найдена или уже удалена" });
        }

        return Results.Ok();
    }
}