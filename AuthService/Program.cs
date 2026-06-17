
using AuthService.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AuthService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddServiceDefaults();
        builder.Services.AddControllers();

        // EF Core + PostgreSQL
        builder.Services.AddDbContext<AuthDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("authdb")));

        // JWT
        var jwtKey = builder.Configuration["Jwt:Key"]!;
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

        builder.Services.AddAuthorization();

        var app = builder.Build();

        app.MapDefaultEndpoints();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        var connStr = builder.Configuration.GetConnectionString("authdb");
        Console.WriteLine($"CONNECTION STRING: {connStr}");
        // Авто-миграция при старте
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

            //for (int i = 0; i < 10; i++)
            //{
                try
                {
                    db.Database.Migrate();
                    Console.WriteLine("Database ready!");
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
