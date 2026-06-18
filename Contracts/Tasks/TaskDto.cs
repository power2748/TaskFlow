using System;
using System.Collections.Generic;
using System.Text;

namespace Contracts.Tasks
{
    public record TaskDto(Guid Id, string Title, string Description, bool IsCompleted, string AssignedUserEmail, Guid? ProjectId);
}
