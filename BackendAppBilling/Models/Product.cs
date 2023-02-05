using System;
using System.Collections.Generic;

namespace BackendAppBilling.Models;

public partial class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int Stock { get; set; }

    public int Price { get; set; }

    public virtual ICollection<Detail> Details { get; } = new List<Detail>();
}
