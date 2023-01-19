using System;
using System.Collections.Generic;

namespace DomainLayer.Data;

public partial class ExpenseTransaction
{
    public int Id { get; set; }

    public int ExpensesId { get; set; }

    public int PaidByMemberId { get; set; }

    public int ParticipantMemberId { get; set; }

    public decimal? Amount { get; set; }

    public DateTime AddedOn { get; set; }

    public Guid AddedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool? IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public string? TransactionNumber { get; set; }

    public virtual Expense Expenses { get; set; } = null!;
}
