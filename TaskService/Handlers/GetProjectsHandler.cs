using Contracts.Projects;
using Dapper;
using Npgsql;
using Wolverine.Http;

namespace TaskService.Handlers
{
    public class GetProjectsHandler
    {
        [WolverineGet("/api/projects/{userId}")]
        public async Task<IEnumerable<ProjectDto>> Handle(Guid userId, IConfiguration config)
        {
            using var connection = new NpgsqlConnection(config.GetConnectionString("taskdb"));

            var projects = await connection.QueryAsync<dynamic>(
                @"SELECT p.""Id"", p.""Name"", p.""OwnerId""
              FROM write.projects p
              LEFT JOIN write.project_members pm ON pm.""ProjectId"" = p.""Id""
              WHERE p.""OwnerId"" = @UserId OR pm.""UserId"" = @UserId
              GROUP BY p.""Id"", p.""Name"", p.""OwnerId""",
                new { UserId = userId });

            var result = new List<ProjectDto>();
            foreach (var p in projects)
            {
                var members = await connection.QueryAsync<ProjectMemberDto>(
                    @"SELECT pm.""UserId"", pm.""UserEmail"" as email
                  FROM write.project_members pm
                  WHERE pm.""ProjectId"" = @ProjectId",
                    new { ProjectId = (Guid)p.Id });

                result.Add(new ProjectDto(p.Id, p.Name, p.OwnerId, members.ToList()));
            }

            return result;
        }
    }
}
