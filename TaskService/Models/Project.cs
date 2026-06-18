namespace TaskService.Models
{
    public class Project
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid OwnerId { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<ProjectMember> Members { get; set; } = new();
        public List<TaskItem> Tasks { get; set; } = new();
    }
}
