using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class CustomerCompany
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Column("AddressID")]
    public int? AddressId { get; set; }

    public bool IsSynched { get; set; }

    [ForeignKey("AddressId")]
    [InverseProperty("CustomerCompanies")]
    public virtual Address? Address { get; set; }

    [InverseProperty("Company")]
    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
}
