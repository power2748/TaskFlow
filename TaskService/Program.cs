
using Contracts.Tasks;
using JasperFx.CodeGeneration.Model;
using Microsoft.EntityFrameworkCore;
using TaskService.Data;
using TaskService.Handlers;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Http;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace TaskService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddServiceDefaults();

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("taskdb")));

        builder.Host.UseWolverine(opts =>
        {
            opts.ApplicationAssembly = typeof(Program).Assembly;
            opts.Policies.AutoApplyTransactions();
            opts.CodeGeneration.AlwaysUseServiceLocationFor<TaskService.Data.AppDbContext>();
            //opts.UseEntityFrameworkCoreTransactions();
        });
        builder.Services.AddWolverineHttp();
        var jwtKey = builder.Configuration["Jwt:Key"] ?? "super-secret-key-minimum-32-characters!!";

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                        ValidateIssuer = false,   // Отключаем строгую проверку издателя
                        ValidateAudience = false, // Отключаем строгую проверку аудитории
                        ValidateLifetime = true   // Проверяем только то, что срок годности не истек
                    };
            });

        builder.Services.AddAuthorization();

        var app = builder.Build();
        app.MapDefaultEndpoints();
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.MapWolverineEndpoints(); // автоматически регистрирует все [WolverinePost/Get]

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            for (int i = 0; i < 10; i++)
            {
                try
                {
                    db.Database.Migrate();
                    Console.WriteLine("TaskService DB ready!");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DB attempt {i + 1} failed: {ex.Message}");
                    Thread.Sleep(3000);
                }
            }
        }

        app.Run();
    }
}
