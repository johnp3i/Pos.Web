using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

[Table("JCCTransactionsHistory")]
public partial class JcctransactionsHistory
{
    /// <summary>
    /// This is the system id, that is sent to jcc client service.
    /// </summary>
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Precision(2)]
    public DateTime Timestamp { get; set; }

    [Column("InvoiceID")]
    public int? InvoiceId { get; set; }

    [Column("PluginID")]
    public byte PluginId { get; set; }

    [Column("JCCPluginTypeID")]
    public byte JccpluginTypeId { get; set; }

    [Column(TypeName = "decimal(9, 2)")]
    public decimal Amount { get; set; }

    [StringLength(50)]
    public string? TransactionType { get; set; }

    [StringLength(50)]
    public string? ResultStatus { get; set; }

    [StringLength(50)]
    public string? ErrorCode { get; set; }

    [StringLength(50)]
    public string? TerminalNo { get; set; }

    [StringLength(50)]
    public string? BatchNo { get; set; }

    [StringLength(50)]
    public string? ResponseCode { get; set; }

    [StringLength(50)]
    public string? AccountNumber { get; set; }

    [ForeignKey("InvoiceId")]
    [InverseProperty("JcctransactionsHistories")]
    public virtual Invoice? Invoice { get; set; }
}
