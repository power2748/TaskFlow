using BlazorFrontend.Models;

namespace BlazorFrontend.Services
{
    public class ToastService
    {
        public List<ToastMessage> Toasts { get; } = new();

        // Событие, на которое подпишется наш UI-компонент
        public event Action? OnCreate;
        public event Action? OnRemove;

        public void ShowToast(string title, string message, ToastType type = ToastType.Info)
        {
            var toast = new ToastMessage
            {
                Title = title,
                Message = message,
                Type = type
            };

            Toasts.Add(toast);
            OnCreate?.Invoke();

            // Автоматически удаляем уведомление через 5 секунд
            var timer = new System.Threading.Timer(async _ =>
            {
                Toasts.Remove(toast);
                await InvokeAsync(OnRemove);
            }, null, 5000, System.Threading.Timeout.Infinite);
        }

        private async Task InvokeAsync(Action? action)
        {
            if (action != null) action();
        }
    }
}
