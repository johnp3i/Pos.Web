using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class CategoryOperationDepartment
{
    [Key]
    [Column("ID")]
    public byte Id { get; set; }

    [StringLength(50)]
    public string Name { get; set; } = null!;

    public bool IsReceiptPrintEnable { get; set; }

    public int ReceiptMinItemNumberThreshold { get; set; }

    [StringLength(100)]
    public string? PrinterName { get; set; }

    public int? TotalNumberOfPrints { get; set; }

    [InverseProperty("CategoryOperationDepartment")]
    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
}
