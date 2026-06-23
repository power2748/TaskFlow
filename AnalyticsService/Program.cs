using AnalyticsService.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Concurrent;
using System.Text;
using Wolverine;
using Wolverine.Transports.Tcp;
using Npgsql;
using Dapper;

namespace AnalyticsService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        var jwtKey = builder.Configuration["Jwt:Key"] ?? "super-secret-key-minimum-32-characters!!";

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true
                };
            });
        // Add services to the container.
        builder.Services.AddAuthorization();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        builder.Services.AddSingleton<AnalyticsRepository>();

        builder.Host.UseWolverine(opts =>
        {
            // УКАЗЫВАЕМ СБОРКУ, ЧТОБЫ WOLVERINE НАШЕЛ ВАШ TaskNotificationHandler
            opts.ApplicationAssembly = typeof(Program).Assembly;

            opts.ListenAtPort(5007);
            opts.UseRuntimeCompilation();
        });

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            });
        });

        var app = builder.Build();
        app.UseCors();

        app.MapDefaultEndpoints();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapGet("/api/admin/dashboard", async (HttpContext context, AnalyticsService.Data.AnalyticsRepository repo, IConfiguration config) =>
        {
            if (!context.Request.Headers.TryGetValue("X-User-Id", out var userIdHeader) ||
                !Guid.TryParse(userIdHeader, out Guid currentUserId))
            {
                return Results.BadRequest("Missing or invalid X-User-Id header.");
            }

            Guid adminId = Guid.Parse("c1d8ff5d-c728-4e9a-b546-57cd113d034b");
            bool isAdmin = currentUserId == adminId;

            if (isAdmin)
            {
                // 1. Собираем все уникальные UserId, по которым у нас накопилась статистика
                var userIds = repo.UserStatusStats.Keys
                    .Select(idStr => Guid.TryParse(idStr, out var g) ? g : Guid.Empty)
                    .Where(g => g != Guid.Empty)
                    .ToList();

                var userDictionary = new Dictionary<Guid, string>();

                // 2. Если статистика не пустая, стягиваем имейлы из authdb
                if (userIds.Any())
                {
                    try
                    {
                        using var authConnection = new NpgsqlConnection(config.GetConnectionString("authdb"));

                        var usersList = await authConnection.QueryAsync<UserDto>(
                            @"SELECT ""Id"" as id, ""Email"" as email 
                      FROM public.""Users"" 
                      WHERE ""Id"" = ANY(@UserIds)",
                            new { UserIds = userIds });

                        if (usersList.Any())
                        {
                            userDictionary = usersList.ToDictionary(u => u.Id, u => u.Email);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ОШИБКА ЗАГРУЗКИ EMAIL ДЛЯ АНАЛИТИКИ]: {ex.Message}");
                    }
                }

                // 3. Формируем отчет, подставляя Email вместо ID
                var report = repo.UserStatusStats.Select(u =>
                {
                    // Пытаемся найти email в словаре. Если не нашли (или это старый мусорный ID) — оставляем исходный ID
                    string userDisplayName = Guid.TryParse(u.Key, out var g) && userDictionary.TryGetValue(g, out var email)
                        ? email
                        : u.Key;

                    return new
                    {
                        UserId = userDisplayName, // Теперь здесь будет лежать email!
                        Todo = u.Value.GetValueOrDefault(0, 0),
                        InProgress = u.Value.GetValueOrDefault(1, 0),
                        CodeReview = u.Value.GetValueOrDefault(2, 0),
                        Done = u.Value.GetValueOrDefault(3, 0)
                    };
                }).ToList();

                return Results.Ok(new
                {
                    IsAdminView = true,
                    TotalCreated = repo.TotalTasksCreated,
                    TotalDone = repo.TotalTasksCompleted,
                    TotalDeleted = repo.TotalTasksDeleted,
                    ActiveCount = repo.TotalTasksCreated - repo.TotalTasksCompleted - repo.TotalTasksDeleted,
                    UserLoadReport = report
                });
            }
            else
            {
                // Для обычного пользователя оставляем заглушку "Моя статистика", её переименовывать не нужно
                var userIdStr = currentUserId.ToString();
                var userStats = repo.UserStatusStats.GetValueOrDefault(userIdStr)
                                ?? new System.Collections.Concurrent.ConcurrentDictionary<int, int>();

                int uTodo = userStats.GetValueOrDefault(0, 0);
                int uInProgress = userStats.GetValueOrDefault(1, 0);
                int uCodeReview = userStats.GetValueOrDefault(2, 0);
                int uDone = userStats.GetValueOrDefault(3, 0);
                int uCreated = uTodo + uInProgress + uCodeReview + uDone;

                return Results.Ok(new
                {
                    IsAdminView = false,
                    TotalCreated = uCreated,
                    ActiveCount = uTodo + uInProgress + uCodeReview,
                    TotalDone = uDone,
                    TotalDeleted = 0,
                    UserLoadReport = new List<object>
            {
                new { UserId = "Моя статистика", Todo = uTodo, InProgress = uInProgress, CodeReview = uCodeReview, Done = uDone }
            }
                });
            }
        });

        app.Run();
    }

    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
    }
}
