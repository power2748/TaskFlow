using System;
using System.Collections.Generic;
using System.Text;

namespace Contracts.Events
{
    public record TaskAssigned(Guid TaskId, Guid AssigneeId, string TaskTitle);
    public record TaskUpdated(Guid TaskId, string AssigneeId, string OldTitle, string NewTitle);
    public record TaskStatusUpdated(Guid TaskId, string AssigneeId, int OldStatus, int NewStatus);
    public record TaskDeleted(Guid TaskId, string AssigneeId, string TaskTitle);
}
