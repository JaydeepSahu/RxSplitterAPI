using System;
using System.Collections.Generic;

namespace DomainLayer.Data;

public partial class Summary
{
    public int Id { get; set; }

    public int GroupId { get; set; }

    public int ParticipantId { get; set; }

    public decimal? RemainingAmount { get; set; }

    public Guid AddedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public bool? IsActive { get; set; }

    public bool IsDelete { get; set; }

    public DateTime AddedOn { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual Group Group { get; set; } = null!;

    public virtual GroupMember Participant { get; set; } = null!;
}
