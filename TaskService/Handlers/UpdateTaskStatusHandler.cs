using Contracts.Events;
using Dapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Npgsql;
using Wolverine;
using Wolverine.Http;

namespace TaskService.Handlers
{
    public record UpdateTaskStatusCommand(Guid TaskId, Contracts.Tasks.TaskStatus NewStatus);

    public class UpdateTaskStatusHandler
    {
        [WolverinePut("/api/tasks/status")]
        public static async Task<IResult> Handle(UpdateTaskStatusCommand command, IConfiguration config, IMessageBus bus)
        {
            using var connection = new NpgsqlConnection(config.GetConnectionString("taskdb"));
            var selectSql = @"SELECT ""Status"", ""CreatedBy"" FROM write.tasks WHERE ""Id"" = @TaskId";
            var task = await connection.QuerySingleOrDefaultAsync<dynamic>(selectSql, new { TaskId = command.TaskId });

            if (task == null)
                return Results.NotFound("Задача не найдена.");

            int oldStatus = Convert.ToInt32(task.Status);
            int newStatus = (int)command.NewStatus;

            // Защита: если статус не изменился, не спамим уведомлениями
            if (oldStatus == newStatus)
                return Results.Ok();

            // Приводим к string или берем Guid в зависимости от того, как устроен твой TaskStatusUpdated
            string assigneeId = task.CreatedBy?.ToString() ?? Guid.Empty.ToString();
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

            await bus.PublishAsync(new TaskStatusUpdated(command.TaskId, assigneeId, oldStatus, newStatus));
            return Results.Ok();
        }
    }
}
