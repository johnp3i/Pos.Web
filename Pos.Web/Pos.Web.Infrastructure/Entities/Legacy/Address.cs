using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class Address
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(300)]
    public string Name { get; set; } = null!;

    [Column("DataSourceTypeID")]
    public byte DataSourceTypeId { get; set; }

    [StringLength(300)]
    public string? StreetName { get; set; }

    [StringLength(5)]
    public string? Number { get; set; }

    [StringLength(30)]
    public string? FlatNumber { get; set; }

    [StringLength(10)]
    public string? PostCode { get; set; }

    [StringLength(200)]
    public string? City { get; set; }

    [Column("CountryID")]
    public byte? CountryId { get; set; }

    [Column(TypeName = "decimal(9, 6)")]
    public decimal? Latitude { get; set; }

    [Column(TypeName = "decimal(9, 6)")]
    public decimal? Longitude { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime? LastUpdateTimestamp { get; set; }

    [ForeignKey("CountryId")]
    [InverseProperty("Addresses")]
    public virtual Country? Country { get; set; }

    [InverseProperty("Address")]
    public virtual ICollection<CustomerAddress> CustomerAddresses { get; set; } = new List<CustomerAddress>();

    [InverseProperty("Address")]
    public virtual ICollection<CustomerCompany> CustomerCompanies { get; set; } = new List<CustomerCompany>();

    [InverseProperty("DefaultAddress")]
    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();

    [ForeignKey("DataSourceTypeId")]
    [InverseProperty("Addresses")]
    public virtual DataSourceType DataSourceType { get; set; } = null!;

    [InverseProperty("Address")]
    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    [InverseProperty("Address")]
    public virtual ICollection<PendingInvoice> PendingInvoices { get; set; } = new List<PendingInvoice>();
}
