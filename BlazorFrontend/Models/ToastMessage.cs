namespace BlazorFrontend.Models
{
    public class ToastMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public ToastType Type { get; set; } = ToastType.Info;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public enum ToastType
    {
        Info,
        Success,
        Warning,
        Error
    }
}
