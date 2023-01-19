using System;
using System.Collections.Generic;

namespace DomainLayer.Data;

public partial class Expense
{
    public int Id { get; set; }

    public int GroupId { get; set; }

    public int? CategoryId { get; set; }

    public string? Description { get; set; }

    public decimal Amount { get; set; }

    public DateTime Date { get; set; }

    public string? FilePath { get; set; }

    public DateTime? AddedOn { get; set; }

    public Guid AddedBy { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public int? UpdatedBy { get; set; }

    public bool? IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public string? ExpenseNumber { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<ExpenseTransaction> ExpenseTransactions { get; } = new List<ExpenseTransaction>();
}
