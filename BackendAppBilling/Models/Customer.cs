using System;
using System.Collections.Generic;

namespace BackendAppBilling.Models;

public partial class Customer
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int Age { get; set; }

    public virtual ICollection<Billing> Billings { get; } = new List<Billing>();
}
