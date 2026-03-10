using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Pos.Web.Infrastructure.Entities;

namespace Pos.Web.Infrastructure.Data;

/// <summary>
/// Entity Framework DbContext for WebPosMembership database.
/// Inherits from IdentityDbContext to provide ASP.NET Core Identity tables with custom user and role types.
/// </summary>
public class WebPosMembershipDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public WebPosMembershipDbContext(DbContextOptions<WebPosMembershipDbContext> options)
        : base(options)
    {
    }

    // Custom authentication tables
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }
    public DbSet<AuthAuditLog> AuthAuditLogs { get; set; }
    public DbSet<PasswordHistory> PasswordHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure ApplicationUser
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("AspNetUsers");

            // Unique constraint on EmployeeId
            entity.HasIndex(e => e.EmployeeId)
                .IsUnique()
                .HasDatabaseName("UQ_AspNetUsers_EmployeeId");

            // Index on IsActive for filtering active users
            entity.HasIndex(e => e.IsActive)
                .HasDatabaseName("IX_AspNetUsers_IsActive");

            // Configure relationships
            entity.HasMany(e => e.RefreshTokens)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.UserSessions)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.AuditLogs)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.PasswordHistories)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure ApplicationRole
        builder.Entity<ApplicationRole>(entity =>
        {
            entity.ToTable("AspNetRoles");
        });

        // Configure RefreshToken
        builder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");

            entity.HasKey(e => e.Id);

            // Unique constraint on Token
            entity.HasIndex(e => e.Token)
                .IsUnique()
                .HasDatabaseName("UQ_RefreshTokens_Token");

            // Performance indexes
            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_RefreshTokens_UserId");

            entity.HasIndex(e => e.ExpiresAt)
                .HasDatabaseName("IX_RefreshTokens_ExpiresAt");

            entity.HasIndex(e => e.RevokedAt)
                .HasDatabaseName("IX_RefreshTokens_RevokedAt");

            // Ignore computed properties
            entity.Ignore(e => e.IsExpired);
            entity.Ignore(e => e.IsRevoked);
            entity.Ignore(e => e.IsActive);
        });

        // Configure UserSession
        builder.Entity<UserSession>(entity =>
        {
            entity.ToTable("UserSessions");

            entity.HasKey(e => e.SessionId);

            // Check constraint on DeviceType
            entity.HasCheckConstraint(
                "CHK_UserSessions_DeviceType",
                "[DeviceType] IN ('Desktop', 'Tablet', 'Mobile')");

            // Performance indexes
            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_UserSessions_UserId");

            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_UserSessions_CreatedAt");

            entity.HasIndex(e => e.EndedAt)
                .HasDatabaseName("IX_UserSessions_EndedAt");

            // Ignore computed property
            entity.Ignore(e => e.IsActive);
        });

        // Configure AuthAuditLog
        builder.Entity<AuthAuditLog>(entity =>
        {
            entity.ToTable("AuthAuditLog");

            entity.HasKey(e => e.Id);

            // Performance indexes
            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_AuthAuditLog_UserId");

            entity.HasIndex(e => e.EventType)
                .HasDatabaseName("IX_AuthAuditLog_EventType");

            entity.HasIndex(e => e.Timestamp)
                .HasDatabaseName("IX_AuthAuditLog_Timestamp");

            entity.HasIndex(e => e.IsSuccessful)
                .HasDatabaseName("IX_AuthAuditLog_IsSuccessful");
        });

        // Configure PasswordHistory
        builder.Entity<PasswordHistory>(entity =>
        {
            entity.ToTable("PasswordHistory");

            entity.HasKey(e => e.Id);

            // Performance indexes
            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_PasswordHistory_UserId");

            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_PasswordHistory_CreatedAt");
        });
    }
}
