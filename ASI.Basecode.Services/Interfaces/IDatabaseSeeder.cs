namespace ASI.Basecode.Services.Interfaces
{
    /// <summary>
    /// Idempotent startup seeding for the CampusPulse database.
    /// The scaffolded schema ships empty; this fills the codebooks and a demo data set
    /// so the application is usable immediately after connecting to SQL Server.
    /// </summary>
    public interface IDatabaseSeeder
    {
        /// <summary>
        /// Ensures the auth-only schema exists, then seeds codebooks and demo records.
        /// Safe to run on every startup.
        /// </summary>
        void Seed();
    }
}
