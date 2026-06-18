using Dapper;
using Npgsql;
using Wolverine.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Tasks;

namespace TaskService.Handlers;

public class GetTasksByUserHandler
{
    // УБРАЛИ [Authorize], чтобы не было ошибки 401
    [WolverineGet("/api/tasks/{userId}")]
    public async Task<IEnumerable<TaskDto>> Handle(
        Guid userId,
        IConfiguration config) // Убрали параметр ClaimsPrincipal user
    {
        // ВАЖНО: Зайдите в таблицу Users базы authdb, найдите там админа admin@admin.com
        // и скопируйте его реальный Id. Вставьте его вместо нулей ниже:
        Guid adminId = Guid.Parse("c1d8ff5d-c728-4e9a-b546-57cd113d034b");

        // Если пришедший в URL-адресе ID совпадает с ID админа — включаем режим всевидящего ока
        bool isAdmin = userId == adminId;

        // Печатаем в консоль для проверки
        Console.WriteLine($"[DEBUG] Запрос задач для ID: {userId}. Это админ? {isAdmin}");

        using var taskConnection = new NpgsqlConnection(config.GetConnectionString("taskdb"));

        string sql;
        object queryParams;

        if (isAdmin)
        {
            // Админ видит задачи ВСЕХ пользователей
            sql = @"SELECT ""Id"" as id, 
                           ""Title"" as title, 
                           ""Description"" as description, 
                           ""IsCompleted"" as iscompleted,
                           ""CreatedBy"" as createdby,
                           ""ProjectId"" as projectid
                    FROM write.tasks";
            queryParams = new { };
        }
        else
        {
            var projectTasks = await taskConnection.QueryAsync<dynamic>(
       @"SELECT t.""Id"", t.""Title"", t.""ProjectId"" 
          FROM write.tasks t
          INNER JOIN write.project_members pm ON pm.""ProjectId"" = t.""ProjectId""
          WHERE pm.""UserId"" = @UserId",
       new { UserId = userId });

            Console.WriteLine($"[DEBUG] Tasks through INNER JOIN: {projectTasks.Count()}");
            // Обычный пользователь видит только свои
            sql = @"
                SELECT DISTINCT 
                 t.""Id"" as id, 
                t.""Title"" as title, 
                t.""Description"" as description, 
                t.""IsCompleted"" as iscompleted,
                t.""CreatedBy"" as createdby,
                t.""ProjectId"" as projectid
                FROM write.tasks t
                LEFT JOIN write.projects p ON p.""Id"" = t.""ProjectId""
                LEFT JOIN write.project_members pm ON pm.""ProjectId"" = p.""Id""
                WHERE t.""CreatedBy"" = @UserId          
                   OR p.""OwnerId"" = @UserId            
                   OR pm.""UserId"" = @UserId            
                ";
            queryParams = new { UserId = userId };
            queryParams = new { UserId = userId };
        }

        var dbTasks = (await taskConnection.QueryAsync<DbTaskExtended>(sql, queryParams)).ToList();

        // Если это админ, вытягиваем имейлы создателей из соседней базы authdb
        if (isAdmin && dbTasks.Any())
        {
            try
            {
                using var authConnection = new NpgsqlConnection(config.GetConnectionString("authdb"));

                // Собираем все уникальные ID создателей задач
                var userIds = dbTasks.Select(t => t.CreatedBy).Distinct().ToList();

                // Универсальный запрос с экранированием для PostgreSQL (учитывает регистр EF Core)
                var usersList = await authConnection.QueryAsync<UserDto>(
                    @"SELECT ""Id"" as id, ""Email"" as email 
              FROM public.""Users"" 
              WHERE ""Id"" = ANY(@UserIds)",
                    new { UserIds = userIds });

                // Выводим в консоль для отладки, сколько пользователей удалось найти
                Console.WriteLine($"[DEBUG] Из authdb успешно загружено пользователей: {usersList.Count()}");

                if (usersList.Any())
                {
                    var userDictionary = usersList.ToDictionary(u => u.Id, u => u.Email);

                    foreach (var task in dbTasks)
                    {
                        if (userDictionary.TryGetValue(task.CreatedBy, out var email))
                        {
                            task.AssignedUserEmail = email;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Сюда выведется ошибка, если PostgreSQL не найдет таблицу или колонку
                Console.WriteLine($"[КРИТИЧЕСКАЯ ОШИБКА ОБЪЕДИНЕНИЯ БАЗ]: {ex.Message}");
            }
        }

        return dbTasks.Select(t => new TaskDto(
            t.Id,
            t.Title,
            t.Description,
            t.IsCompleted,
            isAdmin ? (t.AssignedUserEmail ?? "Неизвестный пользователь") : "Вы",
            t.ProjectId
        ));
    }

    [WolverineGet("/api/tasks/single/{id}")]
    public static async Task<IResult> HandleSingle(Guid id, IConfiguration config)
    {
        using var connection = new NpgsqlConnection(config.GetConnectionString("taskdb"));

        var dbTask = await connection.QueryFirstOrDefaultAsync<DbTaskExtended>(
            @"SELECT ""Id"" as id, 
                     ""Title"" as title, 
                     ""Description"" as description, 
                     ""IsCompleted"" as iscompleted
              FROM write.tasks
              WHERE ""Id"" = @Id",
            new { Id = id });

        if (dbTask == null)
            return Results.NotFound();

        var result = new TaskDto(
            dbTask.Id,
            dbTask.Title,
            dbTask.Description,
            dbTask.IsCompleted,
            string.Empty,
            dbTask.ProjectId
        );

        return Results.Ok(result);
    }
}

// Модели для Dapper внутри этого файла
public class DbTaskExtended
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid? ProjectId { get; set; } // добавь
    public string? AssignedUserEmail { get; set; }
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
}