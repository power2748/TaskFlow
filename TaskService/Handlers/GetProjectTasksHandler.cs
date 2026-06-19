using Contracts.Tasks;
using Dapper;
using Npgsql;
using Wolverine.Http;

namespace TaskService.Handlers
{
    public class GetProjectTasksHandler
    {
        [WolverineGet("/api/projects/{projectId}/tasks")]
        public async Task<IEnumerable<TaskDto>> Handle(Guid projectId, IConfiguration config)
        {
            using var connection = new NpgsqlConnection(config.GetConnectionString("taskdb"));

            var parameters = new DynamicParameters();
            parameters.Add("ProjectId", projectId);

            return await connection.QueryAsync<TaskDto>(
                @"SELECT ""Id"" as id, ""Title"" as title, ""Description"" as description,
                 ""Status"" as status, '' as assigneduseremail, ""ProjectId"" as projectid
          FROM write.tasks
          WHERE ""ProjectId"" = @ProjectId",
                parameters);
        }
    }
}
