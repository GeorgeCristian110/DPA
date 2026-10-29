using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DPA.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Destinations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Address = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ZipCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    City = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    DrivingTime = table.Column<TimeSpan>(type: "interval", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Destinations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VehicleProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    MinFrontOffsetM = table.Column<decimal>(type: "numeric(3,2)", precision: 3, scale: 2, nullable: false),
                    MaxFrontOffsetM = table.Column<decimal>(type: "numeric(3,2)", precision: 3, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LengthM = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    WidthM = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    HeightM = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    EmptyWeightKg = table.Column<decimal>(type: "numeric(5,0)", precision: 5, scale: 0, nullable: false),
                    FrontToRearSupportM = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    EmptyCgXm = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    FrontSupportOffsetM = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    MaxPayloadKg = table.Column<decimal>(type: "numeric(5,0)", precision: 5, scale: 0, nullable: false),
                    VehicleProfileId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vehicles_VehicleProfiles_VehicleProfileId",
                        column: x => x.VehicleProfileId,
                        principalTable: "VehicleProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Manifests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DestinationId = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RequiredArrivalAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PlanDueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TruckArrivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DispatchedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ConfirmedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConfirmedByName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedByName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Manifests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Manifests_Destinations_DestinationId",
                        column: x => x.DestinationId,
                        principalTable: "Destinations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Manifests_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CargoItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PalletCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LengthM = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    WidthM = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    HeightM = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    WeightKg = table.Column<decimal>(type: "numeric(5,0)", precision: 5, scale: 0, nullable: false),
                    Stability = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ManifestId = table.Column<Guid>(type: "uuid", nullable: true),
                    ArrivedOnDockAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CargoItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CargoItems_Manifests_ManifestId",
                        column: x => x.ManifestId,
                        principalTable: "Manifests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Placements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CargoItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Xm = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    Ym = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    IsRotated = table.Column<bool>(type: "boolean", nullable: false),
                    WeightKg = table.Column<decimal>(type: "numeric(5,0)", precision: 5, scale: 0, nullable: false),
                    LengthM = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    WidthM = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    HeightM = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    ManifestId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Placements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Placements_Manifests_ManifestId",
                        column: x => x.ManifestId,
                        principalTable: "Manifests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Rejections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RejectedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RejectedByUserName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RejectionMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    RejectionDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ManifestId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rejections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rejections_Manifests_ManifestId",
                        column: x => x.ManifestId,
                        principalTable: "Manifests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CargoItems_ManifestId",
                table: "CargoItems",
                column: "ManifestId");

            migrationBuilder.CreateIndex(
                name: "IX_CargoItems_PalletCode",
                table: "CargoItems",
                column: "PalletCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Manifests_DestinationId",
                table: "Manifests",
                column: "DestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_Manifests_VehicleId",
                table: "Manifests",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_Placements_ManifestId",
                table: "Placements",
                column: "ManifestId");

            migrationBuilder.CreateIndex(
                name: "IX_Rejections_ManifestId",
                table: "Rejections",
                column: "ManifestId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_Code",
                table: "Vehicles",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_VehicleProfileId",
                table: "Vehicles",
                column: "VehicleProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CargoItems");

            migrationBuilder.DropTable(
                name: "Placements");

            migrationBuilder.DropTable(
                name: "Rejections");

            migrationBuilder.DropTable(
                name: "Manifests");

            migrationBuilder.DropTable(
                name: "Destinations");

            migrationBuilder.DropTable(
                name: "Vehicles");

            migrationBuilder.DropTable(
                name: "VehicleProfiles");
        }
    }
}
