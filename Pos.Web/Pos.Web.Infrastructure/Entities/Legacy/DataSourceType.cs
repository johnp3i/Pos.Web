using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class DataSourceType
{
    [Key]
    [Column("ID")]
    public byte Id { get; set; }

    [StringLength(50)]
    public string Description { get; set; } = null!;

    public string? IconFileName { get; set; }

    [InverseProperty("DataSourceType")]
    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();

    [InverseProperty("DataSourceType")]
    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();

    [InverseProperty("DataSourceType")]
    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    [InverseProperty("DataSourceType")]
    public virtual ICollection<InvoicesPaymentActionHistory> InvoicesPaymentActionHistories { get; set; } = new List<InvoicesPaymentActionHistory>();

    [InverseProperty("DataSourceType")]
    public virtual ICollection<OnlineOrdersHistory> OnlineOrdersHistories { get; set; } = new List<OnlineOrdersHistory>();

    [InverseProperty("DataSourceType")]
    public virtual ICollection<PendingInvoice> PendingInvoices { get; set; } = new List<PendingInvoice>();

    [InverseProperty("DataSourceType")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
