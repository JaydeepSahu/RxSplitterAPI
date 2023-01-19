using System;
using System.Collections.Generic;

namespace DomainLayer.Data;

public partial class Group
{
    public int Id { get; set; }

    public string GroupName { get; set; } = null!;

    public string? GroupImage { get; set; }

    public DateTime AddedOn { get; set; }

    public Guid AddedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool? IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public int? CurrencyId { get; set; }

    public virtual ICollection<GroupMember> GroupMembers { get; } = new List<GroupMember>();

    public virtual ICollection<MemberInvitation> MemberInvitations { get; } = new List<MemberInvitation>();

    public virtual ICollection<SettleUp> SettleUps { get; } = new List<SettleUp>();

    public virtual ICollection<Summary> Summaries { get; } = new List<Summary>();
}
