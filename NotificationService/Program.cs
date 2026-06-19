using Wolverine;
using Wolverine.Transports.Tcp;

namespace NotificationService;

public class Program
{
    public static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        var builder = WebApplication.CreateBuilder(args);

        // Добавляем поддержку Aspire Service Defaults
        builder.AddServiceDefaults();

        // Настраиваем Wolverine на прослушивание входящих TCP-сообщений
        builder.Host.UseWolverine(opts =>
        {
            // УКАЗЫВАЕМ СБОРКУ, ЧТОБЫ WOLVERINE НАШЕЛ ВАШ TaskNotificationHandler
            opts.ApplicationAssembly = typeof(Program).Assembly;

            opts.ListenAtPort(5005);
            opts.UseRuntimeCompilation();
        });

        var app = builder.Build();

        app.MapDefaultEndpoints();

        app.MapGet("/", () => "Notification Service запущен и слушает шину...");

        app.Run();
    }
}
