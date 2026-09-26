using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XFramework.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class ERP_Modules_R7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EntityName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    ChangesJson = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CostCenters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ParentCostCenterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ManagerId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BudgetAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    BudgetCurrency = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CostCenters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CostCenters_CostCenters_ParentCostCenterId",
                        column: x => x.ParentCostCenterId,
                        principalTable: "CostCenters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomDimensions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DimensionKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    AllowHierarchy = table.Column<bool>(type: "bit", nullable: false),
                    ParentDimensionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Attributes = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomDimensions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomDimensions_CustomDimensions_ParentDimensionId",
                        column: x => x.ParentDimensionId,
                        principalTable: "CustomDimensions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    BaseUnit = table.Column<int>(type: "int", nullable: false),
                    CostingMethod = table.Column<int>(type: "int", nullable: false),
                    StandardCost = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsStocked = table.Column<bool>(type: "bit", nullable: false),
                    IsPurchasable = table.Column<bool>(type: "bit", nullable: false),
                    IsSellable = table.Column<bool>(type: "bit", nullable: false),
                    IsProducible = table.Column<bool>(type: "bit", nullable: false),
                    MinimumStock = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    MaximumStock = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    ReorderPoint = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    DefaultWarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Barcode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CardexEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionType = table.Column<int>(type: "int", nullable: false),
                    DocumentReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QuantityIn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuantityOut = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RunningBalance = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UnitCost = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalCost = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CostingMethod = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReferenceDocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReferenceDocumentLine = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardexEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NumberSequences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Prefix = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CurrentNumber = table.Column<int>(type: "int", nullable: false),
                    MinimumDigits = table.Column<int>(type: "int", nullable: false),
                    IncrementBy = table.Column<int>(type: "int", nullable: false),
                    MaximumNumber = table.Column<int>(type: "int", nullable: true),
                    Scope = table.Column<int>(type: "int", nullable: false),
                    ScopeIdentifier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ResetDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AutoReset = table.Column<bool>(type: "bit", nullable: false),
                    FormatTemplate = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NumberSequences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EventVersion = table.Column<int>(type: "int", nullable: false),
                    AggregateType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AggregateId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ModuleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Headers = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    NextAttemptOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LockId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LockedUntilUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastError = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OccurredOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CausationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcessedMessages",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HandlerName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ProcessedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CorrelationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessedMessages", x => new { x.EventId, x.HandlerName });
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Budget = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaxCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TaxType = table.Column<int>(type: "int", nullable: false),
                    CalculationMethod = table.Column<int>(type: "int", nullable: false),
                    Application = table.Column<int>(type: "int", nullable: false),
                    Rate = table.Column<string>(type: "nvarchar(max)", precision: 5, scale: 2, nullable: false),
                    FixedAmount = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PerUnit = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsRecoverable = table.Column<bool>(type: "bit", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Tiers = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Warehouses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AllowNegativeStock = table.Column<bool>(type: "bit", nullable: false),
                    ManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warehouses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntries_EntityName_EntityId",
                table: "AuditEntries",
                columns: new[] { "EntityName", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntries_Timestamp",
                table: "AuditEntries",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntries_UserId",
                table: "AuditEntries",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CostCenters_Code",
                table: "CostCenters",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CostCenters_ManagerId",
                table: "CostCenters",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_CostCenters_ParentCostCenterId",
                table: "CostCenters",
                column: "ParentCostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_CostCenters_Status",
                table: "CostCenters",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CustomDimensions_Code",
                table: "CustomDimensions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomDimensions_DimensionKey",
                table: "CustomDimensions",
                column: "DimensionKey");

            migrationBuilder.CreateIndex(
                name: "IX_CustomDimensions_ParentDimensionId",
                table: "CustomDimensions",
                column: "ParentDimensionId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomDimensions_Status",
                table: "CustomDimensions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Items_Code",
                table: "Items",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_DefaultWarehouseId",
                table: "Items",
                column: "DefaultWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_Status",
                table: "Items",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Items_Type",
                table: "Items",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_CardexEntries_DocumentReference",
                table: "CardexEntries",
                column: "DocumentReference");

            migrationBuilder.CreateIndex(
                name: "IX_CardexEntries_ItemId_WarehouseId_TransactionDate",
                table: "CardexEntries",
                columns: new[] { "ItemId", "WarehouseId", "TransactionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CardexEntries_ReferenceDocumentId",
                table: "CardexEntries",
                column: "ReferenceDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_CardexEntries_TransactionType",
                table: "CardexEntries",
                column: "TransactionType");

            migrationBuilder.CreateIndex(
                name: "IX_NumberSequences_Code_Scope_ScopeIdentifier",
                table: "NumberSequences",
                columns: new[] { "Code", "Scope", "ScopeIdentifier" },
                unique: true,
                filter: "[ScopeIdentifier] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_NumberSequences_Scope",
                table: "NumberSequences",
                column: "Scope");

            migrationBuilder.CreateIndex(
                name: "IX_NumberSequences_Status",
                table: "NumberSequences",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_EventId",
                table: "OutboxMessages",
                column: "EventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Status_NextAttemptOnUtc_LockedUntilUtc",
                table: "OutboxMessages",
                columns: new[] { "Status", "NextAttemptOnUtc", "LockedUntilUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Code",
                table: "Projects",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_CustomerId",
                table: "Projects",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ManagerId",
                table: "Projects",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_StartDate",
                table: "Projects",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Status",
                table: "Projects",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TaxCodes_Code",
                table: "TaxCodes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaxCodes_EffectiveFrom",
                table: "TaxCodes",
                column: "EffectiveFrom");

            migrationBuilder.CreateIndex(
                name: "IX_TaxCodes_IsDefault",
                table: "TaxCodes",
                column: "IsDefault");

            migrationBuilder.CreateIndex(
                name: "IX_TaxCodes_Status",
                table: "TaxCodes",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TaxCodes_TaxType",
                table: "TaxCodes",
                column: "TaxType");

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_Code",
                table: "Warehouses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_IsActive",
                table: "Warehouses",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_ManagerId",
                table: "Warehouses",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_Type",
                table: "Warehouses",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditEntries");

            migrationBuilder.DropTable(
                name: "CostCenters");

            migrationBuilder.DropTable(
                name: "CustomDimensions");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "CardexEntries");

            migrationBuilder.DropTable(
                name: "NumberSequences");

            migrationBuilder.DropTable(
                name: "OutboxMessages");

            migrationBuilder.DropTable(
                name: "ProcessedMessages");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "TaxCodes");

            migrationBuilder.DropTable(
                name: "Warehouses");
        }
    }
}
