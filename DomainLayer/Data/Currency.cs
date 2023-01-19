using System;
using System.Collections.Generic;

namespace DomainLayer.Data;

public partial class Currency
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Code { get; set; }

    public string? Symbol { get; set; }

    public string? Icon { get; set; }
}
