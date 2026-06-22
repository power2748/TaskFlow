using AnalyticsService.Data;
using Contracts.Events;
using System.Collections.Concurrent;

namespace AnalyticsService.Handlers
{
    public class TaskAnalyticsHandler
    {
        private readonly AnalyticsRepository _repo;
        private readonly ILogger<TaskAnalyticsHandler> _logger;

        public TaskAnalyticsHandler(AnalyticsRepository repo, ILogger<TaskAnalyticsHandler> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public void Handle(TaskAssigned message)
        {
            _repo.TotalTasksCreated++;

            var userStats = _repo.UserStatusStats.GetOrAdd(message.AssigneeId.ToString(), _ => new ConcurrentDictionary<int, int>());
            userStats.AddOrUpdate(0, 1, (_, current) => current + 1); // По умолчанию падает в статус 0 (TODO)

            _logger.LogInformation("📊 [АНАЛИТИКА] Создана новая задача. Всего создано: {Total}", _repo.TotalTasksCreated);
        }

        public void Handle(TaskStatusUpdated message)
        {
            var userStats = _repo.UserStatusStats.GetOrAdd(message.AssigneeId, _ => new ConcurrentDictionary<int, int>());

            // Снижаем счетчик старого статуса
            userStats.AddOrUpdate(message.OldStatus, 0, (_, current) => Math.Max(0, current - 1));
            // Наращиваем счетчик нового статуса
            userStats.AddOrUpdate(message.NewStatus, 1, (_, current) => current + 1);

            // Если перевели в финальный статус Done (3)
            if (message.NewStatus == 3 && message.OldStatus != 3)
            {
                _repo.TotalTasksCompleted++;
            }
            // Если вернули задачу из Done назад в работу
            else if (message.OldStatus == 3 && message.NewStatus != 3)
            {
                _repo.TotalTasksCompleted = Math.Max(0, _repo.TotalTasksCompleted - 1);
            }

            _logger.LogInformation("📊 [АНАЛИТИКА] Статус изменен. Выполнено за всё время: {Done}", _repo.TotalTasksCompleted);
        }

        public void Handle(TaskDeleted message)
        {
            _repo.TotalTasksDeleted++;

            // Убираем задачу из TODO (или любого активного статуса) пользователя при физическом удалении
            if (_repo.UserStatusStats.TryGetValue(message.AssigneeId, out var userStats))
            {
                // Здесь для точности можно хранить маппинг TaskId -> Status, но для общей аналитики просто снижаем TODO/In Progress
                userStats.AddOrUpdate(0, 0, (_, current) => Math.Max(0, current - 1));
            }

            _logger.LogInformation("📊 [АНАЛИТИКА] Задача удалена. Всего удалено: {Total}", _repo.TotalTasksDeleted);
        }
    }
}


