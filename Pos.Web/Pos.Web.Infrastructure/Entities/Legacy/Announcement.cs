using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

public partial class Announcement
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column(TypeName = "smalldatetime")]
    public DateTime Timestamp { get; set; }

    [StringLength(50)]
    public string DisplayName { get; set; } = null!;

    [StringLength(50)]
    public string? Header { get; set; }

    [StringLength(50)]
    public string? Footer { get; set; }

    [StringLength(50)]
    public string? Title { get; set; }

    [StringLength(50)]
    public string? Subtitle { get; set; }

    public string? ImageUri { get; set; }

    public string? AnnouncementText { get; set; }

    [StringLength(200)]
    public string? AvailableServicesJson { get; set; }

    public bool IsShopHeaderVisible { get; set; }

    public bool IsAutoPrinterOnCheckout { get; set; }

    public bool IsActive { get; set; }
}
