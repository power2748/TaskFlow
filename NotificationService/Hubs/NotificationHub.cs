using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace NotificationService.Hubs
{
    public class NotificationHub : Hub
    {
        // При подключении фронтенд передает свой UserId в query-строке
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var userId = httpContext?.Request.Query["userId"].ToString();

            if (!string.IsNullOrEmpty(userId))
            {
                // Добавляем пользователя в группу, названную его UserId
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
                Console.WriteLine($"[SIGNALR] Пользователь {userId} подключился к хабу уведомлений.");
            }

            await base.OnConnectedAsync();
        }
    }
}
