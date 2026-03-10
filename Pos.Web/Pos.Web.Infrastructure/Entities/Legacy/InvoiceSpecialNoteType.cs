using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class InvoiceSpecialNoteType
{
    [Key]
    [Column("ID")]
    public byte Id { get; set; }

    [StringLength(20)]
    public string Name { get; set; } = null!;

    [StringLength(80)]
    public string NoteDescription { get; set; } = null!;

    public bool IsDisplayInBottom { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    [InverseProperty("InvoiceSpecialNoteType")]
    public virtual ICollection<InvoicesSpecialNote> InvoicesSpecialNotes { get; set; } = new List<InvoicesSpecialNote>();
}
