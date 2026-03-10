using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class CustomerAddress
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime EnrollmentTimestamp { get; set; }

    [StringLength(50)]
    public string? ShortName { get; set; }

    [Column("AddressID")]
    public int AddressId { get; set; }

    [Column("CustomerID")]
    public int CustomerId { get; set; }

    public bool IsActive { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime? LastDeactivationDate { get; set; }

    [ForeignKey("AddressId")]
    [InverseProperty("CustomerAddresses")]
    public virtual Address Address { get; set; } = null!;

    [ForeignKey("CustomerId")]
    [InverseProperty("CustomerAddresses")]
    public virtual Customer Customer { get; set; } = null!;
}
