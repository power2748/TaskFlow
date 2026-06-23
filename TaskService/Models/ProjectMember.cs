namespace TaskService.Models
{
    public class ProjectMember
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public Guid UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;

        public Project Project { get; set; } = null!;
    }
}
