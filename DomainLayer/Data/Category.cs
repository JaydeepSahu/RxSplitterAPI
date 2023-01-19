using System;
using System.Collections.Generic;

namespace DomainLayer.Data;

public partial class Category
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public DateTime AddedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public bool? IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<Expense> Expenses { get; } = new List<Expense>();
}
