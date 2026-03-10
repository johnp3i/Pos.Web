using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

[Table("DocumentsHistory")]
public partial class DocumentsHistory
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime Timestamp { get; set; }

    [Column("UserID")]
    public int? UserId { get; set; }

    [Column("DocumentRefID")]
    public int DocumentRefId { get; set; }

    [Column("DocumentActionTypeID")]
    public byte DocumentActionTypeId { get; set; }

    [Column("DocumentTypeID")]
    public byte DocumentTypeId { get; set; }

    public string? CategoryItemsUpdatesJson { get; set; }

    public bool IsCancelledByAdmin { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }

    [ForeignKey("DocumentActionTypeId")]
    [InverseProperty("DocumentsHistories")]
    public virtual DocumentActionType DocumentActionType { get; set; } = null!;

    [ForeignKey("DocumentTypeId")]
    [InverseProperty("DocumentsHistories")]
    public virtual DocumentType DocumentType { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("DocumentsHistories")]
    public virtual User? User { get; set; }
}
