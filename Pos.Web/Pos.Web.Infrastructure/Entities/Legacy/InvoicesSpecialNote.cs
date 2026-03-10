using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class InvoicesSpecialNote
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("InvoiceID")]
    public int InvoiceId { get; set; }

    [Column("InvoiceSpecialNoteTypeID")]
    public byte InvoiceSpecialNoteTypeId { get; set; }

    [ForeignKey("InvoiceId")]
    [InverseProperty("InvoicesSpecialNotes")]
    public virtual Invoice Invoice { get; set; } = null!;

    [ForeignKey("InvoiceSpecialNoteTypeId")]
    [InverseProperty("InvoicesSpecialNotes")]
    public virtual InvoiceSpecialNoteType InvoiceSpecialNoteType { get; set; } = null!;
}
