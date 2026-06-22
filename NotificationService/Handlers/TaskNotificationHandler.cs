using Contracts.Events;
using Microsoft.Extensions.Logging;

namespace NotificationService.Handlers
{
    public class TaskNotificationHandler
    {
        private readonly ILogger<TaskNotificationHandler> _logger;

        public TaskNotificationHandler(ILogger<TaskNotificationHandler> logger)
        {
            _logger = logger;
        }

        // Этот метод автоматически вызовется, как только TaskService сделает bus.PublishAsync!
        public void Handle(TaskAssigned message)
        {
            // Имитируем отправку уведомления. В будущем здесь будет SignalR для фронтенда или SMTP клиент.
            _logger.LogInformation("========================================================");
            _logger.LogInformation("🔔 [НОВОЕ УВЕДОМЛЕНИЕ ДЛЯ ПОЛЬЗОВАТЕЛЯ {UserId}]:", message.AssigneeId);
            _logger.LogInformation("Вам назначена новая задача: \"{TaskTitle}\" (ID: {TaskId})", message.TaskTitle, message.TaskId);
            _logger.LogInformation("========================================================");
        }

        public void Handle(TaskUpdated message)
        {
            _logger.LogInformation("========================================================");
            _logger.LogInformation("✏️ [ОБНОВЛЕНИЕ] Задача (ID: {TaskId}) изменена пользователем {UserId}!", message.TaskId, message.AssigneeId);
            _logger.LogInformation("Старое название: \"{OldTitle}\" -> Новое: \"{NewTitle}\"", message.OldTitle, message.NewTitle);
            _logger.LogInformation("========================================================");
        }

        public void Handle(TaskStatusUpdated message)
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
        }

        public void Handle(TaskDeleted message)
        {
            _logger.LogInformation("========================================================");
            _logger.LogInformation("🗑️ [УДАЛЕНИЕ] Задача \"{TaskTitle}\" (ID: {TaskId}) была удалена!", message.TaskTitle, message.TaskId);
            _logger.LogInformation("Уведомление отправлено бывшему исполнителю: {UserId}", message.AssigneeId);
            _logger.LogInformation("========================================================");
        }
    }
}
