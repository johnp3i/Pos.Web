using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

[Table("VouchersUsageHistory")]
public partial class VouchersUsageHistory
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime Timestamp { get; set; }

    [Column("VouchersReleasesID")]
    public int VouchersReleasesId { get; set; }

    [Column("UsedByInvoiceID")]
    public int UsedByInvoiceId { get; set; }

    [Column("UsedByCustomerID")]
    public int? UsedByCustomerId { get; set; }

    [ForeignKey("UsedByCustomerId")]
    [InverseProperty("VouchersUsageHistories")]
    public virtual Customer? UsedByCustomer { get; set; }

    [ForeignKey("UsedByInvoiceId")]
    [InverseProperty("VouchersUsageHistories")]
    public virtual Invoice UsedByInvoice { get; set; } = null!;

    [ForeignKey("VouchersReleasesId")]
    [InverseProperty("VouchersUsageHistories")]
    public virtual VouchersRelease VouchersReleases { get; set; } = null!;
}
