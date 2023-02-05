using System;
using System.Collections.Generic;

namespace BackendAppBilling.Models;

public partial class Billing
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int IdCustomer { get; set; }

    public virtual ICollection<Detail> Details { get; } = new List<Detail>();

    public virtual Customer IdCustomerNavigation { get; set; } = null!;
}
