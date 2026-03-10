using Microsoft.EntityFrameworkCore;
using Pos.Web.Infrastructure.Entities;
using Pos.Web.Infrastructure.Entities.Legacy;

namespace Pos.Web.Infrastructure.Data;

/// <summary>
/// Database context for the POS system
/// Connects to existing SQL Server database with dbo and web schemas
/// Now using scaffolded entities from Entities/Legacy for accurate database mapping
/// </summary>
public class PosDbContext : DbContext
{
    public PosDbContext(DbContextOptions<PosDbContext> options) : base(options)
    {
    }

    // Legacy dbo schema tables (using scaffolded entities for accurate mapping)
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<CustomerAddress> CustomerAddresses { get; set; } = null!;
    public DbSet<Invoice> Invoices { get; set; } = null!;
    public DbSet<InvoiceItem> InvoiceItems { get; set; } = null!;
    public DbSet<CategoryItem> CategoryItems { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    
    // Web schema tables (new tables for web app)
    public DbSet<OrderLock> OrderLocks { get; set; } = null!;
    public DbSet<Entities.ApiAuditLog> ApiAuditLogs { get; set; } = null!;
    public DbSet<Entities.FeatureFlag> FeatureFlags { get; set; } = null!;
    public DbSet<SyncQueue> SyncQueues { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Note: Legacy dbo schema tables (Category, CategoryItem, Invoice, etc.) 
        // are configured by EF Core conventions and scaffolded configurations.
        // We don't need to manually configure them here.
        
        // Configure web schema tables only
        ConfigureOrderLock(modelBuilder);
        ConfigureApiAuditLog(modelBuilder);
        ConfigureFeatureFlag(modelBuilder);
        ConfigureSyncQueue(modelBuilder);
    }

    /// <summary>
    /// Configure OrderLock entity
    /// </summary>
    private void ConfigureOrderLock(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderLock>(entity =>
        {
            entity.ToTable("OrderLocks", "web");
            entity.HasKey(e => e.ID);
            
            entity.Property(e => e.OrderID).IsRequired();
            entity.Property(e => e.UserID).IsRequired();
            entity.Property(e => e.LockAcquiredAt).IsRequired();
            entity.Property(e => e.LockExpiresAt).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.SessionID).HasMaxLength(100);
            entity.Property(e => e.DeviceInfo).HasMaxLength(200);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            
            // Indexes
            entity.HasIndex(e => new { e.OrderID, e.IsActive })
                .HasDatabaseName("IX_OrderLocks_OrderID_IsActive");
            
            entity.HasIndex(e => e.LockExpiresAt)
                .HasDatabaseName("IX_OrderLocks_LockExpiresAt")
                .HasFilter("[IsActive] = 1");
            
            // Foreign key to User
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserID)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    /// <summary>
    /// Configure ApiAuditLog entity (web schema)
    /// </summary>
    private void ConfigureApiAuditLog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Entities.ApiAuditLog>(entity =>
        {
            entity.ToTable("ApiAuditLog", "web");
            entity.HasKey(e => e.ID);
            
            entity.Property(e => e.Timestamp).IsRequired();
            entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityType).HasMaxLength(100);
            entity.Property(e => e.IPAddress).HasMaxLength(50);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.RequestPath).HasMaxLength(500);
            entity.Property(e => e.RequestMethod).HasMaxLength(10);
            
            // Indexes
            entity.HasIndex(e => e.Timestamp)
                .HasDatabaseName("IX_ApiAuditLog_Timestamp")
                .IsDescending();
            
            entity.HasIndex(e => new { e.UserID, e.Timestamp })
                .HasDatabaseName("IX_ApiAuditLog_UserID_Timestamp")
                .IsDescending(false, true);
            
            entity.HasIndex(e => new { e.Action, e.Timestamp })
                .HasDatabaseName("IX_ApiAuditLog_Action_Timestamp")
                .IsDescending(false, true);
            
            // Foreign key to User
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserID)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    /// <summary>
    /// Configure FeatureFlag entity (web schema)
    /// </summary>
    private void ConfigureFeatureFlag(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Entities.FeatureFlag>(entity =>
        {
            entity.ToTable("FeatureFlags", "web");
            entity.HasKey(e => e.ID);
            
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsEnabled).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            
            // Unique constraint on Name
            entity.HasIndex(e => e.Name)
                .IsUnique()
                .HasDatabaseName("UQ_FeatureFlags_Name");
            
            // Index
            entity.HasIndex(e => e.IsEnabled)
                .HasDatabaseName("IX_FeatureFlags_IsEnabled");
            
            // Foreign key to User (UpdatedBy)
            entity.HasOne(e => e.UpdatedByUser)
                .WithMany()
                .HasForeignKey(e => e.UpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    /// <summary>
    /// Configure SyncQueue entity (web schema)
    /// </summary>
    private void ConfigureSyncQueue(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SyncQueue>(entity =>
        {
            entity.ToTable("SyncQueue", "web");
            entity.HasKey(e => e.ID);
            
            entity.Property(e => e.UserID).IsRequired();
            entity.Property(e => e.DeviceID).IsRequired().HasMaxLength(100);
            entity.Property(e => e.OperationType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Payload).IsRequired();
            entity.Property(e => e.ClientTimestamp).IsRequired();
            entity.Property(e => e.ServerTimestamp).IsRequired();
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.Property(e => e.AttemptCount).IsRequired();
            
            // Indexes
            entity.HasIndex(e => new { e.Status, e.ClientTimestamp })
                .HasDatabaseName("IX_SyncQueue_Status_ClientTimestamp");
            
            entity.HasIndex(e => new { e.UserID, e.DeviceID, e.Status })
                .HasDatabaseName("IX_SyncQueue_UserID_DeviceID_Status");
            
            // Foreign key to User
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserID)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
