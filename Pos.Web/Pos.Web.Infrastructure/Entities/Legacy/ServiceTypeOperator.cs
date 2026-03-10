using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class ServiceTypeOperator
{
    [Key]
    [Column("ID")]
    public byte Id { get; set; }

    [StringLength(50)]
    public string Name { get; set; } = null!;

    [StringLength(50)]
    public string DisplayName { get; set; } = null!;

    [Column("ServingTypeID")]
    public byte ServingTypeId { get; set; }

    [Column("ColorTypeID")]
    public byte ColorTypeId { get; set; }

    public bool IsActive { get; set; }

    [StringLength(20)]
    public string? WorkPhone { get; set; }

    [StringLength(20)]
    public string? PersonalPhone { get; set; }

    public byte DisplayOrder { get; set; }

    [ForeignKey("ColorTypeId")]
    [InverseProperty("ServiceTypeOperators")]
    public virtual ColorsType ColorType { get; set; } = null!;

    [InverseProperty("OrderServiceTypeOperator")]
    public virtual ICollection<InvoiceOrderProcessingHistory> InvoiceOrderProcessingHistories { get; set; } = new List<InvoiceOrderProcessingHistory>();

    [ForeignKey("ServingTypeId")]
    [InverseProperty("ServiceTypeOperators")]
    public virtual ServingType ServingType { get; set; } = null!;
}
