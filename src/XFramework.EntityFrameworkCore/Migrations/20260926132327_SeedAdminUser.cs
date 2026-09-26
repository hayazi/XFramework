using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Migrations;
using XFramework.EntityFrameworkCore.Identity;

#nullable disable

namespace XFramework.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create Admin role in both AspNetRoles and XfRoles
            var adminRoleId = new Guid("11111111-1111-1111-1111-111111111111");
            
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "Name", "NormalizedName", "ConcurrencyStamp" },
                values: new object[] { adminRoleId, "Admin", "ADMIN", Guid.NewGuid().ToString() });

            migrationBuilder.InsertData(
                table: "XfRoles",
                columns: new[] { "Id", "Name", "DisplayName", "IsSystemRole", "IsActive" },
                values: new object[] { adminRoleId, "Admin", "Administrator", true, true });

            // Create Admin user
            var adminUserId = new Guid("22222222-2222-2222-2222-222222222222");
            var applicationUserId = new Guid("33333333-3333-3333-3333-333333333333");
            var hasher = new PasswordHasher<XFrameworkIdentityUser>();
            var passwordHash = hasher.HashPassword(null!, "123qwe");

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "ApplicationUserId", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount" },
                values: new object[] { adminUserId, applicationUserId, "admin", "ADMIN", "admin@xframework.local", "ADMIN@XFRAMEWORK.LOCAL", true, passwordHash, Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), null, false, false, null, true, 0 });

            // Also add to XfUsers for custom identity (must be before XfUserRoles)
            migrationBuilder.InsertData(
                table: "XfUsers",
                columns: new[] { "Id", "UserName", "DisplayName", "IsActive", "CreatedAt" },
                values: new object[] { applicationUserId, "admin", "Administrator", true, DateTime.UtcNow });

            // Add user to Admin role in AspNetUserRoles
            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "UserId", "RoleId" },
                values: new object[] { adminUserId, adminRoleId });

            // Also add user-role link in XfUserRoles
            migrationBuilder.InsertData(
                table: "XfUserRoles",
                columns: new[] { "UserId", "RoleId" },
                values: new object[] { applicationUserId, adminRoleId });

            // Add all permissions to Admin role
            var permissions = new[]
            {
                // Identity
                "Identity.Role.View", "Identity.Role.Create", "Identity.Role.Edit", "Identity.Role.Delete", "Identity.Role.ManagePermissions",
                // Inventory
                "Inventory.Items.View", "Inventory.Items.Create", "Inventory.Items.Edit", "Inventory.Items.Delete",
                "Inventory.Warehouses.View", "Inventory.Warehouses.Create", "Inventory.Warehouses.Edit", "Inventory.Warehouses.Delete",
                "Inventory.Cardex.View", "Inventory.Cardex.Create", "Inventory.Cardex.Edit", "Inventory.Cardex.Delete",
                // Dimensions
                "Dimensions.CostCenters.View", "Dimensions.CostCenters.Create", "Dimensions.CostCenters.Edit", "Dimensions.CostCenters.Delete",
                "Dimensions.Projects.View", "Dimensions.Projects.Create", "Dimensions.Projects.Edit", "Dimensions.Projects.Delete",
                "Dimensions.CustomDimensions.View", "Dimensions.CustomDimensions.Create", "Dimensions.CustomDimensions.Edit", "Dimensions.CustomDimensions.Delete",
                // Numbering
                "Numbering.NumberSequences.View", "Numbering.NumberSequences.Create", "Numbering.NumberSequences.Edit", "Numbering.NumberSequences.Delete",
                // Tax
                "Tax.TaxCodes.View", "Tax.TaxCodes.Create", "Tax.TaxCodes.Edit", "Tax.TaxCodes.Delete",
                // Parties
                "Parties.Parties.View", "Parties.Parties.Create", "Parties.Parties.Edit", "Parties.Parties.Delete",
                // Accounting
                "Accounting.Accounts.View", "Accounting.Accounts.Create", "Accounting.Accounts.Edit", "Accounting.Accounts.Delete",
                "Accounting.JournalEntries.View", "Accounting.JournalEntries.Create", "Accounting.JournalEntries.Edit", "Accounting.JournalEntries.Delete",
                "Accounting.FiscalPeriods.View", "Accounting.FiscalPeriods.Create", "Accounting.FiscalPeriods.Edit", "Accounting.FiscalPeriods.Delete",
                // Administration
                "Administration.Outbox.View", "Administration.Outbox.Manage"
            };

            // Insert permissions and role permissions
            foreach (var perm in permissions)
            {
                var permId = Guid.NewGuid();
                migrationBuilder.InsertData(
                    table: "XfPermissions",
                    columns: new[] { "Id", "Name", "DisplayName", "Description", "GroupName", "IsEnabled" },
                    values: new object[] { permId, perm, perm.Split('.').Last(), $"Auto-generated permission: {perm}", perm.Split('.')[0], true });

                migrationBuilder.InsertData(
                    table: "XfRolePermissions",
                    columns: new[] { "RoleId", "PermissionId" },
                    values: new object[] { adminRoleId, permId });
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Delete seeded data - reverse order
            var adminRoleId = new Guid("11111111-1111-1111-1111-111111111111");
            var adminUserId = new Guid("22222222-2222-2222-2222-222222222222");
            var applicationUserId = new Guid("33333333-3333-3333-3333-333333333333");

            // Delete role permissions
            migrationBuilder.Sql($"DELETE FROM XfRolePermissions WHERE RoleId = '{adminRoleId}'");

            // Delete permissions created by this migration
            var permissionNames = new[]
            {
                "Identity.Role.View", "Identity.Role.Create", "Identity.Role.Edit", "Identity.Role.Delete", "Identity.Role.ManagePermissions",
                "Inventory.Items.View", "Inventory.Items.Create", "Inventory.Items.Edit", "Inventory.Items.Delete",
                "Inventory.Warehouses.View", "Inventory.Warehouses.Create", "Inventory.Warehouses.Edit", "Inventory.Warehouses.Delete",
                "Inventory.Cardex.View", "Inventory.Cardex.Create", "Inventory.Cardex.Edit", "Inventory.Cardex.Delete",
                "Dimensions.CostCenters.View", "Dimensions.CostCenters.Create", "Dimensions.CostCenters.Edit", "Dimensions.CostCenters.Delete",
                "Dimensions.Projects.View", "Dimensions.Projects.Create", "Dimensions.Projects.Edit", "Dimensions.Projects.Delete",
                "Dimensions.CustomDimensions.View", "Dimensions.CustomDimensions.Create", "Dimensions.CustomDimensions.Edit", "Dimensions.CustomDimensions.Delete",
                "Numbering.NumberSequences.View", "Numbering.NumberSequences.Create", "Numbering.NumberSequences.Edit", "Numbering.NumberSequences.Delete",
                "Tax.TaxCodes.View", "Tax.TaxCodes.Create", "Tax.TaxCodes.Edit", "Tax.TaxCodes.Delete",
                "Parties.Parties.View", "Parties.Parties.Create", "Parties.Parties.Edit", "Parties.Parties.Delete",
                "Accounting.Accounts.View", "Accounting.Accounts.Create", "Accounting.Accounts.Edit", "Accounting.Accounts.Delete",
                "Accounting.JournalEntries.View", "Accounting.JournalEntries.Create", "Accounting.JournalEntries.Edit", "Accounting.JournalEntries.Delete",
                "Accounting.FiscalPeriods.View", "Accounting.FiscalPeriods.Create", "Accounting.FiscalPeriods.Edit", "Accounting.FiscalPeriods.Delete",
                "Administration.Outbox.View", "Administration.Outbox.Manage"
            };

            foreach (var perm in permissionNames)
            {
                migrationBuilder.Sql($"DELETE FROM XfPermissions WHERE Name = '{perm}'");
            }

            // Delete user roles in XfUserRoles
            migrationBuilder.Sql($"DELETE FROM XfUserRoles WHERE UserId = '{applicationUserId}' AND RoleId = '{adminRoleId}'");

            // Delete user roles in AspNetUserRoles
            migrationBuilder.Sql($"DELETE FROM AspNetUserRoles WHERE UserId = '{adminUserId}' AND RoleId = '{adminRoleId}'");

            // Delete user
            migrationBuilder.Sql($"DELETE FROM AspNetUsers WHERE Id = '{adminUserId}'");

            // Delete role from AspNetRoles
            migrationBuilder.Sql($"DELETE FROM AspNetRoles WHERE Id = '{adminRoleId}'");

            // Delete role from XfRoles
            migrationBuilder.Sql($"DELETE FROM XfRoles WHERE Id = '{adminRoleId}'");

            // Delete XfUsers entry
            migrationBuilder.Sql($"DELETE FROM XfUsers WHERE Id = '{applicationUserId}'");
        }
    }
}