using System.Collections.Concurrent;

namespace AnalyticsService.Data
{
    public class AnalyticsRepository
    {
        // Общие счетчики
        public int TotalTasksCreated { get; set; }
        public int TotalTasksCompleted { get; set; }
        public int TotalTasksDeleted { get; set; }

        // Статистика по конкретным пользователям: UserId -> Кол-во активных задач
        public ConcurrentDictionary<string, int> UserTaskCounters { get; } = new();
        public ConcurrentDictionary<string, ConcurrentDictionary<int, int>> UserStatusStats { get; } = new();
    }
}
