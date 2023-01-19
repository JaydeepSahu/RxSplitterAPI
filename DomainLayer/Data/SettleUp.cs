using System;
using System.Collections.Generic;

namespace DomainLayer.Data;

public partial class SettleUp
{
    public int Id { get; set; }

    public int GroupId { get; set; }

    public int PayerMemberId { get; set; }

    public int RecipientMemberId { get; set; }

    public decimal? Amount { get; set; }

    public DateTime SettleupDate { get; set; }

    public string? FilePath { get; set; }

    public Guid AddedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public bool? IsActive { get; set; }

    public bool IsDelete { get; set; }

    public virtual Group Group { get; set; } = null!;

    public virtual GroupMember PayerMember { get; set; } = null!;

    public virtual GroupMember RecipientMember { get; set; } = null!;
}
