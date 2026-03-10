using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class DocumentActionType
{
    [Key]
    [Column("ID")]
    public byte Id { get; set; }

    [StringLength(50)]
    public string Name { get; set; } = null!;

    [StringLength(50)]
    public string? PreviewName { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    [InverseProperty("DocumentActionType")]
    public virtual ICollection<DocumentsHistory> DocumentsHistories { get; set; } = new List<DocumentsHistory>();
}
