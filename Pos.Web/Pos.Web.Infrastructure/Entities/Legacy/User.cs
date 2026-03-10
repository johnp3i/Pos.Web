using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Pos.Web.Infrastructure.Entities.Legacy;

[Index("Code", Name = "IX_Users", IsUnique = true)]
public partial class User
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    public int? Code { get; set; }

    [StringLength(50)]
    public string Name { get; set; } = null!;

    [StringLength(50)]
    public string? Surname { get; set; }

    [Column("PositionTypeID")]
    public byte PositionTypeId { get; set; }

    [StringLength(20)]
    public string Password { get; set; } = null!;

    public bool IsActive { get; set; }

    [Precision(2)]
    public DateTime RegistrationDate { get; set; }

    [Column("ColorTypeID")]
    public byte ColorTypeId { get; set; }

    public byte DisplayOrder { get; set; }

    [Column("DataSourceTypeID")]
    public byte? DataSourceTypeId { get; set; }

    public bool IsAllowToDeleteInvoices { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<ApiAuditLog> ApiAuditLogs { get; set; } = new List<ApiAuditLog>();

    [ForeignKey("ColorTypeId")]
    [InverseProperty("Users")]
    public virtual ColorsType ColorType { get; set; } = null!;

    [ForeignKey("DataSourceTypeId")]
    [InverseProperty("Users")]
    public virtual DataSourceType? DataSourceType { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<DocumentsHistory> DocumentsHistories { get; set; } = new List<DocumentsHistory>();

    [InverseProperty("User")]
    public virtual ICollection<ErpCategoryItemRecipe> ErpCategoryItemRecipes { get; set; } = new List<ErpCategoryItemRecipe>();

    [InverseProperty("UpdatedByNavigation")]
    public virtual ICollection<FeatureFlag> FeatureFlags { get; set; } = new List<FeatureFlag>();

    [InverseProperty("User")]
    public virtual ICollection<InvoiceDeletionHistory> InvoiceDeletionHistories { get; set; } = new List<InvoiceDeletionHistory>();

    [InverseProperty("User")]
    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    [InverseProperty("User")]
    public virtual ICollection<InvoicesPaymentActionHistory> InvoicesPaymentActionHistories { get; set; } = new List<InvoicesPaymentActionHistory>();

    [InverseProperty("User")]
    public virtual ICollection<OpenDrawerHistory> OpenDrawerHistories { get; set; } = new List<OpenDrawerHistory>();

    [InverseProperty("User")]
    public virtual ICollection<OrderLock> OrderLocks { get; set; } = new List<OrderLock>();

    [InverseProperty("ExportUser")]
    public virtual ICollection<PendingInvoice> PendingInvoiceExportUsers { get; set; } = new List<PendingInvoice>();

    [InverseProperty("SavedByUser")]
    public virtual ICollection<PendingInvoice> PendingInvoiceSavedByUsers { get; set; } = new List<PendingInvoice>();

    [ForeignKey("PositionTypeId")]
    [InverseProperty("Users")]
    public virtual PositionType PositionType { get; set; } = null!;

    [InverseProperty("User")]
    public virtual ICollection<SyncQueue> SyncQueues { get; set; } = new List<SyncQueue>();

    [InverseProperty("User")]
    public virtual ICollection<SystemLog> SystemLogs { get; set; } = new List<SystemLog>();

    [InverseProperty("User")]
    public virtual ICollection<UserSession> UserSessions { get; set; } = new List<UserSession>();
}
