using System;
using System.Collections.Generic;

namespace BackendAppBilling.Models;

public partial class Detail
{
    public int Id { get; set; }

    public int IdBilling { get; set; }

    public int IdProduct { get; set; }

    public int Quantity { get; set; }

    public virtual Billing IdBillingNavigation { get; set; } = null!;

    public virtual Product IdProductNavigation { get; set; } = null!;
}
