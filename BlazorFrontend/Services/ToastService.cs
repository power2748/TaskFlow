using BlazorFrontend.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorFrontend.Services
{
    public class ToastService
    {
        public List<ToastMessage> Toasts { get; } = new();

        // События для UI-компонента тостов (ToastContainer)
        public event Action? OnCreate;
        public event Action? OnRemove;

        // НОВОЕ: Событие, на которое будут подписываться страницы (например, список задач)
        public event Action? OnTasksChanged;

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

            // 💡 Проверяем: если прилетело системное уведомление о задачах, 
            // даем знать подписанным страницам, что пора обновить данные
            if (title.Contains("Задача") || title.Contains("статуса") || title.Contains("обновление") || title.Contains("удалена"))
            {
                OnTasksChanged?.Invoke();
            }

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
        public void TriggerTasksChanged()
        {
            OnTasksChanged?.Invoke();
        }

    }
}