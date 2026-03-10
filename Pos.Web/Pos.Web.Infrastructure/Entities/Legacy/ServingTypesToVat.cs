using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

[Table("ServingTypesToVAT")]
public partial class ServingTypesToVat
{
    [Key]
    [Column("ID")]
    public byte Id { get; set; }

    [Column("ServingTypeID")]
    public byte ServingTypeId { get; set; }

    [Column("VATID")]
    public byte Vatid { get; set; }

    [Precision(2)]
    public DateTime Timestamp { get; set; }

    [InverseProperty("ServingTypeToVat")]
    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    [ForeignKey("ServingTypeId")]
    [InverseProperty("ServingTypesToVats")]
    public virtual ServingType ServingType { get; set; } = null!;

    [ForeignKey("Vatid")]
    [InverseProperty("ServingTypesToVats")]
    public virtual Vat Vat { get; set; } = null!;
}
