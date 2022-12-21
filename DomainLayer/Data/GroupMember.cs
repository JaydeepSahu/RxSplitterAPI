using System;
using System.Collections.Generic;

namespace DomainLayer.Data;

public partial class GroupMember
{
    public int Id { get; set; }

    public int GroupId { get; set; }

    public Guid? UserId { get; set; }

    public string Email { get; set; } = null!;

    public DateTime AddedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public bool? IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Group Group { get; set; } = null!;

    public virtual UserDetail? User { get; set; }
}
