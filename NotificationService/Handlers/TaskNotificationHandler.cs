using Contracts.Events;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NotificationService.Hubs;

namespace NotificationService.Handlers
{
    public class TaskNotificationHandler
    {
        private readonly ILogger<TaskNotificationHandler> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;

        public TaskNotificationHandler(ILogger<TaskNotificationHandler> logger, IHubContext<NotificationHub> hubContext)
        {
            _logger = logger;
            _hubContext = hubContext;
        }

        public async Task Handle(TaskAssigned message)
        {
            _logger.LogInformation("========================================================");
            _logger.LogInformation("🔔 [НОВОЕ УВЕДОМЛЕНИЕ ДЛЯ ПОЛЬЗОВАТЕЛЯ {UserId}]:", message.AssigneeId);
            _logger.LogInformation("Вам назначена новая задача: \"{TaskTitle}\" (ID: {TaskId})", message.TaskTitle, message.TaskId);
            _logger.LogInformation("========================================================");

            // ОТПРАВКА НА ФРОНТЕНД: отправляем в группу конкретного пользователя
            string targetUserId = message.AssigneeId.ToString();
            await _hubContext.Clients.Group(targetUserId).SendAsync(
                "ReceiveNotification",
                "Новая задача!",
                $"Вам назначена задача: \"{message.TaskTitle}\""
            );
        }

        public async Task Handle(TaskUpdated message)
        {
            _logger.LogInformation("========================================================");
            _logger.LogInformation("✏️ [ОБНОВЛЕНИЕ] Задача (ID: {TaskId}) изменена пользователем {UserId}!", message.TaskId, message.AssigneeId);
            _logger.LogInformation("Старое название: \"{OldTitle}\" -> Новое: \"{NewTitle}\"", message.OldTitle, message.NewTitle);
            _logger.LogInformation("========================================================");

            string targetUserId = message.AssigneeId.ToString();
            await _hubContext.Clients.Group(targetUserId).SendAsync(
                "ReceiveNotification",
                "Задача обновлена",
                $"Название изменено: \"{message.NewTitle}\""
            );
        }

        public async Task Handle(TaskStatusUpdated message)
        {
            string GetStatusName(int status) => status switch
            {
                0 => "TODO",
                1 => "In progress",
                2 => "Code review",
                3 => "Done"
            };

            _logger.LogInformation("========================================================");
            _logger.LogInformation("🔄 [СМЕНА СТАТУСА] Задача {TaskId} изменила свой этап!", message.TaskId);
            _logger.LogInformation("Статус: \"{OldStatus}\" ➡️ \"{NewStatus}\"",
                GetStatusName(message.OldStatus), GetStatusName(message.NewStatus));
            _logger.LogInformation("Уведомление ушло пользователю: {UserId}", message.AssigneeId);
            _logger.LogInformation("========================================================");

            string targetUserId = message.AssigneeId.ToString();
            await _hubContext.Clients.Group(targetUserId).SendAsync(
                "ReceiveNotification",
                "Смена статуса задачи",
                $"Задача переведена на этап: \"{GetStatusName(message.NewStatus)}\""
            );
        }

        public async Task Handle(TaskDeleted message)
        {
            _logger.LogInformation("========================================================");
            _logger.LogInformation("🗑️ [УДАЛЕНИЕ] Задача \"{TaskTitle}\" (ID: {TaskId}) была удалена!", message.TaskTitle, message.TaskId);
            _logger.LogInformation("Уведомление отправлено бывшему исполнителю: {UserId}", message.AssigneeId);
            _logger.LogInformation("========================================================");

            string targetUserId = message.AssigneeId.ToString();
            await _hubContext.Clients.Group(targetUserId).SendAsync(
                "ReceiveNotification",
                "Задача удалена",
                $"Админ удалил задачу: \"{message.TaskTitle}\""
            );
        }
    }
}
