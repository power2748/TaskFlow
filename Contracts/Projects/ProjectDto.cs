using System;
using System.Collections.Generic;
using System.Text;

namespace Contracts.Projects
{
    public record ProjectDto(Guid Id, string Name, Guid OwnerId, List<ProjectMemberDto> Members);
    public record ProjectMemberDto(Guid UserId, string Email);
}
