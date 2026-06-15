namespace Gateway;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Стандартный CORS
        builder.Services.AddCors(options => {
            options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
        });

        // Самый стандартный YARP без наворотов
        builder.Services.AddReverseProxy()
                        .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

        var app = builder.Build();

        app.UseCors("AllowAll");

        app.MapReverseProxy();

        app.Run();
    }
}
