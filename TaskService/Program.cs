
using Contracts.Events;
using Contracts.Tasks;
using ImTools;
using JasperFx.CodeGeneration.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TaskService.Data;
using TaskService.Handlers;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Http;
using Wolverine.Transports.Tcp;

namespace TaskService;

public class Program
{
    public static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        var builder = WebApplication.CreateBuilder(args);

        builder.AddServiceDefaults();

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("taskdb")));

        builder.Host.UseWolverine(opts =>
        {
            opts.ApplicationAssembly = typeof(Program).Assembly;
            //opts.Policies.AutoApplyTransactions();
            opts.CodeGeneration.AlwaysUseServiceLocationFor<TaskService.Data.AppDbContext>();
            //opts.PublishMessage<TaskAssigned>().To(new Uri("tcp://127.0.0.1:5005"));
            //opts.UseEntityFrameworkCoreTransactions();
            var endpointUri = builder.Configuration["Endpoint:notificationservice:wolverine-tcp"];

            if (!string.IsNullOrEmpty(endpointUri))
            {
                // Aspire может вернуть адрес в виде "tcp://localhost:5005" или "tcp://127.0.0.1:5005"
                opts.PublishMessage<TaskAssigned>().To(new Uri(endpointUri));
                opts.PublishMessage<TaskUpdated>().To(new Uri(endpointUri));
                opts.PublishMessage<TaskDeleted>().To(new Uri(endpointUri));
            }
            else
            {
                // Резервный адрес для локального запуска без оркестратора
                opts.PublishMessage<TaskAssigned>().To(new Uri("tcp://127.0.0.1:5005"));
                opts.PublishMessage<TaskUpdated>().To(new Uri("tcp://127.0.0.1:5005"));
                opts.PublishMessage<TaskDeleted>().To(new Uri("tcp://127.0.0.1:5005"));
            }
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

            //for (int i = 0; i < 10; i++)
            //{
                try
                {
                    db.Database.Migrate();
                    Console.WriteLine("TaskService DB ready!");
                    //break;
                }
                catch (Exception ex)
                {
                    //Console.WriteLine($"DB attempt {i + 1} failed: {ex.Message}");
                    Thread.Sleep(3000);
                }
            //}
        }

        app.Run();
    }
}
