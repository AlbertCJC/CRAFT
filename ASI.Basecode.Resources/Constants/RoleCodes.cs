namespace ASI.Basecode.Resources.Constants
{
    /// <summary>
    /// Role codes as stored in <c>roles.role_code</c>.
    /// Used for <c>[Authorize(Roles = ...)]</c> and dashboard routing.
    /// </summary>
    public static class RoleCodes
    {
        public const string Student = "STUDENT";
        public const string Faculty = "FACULTY";

        /// <summary>Custodian in the HLD; stored as TECHNICIAN in the codebook.</summary>
        public const string Technician = "TECHNICIAN";

        public const string Admin = "ADMIN";

        /// <summary>Both borrower roles — convenient for <c>[Authorize(Roles = RoleCodes.Borrower)]</c>.</summary>
        public const string Borrower = Student + "," + Faculty;

        /// <summary>Operational staff: custodian + administrator.</summary>
        public const string Staff = Technician + "," + Admin;
    }

    /// <summary>
    /// Account status codes as stored in <c>account_statuses.code</c>.
    /// </summary>
    public static class AccountStatusCodes
    {
        public const string Active = "ACTIVE";

        /// <summary>May sign in, but cannot submit reservations until verified in [4.2].</summary>
        public const string Unverified = "UNVERIFIED";

        public const string Inactive = "INACTIVE";
        public const string Suspended = "SUSPENDED";
        public const string Locked = "LOCKED";
    }

    /// <summary>
    /// Extra claim types issued alongside the standard ones.
    /// </summary>
    public static class CampusPulseClaims
    {
        public const string AccountStatus = "cp:account_status";
        public const string AccountIdentifier = "cp:account_identifier";
        public const string IsBorrower = "cp:is_borrower";
        public const string Department = "cp:department";
    }
}
