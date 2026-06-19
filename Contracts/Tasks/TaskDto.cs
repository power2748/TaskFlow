using System;
using System.Collections.Generic;
using System.Text;

namespace Contracts.Tasks
{
    public record TaskDto(Guid Id, string Title, string Description, Contracts.Tasks.TaskStatus Status, string AssignedUserEmail, Guid? ProjectId);

}
