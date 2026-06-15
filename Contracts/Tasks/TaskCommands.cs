using System;
using System.Collections.Generic;
using System.Text;

namespace Contracts.Tasks
{
    public record CreateTask(string Title, string Description, Guid CreatedBy);
    public record UpdateTaskStatus(Guid TaskId, bool IsCompleted);
    public record DeleteTask(Guid TaskId);
}
