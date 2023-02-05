using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.OpenApi.Any;
using System;
using System.Collections.Generic;

namespace BackendAppBilling.Models;

public partial class BillingCreate
{
    public int idCustomer { get; set; }
    public List<ProductInfo> idsProducts { get; set; }
}

public class ProductInfo
{
    public int id { get; set; }
    public int quantity { get; set; }
}