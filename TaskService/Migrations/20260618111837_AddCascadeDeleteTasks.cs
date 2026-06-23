using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskService.Migrations
{
    /// <inheritdoc />
    public partial class AddCascadeDeleteTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tasks_projects_ProjectId",
                schema: "write",
                table: "tasks");

            migrationBuilder.AddForeignKey(
                name: "FK_tasks_projects_ProjectId",
                schema: "write",
                table: "tasks",
                column: "ProjectId",
                principalSchema: "write",
                principalTable: "projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tasks_projects_ProjectId",
                schema: "write",
                table: "tasks");

            migrationBuilder.AddForeignKey(
                name: "FK_tasks_projects_ProjectId",
                schema: "write",
                table: "tasks",
                column: "ProjectId",
                principalSchema: "write",
                principalTable: "projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
