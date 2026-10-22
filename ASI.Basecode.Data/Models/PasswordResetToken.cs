using System;

namespace ASI.Basecode.Data.Models;

/// <summary>
/// Single-use password reset token issued by [1.3 Forgot Password] and consumed by [1.4 Reset Password].
/// </summary>
public partial class PasswordResetToken
{
    public long PasswordResetTokenId { get; set; }

    public long UserId { get; set; }

    /// <summary>Opaque URL-safe token embedded in the reset link.</summary>
    public string Token { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    /// <summary>Link validity window — 24 hours per the HLD.</summary>
    public DateTime ExpiresAtUtc { get; set; }

    /// <summary>Set once the token has been redeemed; a token never works twice.</summary>
    public DateTime? UsedAtUtc { get; set; }

    public virtual User User { get; set; }
}
