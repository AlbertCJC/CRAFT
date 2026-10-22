using ASI.Basecode.Data;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Resources.Constants;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Services.Services
{
    /// <summary>
    /// Idempotent seeding for the empty scaffolded database.
    /// Demo password for every seeded account is <c>Campus123</c>.
    /// </summary>
    public class DatabaseSeeder : ServiceBase, IDatabaseSeeder
    {
        /// <summary>Shared password for all demo accounts (meets the HLD policy).</summary>
        public const string DemoPassword = "Campus123";

        private readonly IUnitOfWork _unitOfWork;

        private AllianceDbContext DbContext => (AllianceDbContext)_unitOfWork.Database;

        public DatabaseSeeder(ILoggerFactory loggerFactory, IUnitOfWork unitOfWork)
            : base(loggerFactory)
        {
            this._unitOfWork = unitOfWork;
        }

        public void Seed()
        {
            try
            {
                this.EnsureAuthSchema();
                this.SeedCodebooks();
                this.SeedDemoData();
            }
            catch (Exception ex)
            {
                // Never take the app down on seeding failure — surface it in the log instead.
                this._logger.LogError(ex, "Database seeding failed.");
            }
        }

        private void EnsureAuthSchema()
        {
            // The scaffolded schema predates password reset support. Create the table
            // in place so no EF migration is required for the database-first setup.
            this.DbContext.Database.ExecuteSqlRaw(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'password_reset_tokens')
BEGIN
    CREATE TABLE dbo.password_reset_tokens (
        password_reset_token_id bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_password_reset_tokens PRIMARY KEY,
        user_id bigint NOT NULL,
        token nvarchar(200) NOT NULL,
        created_at_utc datetime2 NOT NULL CONSTRAINT DF_password_reset_tokens_created DEFAULT (sysutcdatetime()),
        expires_at_utc datetime2 NOT NULL,
        used_at_utc datetime2 NULL,
        CONSTRAINT FK_password_reset_tokens_users FOREIGN KEY (user_id) REFERENCES dbo.users(user_id) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX UQ_password_reset_tokens_token ON dbo.password_reset_tokens(token);
END;

IF COL_LENGTH('dbo.users', 'contact_number') IS NULL
    ALTER TABLE dbo.users ADD contact_number nvarchar(50) NULL;

IF COL_LENGTH('dbo.users', 'alternate_contact') IS NULL
    ALTER TABLE dbo.users ADD alternate_contact nvarchar(100) NULL;

IF COL_LENGTH('dbo.equipment', 'image_path') IS NULL
    ALTER TABLE dbo.equipment ADD image_path nvarchar(500) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'user_preferences')
BEGIN
    CREATE TABLE dbo.user_preferences (
        user_id bigint NOT NULL CONSTRAINT PK_user_preferences PRIMARY KEY,
        notify_reservation_updates bit NOT NULL CONSTRAINT DF_user_preferences_res DEFAULT (1),
        notify_due_reminders bit NOT NULL CONSTRAINT DF_user_preferences_due DEFAULT (1),
        notify_overdue_alerts bit NOT NULL CONSTRAINT DF_user_preferences_overdue DEFAULT (1),
        notify_return_confirmations bit NOT NULL CONSTRAINT DF_user_preferences_return DEFAULT (1),
        notify_announcements bit NOT NULL CONSTRAINT DF_user_preferences_ann DEFAULT (1),
        updated_at_utc datetime2 NOT NULL CONSTRAINT DF_user_preferences_updated DEFAULT (sysutcdatetime()),
        CONSTRAINT FK_user_preferences_users FOREIGN KEY (user_id) REFERENCES dbo.users(user_id) ON DELETE CASCADE
    );
END;");
        }

        private void SeedCodebooks()
        {
            var db = this.DbContext;
            var changed = false;

            if (!db.Roles.Any())
            {
                db.Roles.AddRange(
                    new Role { RoleCode = RoleCodes.Student, RoleName = "Student", IsBorrowerRole = true },
                    new Role { RoleCode = RoleCodes.Faculty, RoleName = "Faculty", IsBorrowerRole = true },
                    new Role { RoleCode = RoleCodes.Technician, RoleName = "Custodian", IsBorrowerRole = false },
                    new Role { RoleCode = RoleCodes.Admin, RoleName = "Administrator", IsBorrowerRole = false });
                changed = true;
            }

            if (!db.AccountStatuses.Any())
            {
                db.AccountStatuses.AddRange(
                    new AccountStatus { Code = AccountStatusCodes.Active, Description = "Active" },
                    new AccountStatus { Code = AccountStatusCodes.Unverified, Description = "Unverified" },
                    new AccountStatus { Code = AccountStatusCodes.Inactive, Description = "Inactive" },
                    new AccountStatus { Code = AccountStatusCodes.Suspended, Description = "Suspended" },
                    new AccountStatus { Code = AccountStatusCodes.Locked, Description = "Locked" });
                changed = true;
            }

            if (!db.ReservationStatuses.Any())
            {
                db.ReservationStatuses.AddRange(
                    new ReservationStatus { Code = "PENDING", Description = "Pending" },
                    new ReservationStatus { Code = "APPROVED", Description = "Approved" },
                    new ReservationStatus { Code = "REJECTED", Description = "Rejected" },
                    new ReservationStatus { Code = "CANCELLED", Description = "Cancelled" },
                    new ReservationStatus { Code = "READY_FOR_RELEASE", Description = "Ready for Release" },
                    new ReservationStatus { Code = "COMPLETED", Description = "Completed" });
                changed = true;
            }

            if (!db.BorrowingStatuses.Any())
            {
                db.BorrowingStatuses.AddRange(
                    new BorrowingStatus { Code = "ACTIVE", Description = "Active" },
                    new BorrowingStatus { Code = "OVERDUE", Description = "Overdue" },
                    new BorrowingStatus { Code = "PARTIALLY_RETURNED", Description = "Partially Returned" },
                    new BorrowingStatus { Code = "RETURNED", Description = "Returned" },
                    new BorrowingStatus { Code = "LOST", Description = "Lost" });
                changed = true;
            }

            if (!db.InspectionStatuses.Any())
            {
                db.InspectionStatuses.AddRange(
                    new InspectionStatus { Code = "PENDING", Description = "Awaiting Inspection" },
                    new InspectionStatus { Code = "COMPLETED", Description = "Inspection Completed" });
                changed = true;
            }

            if (!db.Conditions.Any())
            {
                db.Conditions.AddRange(
                    new Condition { Code = "GOOD", Description = "Good", AffectsAvailability = false },
                    new Condition { Code = "MINOR_DAMAGE", Description = "Minor Damage", AffectsAvailability = false },
                    new Condition { Code = "DAMAGED", Description = "Damaged", AffectsAvailability = true },
                    new Condition { Code = "MISSING_PARTS", Description = "Missing Parts", AffectsAvailability = true },
                    new Condition { Code = "LOST", Description = "Lost", AffectsAvailability = true },
                    new Condition { Code = "AVAILABLE", Description = "Available", AffectsAvailability = false },
                    new Condition { Code = "UNDER_MAINTENANCE", Description = "Under Maintenance", AffectsAvailability = true });
                changed = true;
            }

            if (!db.Departments.Any())
            {
                db.Departments.AddRange(
                    new Department { DepartmentCode = "CCS", DepartmentName = "College of Computer Studies", IsActive = true },
                    new Department { DepartmentCode = "CENG", DepartmentName = "College of Engineering", IsActive = true },
                    new Department { DepartmentCode = "CBA", DepartmentName = "College of Business Administration", IsActive = true },
                    new Department { DepartmentCode = "COED", DepartmentName = "College of Education", IsActive = true },
                    new Department { DepartmentCode = "CAS", DepartmentName = "College of Arts and Sciences", IsActive = true });
                changed = true;
            }

            if (!db.Locations.Any())
            {
                db.Locations.AddRange(
                    new Location { LocationCode = "PO-MAIN", LocationName = "Property Office - Main", IsPickupLocation = true, IsActive = true },
                    new Location { LocationCode = "PO-ANNEX", LocationName = "Property Office - Annex", IsPickupLocation = true, IsActive = true },
                    new Location { LocationCode = "LIB", LocationName = "Library Media Room", IsPickupLocation = false, IsActive = true },
                    new Location { LocationCode = "GYM", LocationName = "Gymnasium Storage", IsPickupLocation = false, IsActive = true });
                changed = true;
            }

            if (!db.EquipmentCategories.Any())
            {
                db.EquipmentCategories.AddRange(
                    new EquipmentCategory { CategoryCode = "LAPTOP", CategoryName = "Laptops" },
                    new EquipmentCategory { CategoryCode = "PROJECTOR", CategoryName = "Projectors" },
                    new EquipmentCategory { CategoryCode = "CAMERA", CategoryName = "Cameras" },
                    new EquipmentCategory { CategoryCode = "AUDIO", CategoryName = "Audio Equipment" },
                    new EquipmentCategory { CategoryCode = "NETWORK", CategoryName = "Networking Equipment" },
                    new EquipmentCategory { CategoryCode = "LAB", CategoryName = "Laboratory Tools" });
                changed = true;
            }

            if (!db.BorrowingPolicyVersions.Any())
            {
                db.BorrowingPolicyVersions.Add(new BorrowingPolicyVersion
                {
                    VersionNumber = "v1.0",
                    EffectiveFromUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    IsCurrent = true,
                    MaxBorrowDurationDays = 7,
                    MaxActiveBorrowingUnits = 3,
                    OverdueGracePeriodMinutes = 60,
                    LowStockThreshold = 2,
                    BlockReservationWhenOverdue = true,
                });
                changed = true;
            }

            if (changed)
            {
                this._unitOfWork.SaveChanges();
            }
        }

        private void SeedDemoData()
        {
            var db = this.DbContext;

            if (!db.Users.Any())
            {
                var roles = db.Roles.ToDictionary(r => r.RoleCode, r => r.RoleId);
                var statuses = db.AccountStatuses.ToDictionary(s => s.Code, s => s.AccountStatusId);
                var departments = db.Departments.ToDictionary(d => d.DepartmentCode, d => d.DepartmentId);

                string Hash() => PasswordManager.Hash(DemoPassword);

                db.Users.AddRange(
                    new User
                    {
                        AccountIdentifier = "ADMIN-0001",
                        FirstName = "Ana",
                        LastName = "Reyes",
                        SchoolEmail = "ana.reyes@university.edu.ph",
                        PasswordHash = Hash(),
                        RoleId = roles[RoleCodes.Admin],
                        DepartmentId = departments["CCS"],
                        AccountStatusId = statuses[AccountStatusCodes.Active],
                        CreatedAtUtc = default,
                    },
                    new User
                    {
                        AccountIdentifier = "EMP-1001",
                        FirstName = "Carlos",
                        LastName = "Diaz",
                        SchoolEmail = "carlos.diaz@university.edu.ph",
                        PasswordHash = Hash(),
                        RoleId = roles[RoleCodes.Technician],
                        DepartmentId = departments["CCS"],
                        AccountStatusId = statuses[AccountStatusCodes.Active],
                        CreatedAtUtc = default,
                    },
                    new User
                    {
                        AccountIdentifier = "2023-104857",
                        FirstName = "Maria",
                        LastName = "Santos",
                        SchoolEmail = "maria.santos@university.edu.ph",
                        PasswordHash = Hash(),
                        RoleId = roles[RoleCodes.Student],
                        DepartmentId = departments["CCS"],
                        AccountStatusId = statuses[AccountStatusCodes.Active],
                        CreatedAtUtc = default,
                    },
                    new User
                    {
                        AccountIdentifier = "EMP-2001",
                        FirstName = "Jose",
                        LastName = "Cruz",
                        SchoolEmail = "jose.cruz@university.edu.ph",
                        PasswordHash = Hash(),
                        RoleId = roles[RoleCodes.Faculty],
                        DepartmentId = departments["CENG"],
                        AccountStatusId = statuses[AccountStatusCodes.Active],
                        CreatedAtUtc = default,
                    },
                    new User
                    {
                        AccountIdentifier = "2024-118002",
                        FirstName = "Liza",
                        LastName = "Mendoza",
                        SchoolEmail = "liza.mendoza@university.edu.ph",
                        PasswordHash = Hash(),
                        RoleId = roles[RoleCodes.Student],
                        DepartmentId = departments["CBA"],
                        AccountStatusId = statuses[AccountStatusCodes.Unverified],
                        CreatedAtUtc = default,
                    });

                this._unitOfWork.SaveChanges();
            }

            if (!db.Equipment.Any())
            {
                var categories = db.EquipmentCategories.ToDictionary(c => c.CategoryCode, c => c.EquipmentCategoryId);
                var locations = db.Locations.ToDictionary(l => l.LocationCode, l => l.LocationId);
                var conditions = db.Conditions.ToDictionary(c => c.Code, c => c.ConditionId);

                void AddEquipment(string asset, string name, string description, string category,
                    string location, int total, int reserved = 0, int released = 0,
                    string condition = "GOOD", string operational = "AVAILABLE")
                {
                    var equipment = new Equipment
                    {
                        AssetCode = asset,
                        EquipmentName = name,
                        Description = description,
                        EquipmentCategoryId = categories[category],
                        LocationId = locations[location],
                        CurrentConditionId = conditions[condition],
                        OperationalStatusId = conditions[operational],
                        IsActive = true,
                        CreatedAtUtc = default,
                    };

                    equipment.EquipmentInventoryBalance = new EquipmentInventoryBalance
                    {
                        TotalQuantity = total,
                        ReservedQuantity = reserved,
                        ReleasedQuantity = released,
                    };

                    db.Equipment.Add(equipment);
                }

                AddEquipment("LP-001", "Dell Latitude 5440", "14-inch business laptop, 16GB RAM, 512GB SSD.", "LAPTOP", "PO-MAIN", 10);
                AddEquipment("LP-002", "MacBook Air M2", "13-inch laptop for creative work and presentations.", "LAPTOP", "PO-MAIN", 5, reserved: 1);
                AddEquipment("PRJ-101", "Epson EB-X06 Projector", "3600 lumens XGA projector with HDMI input.", "PROJECTOR", "PO-ANNEX", 6);
                AddEquipment("CAM-201", "Canon EOS 250D", "Entry-level DSLR with 18-55mm kit lens.", "CAMERA", "LIB", 3, condition: "MINOR_DAMAGE");
                AddEquipment("AUD-301", "Shure SM58 Microphone Set", "Dynamic vocal microphone with XLR cable and stand.", "AUDIO", "GYM", 8);
                AddEquipment("NET-401", "Cisco Catalyst 2960 Switch", "24-port managed network switch for lab use.", "NETWORK", "PO-MAIN", 2, operational: "UNDER_MAINTENANCE");
                AddEquipment("LAB-501", "Arduino Uno Starter Kit", "Microcontroller kit with sensors and breadboard.", "LAB", "PO-MAIN", 0);
                AddEquipment("LP-003", "Lenovo ThinkPad E14", "14-inch laptop, 8GB RAM, 256GB SSD.", "LAPTOP", "PO-ANNEX", 4);

                this._unitOfWork.SaveChanges();
            }

            // Every account gets a default (all-on) notification preference row.
            this.DbContext.Database.ExecuteSqlRaw(@"
INSERT INTO dbo.user_preferences (user_id)
SELECT u.user_id FROM dbo.users u
WHERE NOT EXISTS (SELECT 1 FROM dbo.user_preferences p WHERE p.user_id = u.user_id);");

            if (!db.Notifications.Any())
            {
                var student = db.Users.FirstOrDefault(u => u.AccountIdentifier == "2023-104857");
                if (student != null)
                {
                    db.Notifications.AddRange(
                        new Notification
                        {
                            RecipientUserId = student.UserId,
                            NotificationType = "ACCOUNT",
                            Title = "Welcome to CampusPulse",
                            Message = "Your account is active. Browse equipment availability to start a reservation.",
                            IsRead = false,
                            CreatedAtUtc = DateTime.UtcNow,
                        });
                    this._unitOfWork.SaveChanges();
                }
            }
        }
    }
}
