using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

[Table("ServerCommandsHistory")]
public partial class ServerCommandsHistory
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Precision(2)]
    public DateTime Timestamp { get; set; }

    [Column("ServerCommandTypeID")]
    public byte ServerCommandTypeId { get; set; }

    [Column("UserID")]
    public int? UserId { get; set; }

    [Column("PosDeviceID")]
    public byte? PosDeviceId { get; set; }

    public bool IsForPendingInvoice { get; set; }

    [Column("ReferenceInvoiceID")]
    public int ReferenceInvoiceId { get; set; }

    [Column("ReferenceItemID")]
    public long? ReferenceItemId { get; set; }

    [Column("ReferenceItemsIDs")]
    public string? ReferenceItemsIds { get; set; }

    public bool IsCommandExecuted { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime? ExecutionTimestamp { get; set; }

    [StringLength(50)]
    public string? PrinterName { get; set; }
}
