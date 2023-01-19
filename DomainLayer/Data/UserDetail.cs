using System;
using System.Collections.Generic;

namespace DomainLayer.Data;

public partial class UserDetail
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public DateTime AddedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public bool? IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public string? PhoneNumber { get; set; }

    public DateTime? Dob { get; set; }

    public string? ProfileImage { get; set; }

    public virtual ICollection<GroupMember> GroupMembers { get; } = new List<GroupMember>();

    public virtual ICollection<MemberInvitation> MemberInvitations { get; } = new List<MemberInvitation>();
}
