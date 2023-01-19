using System;
using System.Collections.Generic;

namespace DomainLayer.Data;

public partial class Transaction
{
    public int Id { get; set; }

    public int PaidFromMemberId { get; set; }

    public int PaidToMemberId { get; set; }

    public decimal Amount { get; set; }

    public string Status { get; set; } = null!;

    public bool? IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public int AddedBy { get; set; }

    public DateTime AddedOn { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }
}
