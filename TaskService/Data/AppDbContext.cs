using Microsoft.EntityFrameworkCore;
using TaskService.Models;

namespace TaskService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<TaskItem> Tasks => Set<TaskItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskItem>().ToTable("tasks", schema: "write");
        }
    }
}
