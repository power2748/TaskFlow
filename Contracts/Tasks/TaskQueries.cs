using System;
using System.Collections.Generic;
using System.Text;

namespace Contracts.Tasks
{
    public record GetTasksByUser(Guid UserId);
    public record GetTaskById(Guid TaskId);
}
