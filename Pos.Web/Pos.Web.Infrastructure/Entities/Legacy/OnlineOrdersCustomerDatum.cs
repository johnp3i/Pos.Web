using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class OnlineOrdersCustomerDatum
{
    [Key]
    [Column("InvoiceID")]
    public int InvoiceId { get; set; }

    [StringLength(200)]
    public string Name { get; set; } = null!;

    [StringLength(20)]
    public string Telephone { get; set; } = null!;

    public string? Address { get; set; }

    public string? CompanyName { get; set; }
}
