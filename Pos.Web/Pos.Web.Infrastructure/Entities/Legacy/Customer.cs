using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

[Index("IsTestAccount", Name = "IX_Customers")]
public partial class Customer
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Precision(2)]
    public DateTime RegistrationDate { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string Name { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string? Surname { get; set; }

    [StringLength(300)]
    public string? Email { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? Telephone { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? TelephoneMobile { get; set; }

    [Column("DefaultAddressID")]
    public int? DefaultAddressId { get; set; }

    [StringLength(300)]
    [Unicode(false)]
    public string? Address { get; set; }

    public bool IsCompany { get; set; }

    public bool IsOptOut { get; set; }

    public byte FreeDrinksFactor { get; set; }

    public byte DrinksBuffer { get; set; }

    public int TotalDrinks { get; set; }

    public bool IsAllDrinksFree { get; set; }

    public int Discount { get; set; }

    [Column("CompanyID")]
    public int? CompanyId { get; set; }

    [Column("RegisterByShopID")]
    public int? RegisterByShopId { get; set; }

    public bool IsSynched { get; set; }

    public bool IsTestAccount { get; set; }

    public bool IsCreditAllowed { get; set; }

    public bool IsDeleted { get; set; }

    [Column("DataSourceTypeID")]
    public byte DataSourceTypeId { get; set; }

    public bool IsBusinessCustomer { get; set; }

    [Column("ShopifyCustomerID")]
    public long? ShopifyCustomerId { get; set; }

    [ForeignKey("CompanyId")]
    [InverseProperty("Customers")]
    public virtual CustomerCompany? Company { get; set; }

    [InverseProperty("Customer")]
    public virtual ICollection<CustomerAddress> CustomerAddresses { get; set; } = new List<CustomerAddress>();

    [InverseProperty("Customer")]
    public virtual ICollection<CustomerDiscountHistory> CustomerDiscountHistories { get; set; } = new List<CustomerDiscountHistory>();

    [ForeignKey("DataSourceTypeId")]
    [InverseProperty("Customers")]
    public virtual DataSourceType DataSourceType { get; set; } = null!;

    [ForeignKey("DefaultAddressId")]
    [InverseProperty("Customers")]
    public virtual Address? DefaultAddress { get; set; }

    [InverseProperty("Customer")]
    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    [InverseProperty("Customer")]
    public virtual ICollection<PendingInvoice> PendingInvoices { get; set; } = new List<PendingInvoice>();

    [InverseProperty("Customer")]
    public virtual ICollection<VouchersRelease> VouchersReleases { get; set; } = new List<VouchersRelease>();

    [InverseProperty("UsedByCustomer")]
    public virtual ICollection<VouchersUsageHistory> VouchersUsageHistories { get; set; } = new List<VouchersUsageHistory>();
}
