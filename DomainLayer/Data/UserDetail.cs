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

    public virtual ICollection<GroupMember> GroupMembers { get; } = new List<GroupMember>();
}
