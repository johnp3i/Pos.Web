using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

[Table("POSDevices")]
public partial class Posdevice
{
    [Key]
    [Column("ID")]
    public byte Id { get; set; }

    [Column("IPV4")]
    [StringLength(15)]
    [Unicode(false)]
    public string Ipv4 { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string Name { get; set; } = null!;

    [StringLength(128)]
    public string AuthKey { get; set; } = null!;

    [Column("DeviceID")]
    [StringLength(150)]
    public string? DeviceId { get; set; }

    public bool IsWindowsKeyboardActive { get; set; }

    public bool IsActive { get; set; }

    [Column("DeviceHardwareRefID")]
    public string? DeviceHardwareRefId { get; set; }

    public string? DeviceLicense { get; set; }

    [StringLength(50)]
    public string? PrinterName { get; set; }

    [Column("IsVFDActive")]
    public bool IsVfdactive { get; set; }

    [Column("VFDPort")]
    [StringLength(50)]
    public string? Vfdport { get; set; }

    [Column("VFDBaudRate")]
    public int? VfdbaudRate { get; set; }

    [Column("VFDMessage")]
    [StringLength(200)]
    public string? Vfdmessage { get; set; }

    [Column("JCCTerminalID")]
    public byte? JccterminalId { get; set; }

    [Column("JCCPluginTypeID")]
    public byte? JccpluginTypeId { get; set; }

    [InverseProperty("Posdevice")]
    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    [ForeignKey("JccpluginTypeId")]
    [InverseProperty("Posdevices")]
    public virtual JccpluginType? JccpluginType { get; set; }

    [InverseProperty("Posdevice")]
    public virtual ICollection<PendingInvoice> PendingInvoices { get; set; } = new List<PendingInvoice>();
}
