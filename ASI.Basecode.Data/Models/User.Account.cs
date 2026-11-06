using System;

namespace ASI.Basecode.Data.Models;

/// <summary>
/// Account lifecycle columns (verification, provisioning, lockout) added on top of the
/// scaffolded users table by <c>DatabaseSeeder</c>. Separate partial so re-scaffolding never clobbers it.
/// </summary>
public partial class User
{
    /// <summary>Reason given when an administrator rejected the account's verification.</summary>
    public string VerificationNote { get; set; }

    public DateTime? VerifiedAtUtc { get; set; }

    public long? VerifiedByUserId { get; set; }
}
