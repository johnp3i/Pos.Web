using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

[Table("VATs")]
public partial class Vat
{
    [Key]
    [Column("ID")]
    public byte Id { get; set; }

    public DateOnly ActivationDate { get; set; }

    [Column(TypeName = "decimal(9, 2)")]
    public decimal Value { get; set; }

    [StringLength(50)]
    public string? Description { get; set; }

    [StringLength(100)]
    public string? PreviewTextValue { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDate { get; set; }

    [InverseProperty("Vat")]
    public virtual ICollection<CategoryItem> CategoryItems { get; set; } = new List<CategoryItem>();

    [InverseProperty("Vat")]
    public virtual ICollection<InvoiceVatanalysis> InvoiceVatanalyses { get; set; } = new List<InvoiceVatanalysis>();

    [InverseProperty("Vat")]
    public virtual ICollection<ServingTypesToVat> ServingTypesToVats { get; set; } = new List<ServingTypesToVat>();
}
