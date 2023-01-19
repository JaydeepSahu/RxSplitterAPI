using System;
using System.Collections.Generic;

namespace DomainLayer.Data;

public partial class MemberInvitation
{
    public int Id { get; set; }

    public int GroupId { get; set; }

    public int MemberId { get; set; }

    public Guid? TokenGeneratedByUser { get; set; }

    public DateTime? TokenGeneratedOn { get; set; }

    public string? InvitationStatus { get; set; }

    public bool? IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public virtual Group Group { get; set; } = null!;

    public virtual GroupMember Member { get; set; } = null!;

    public virtual UserDetail? TokenGeneratedByUserNavigation { get; set; }
}
