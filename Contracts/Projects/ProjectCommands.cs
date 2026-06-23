using System;
using System.Collections.Generic;
using System.Text;

namespace Contracts.Projects
{
    public record CreateProject(string Name, Guid OwnerId);
    public record DeleteProject(Guid ProjectId, Guid RequestedBy);
    public record AddMemberToProject(Guid ProjectId, string MemberEmail, Guid RequestedBy);
    public record RemoveMemberFromProject(Guid ProjectId, Guid MemberId, Guid RequestedBy);
}
