using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

[Table("CategoryItemsShortcutToolbar")]
public partial class CategoryItemsShortcutToolbar
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime Timestamp { get; set; }

    [Column("CategoryItemID")]
    public int CategoryItemId { get; set; }

    [StringLength(50)]
    public string? Name { get; set; }

    [StringLength(5)]
    public string ShortcutName { get; set; } = null!;

    public int DisplayOrder { get; set; }

    public string? ImageUri { get; set; }

    public bool IsPrintedAtEnd { get; set; }

    public bool IsActive { get; set; }

    [ForeignKey("CategoryItemId")]
    [InverseProperty("CategoryItemsShortcutToolbars")]
    public virtual CategoryItem CategoryItem { get; set; } = null!;
}
