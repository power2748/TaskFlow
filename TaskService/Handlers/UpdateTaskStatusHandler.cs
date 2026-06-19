using Npgsql;
using Dapper;
using Wolverine.Http;

namespace TaskService.Handlers
{
    public record UpdateTaskStatusCommand(Guid TaskId, Contracts.Tasks.TaskStatus NewStatus);

    public class UpdateTaskStatusHandler
    {
        [WolverinePut("/api/tasks/status")]
        public static async Task<IResult> Handle(UpdateTaskStatusCommand command, IConfiguration config)
        {
            using var connection = new NpgsqlConnection(config.GetConnectionString("taskdb"));

            var sql = @"UPDATE write.tasks 
                    SET ""Status"" = @NewStatus 
                    WHERE ""Id"" = @TaskId";

            // Записываем статус в БД как int
            var affectedRows = await connection.ExecuteAsync(sql, new
            {
                TaskId = command.TaskId,
                NewStatus = (int)command.NewStatus
            });

            if (affectedRows == 0)
                return Results.NotFound("Задача не найдена.");

            return Results.Ok();
        }
    }
}
