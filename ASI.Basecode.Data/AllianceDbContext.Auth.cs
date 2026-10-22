using ASI.Basecode.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace ASI.Basecode.Data;

/// <summary>
/// Auth-related additions to the scaffolded <see cref="AllianceDbContext"/>.
/// Kept in its own partial file so re-scaffolding the database-first model
/// never clobbers it; configuration hangs off the OnModelCreatingPartial hook.
/// </summary>
public partial class AllianceDbContext
{
    public virtual DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    public virtual DbSet<UserPreference> UserPreferences { get; set; }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.ContactNumber)
                .HasMaxLength(50)
                .HasColumnName("contact_number");
            entity.Property(e => e.AlternateContact)
                .HasMaxLength(100)
                .HasColumnName("alternate_contact");
        });

        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.Property(e => e.ImagePath)
                .HasMaxLength(500)
                .HasColumnName("image_path");
        });

        modelBuilder.Entity<UserPreference>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.ToTable("user_preferences");

            entity.Property(e => e.UserId).ValueGeneratedNever().HasColumnName("user_id");
            entity.Property(e => e.NotifyReservationUpdates).HasColumnName("notify_reservation_updates");
            entity.Property(e => e.NotifyDueReminders).HasColumnName("notify_due_reminders");
            entity.Property(e => e.NotifyOverdueAlerts).HasColumnName("notify_overdue_alerts");
            entity.Property(e => e.NotifyReturnConfirmations).HasColumnName("notify_return_confirmations");
            entity.Property(e => e.NotifyAnnouncements).HasColumnName("notify_announcements");
            entity.Property(e => e.UpdatedAtUtc)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("updated_at_utc");

            entity.HasOne(d => d.User).WithOne(p => p.Preference)
                .HasForeignKey<UserPreference>(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_user_preferences_users");
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(e => e.PasswordResetTokenId);

            entity.ToTable("password_reset_tokens");

            entity.HasIndex(e => e.Token, "UQ_password_reset_tokens_token").IsUnique();

            entity.Property(e => e.PasswordResetTokenId).HasColumnName("password_reset_token_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Token)
                .HasMaxLength(200)
                .HasColumnName("token");
            entity.Property(e => e.CreatedAtUtc)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at_utc");
            entity.Property(e => e.ExpiresAtUtc).HasColumnName("expires_at_utc");
            entity.Property(e => e.UsedAtUtc).HasColumnName("used_at_utc");

            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_password_reset_tokens_users");
        });
    }
}
