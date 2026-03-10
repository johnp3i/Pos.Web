using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class DocumentType
{
    [Key]
    [Column("ID")]
    public byte Id { get; set; }

    [StringLength(20)]
    public string Name { get; set; } = null!;

    [StringLength(20)]
    public string? PreviewName { get; set; }

    [StringLength(200)]
    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    /// <summary>
    /// This value determines whether the JDS service processes the specified document type.
    /// </summary>
    public bool IsActive { get; set; }

    [InverseProperty("DocumentType")]
    public virtual ICollection<DocumentsHistory> DocumentsHistories { get; set; } = new List<DocumentsHistory>();
}
