using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BeforeOurTime.Business.Migrations.MSSQL
{
    public partial class initialmigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Icons",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    Location = table.Column<int>(nullable: false),
                    Gzipped = table.Column<bool>(nullable: false),
                    Base64 = table.Column<bool>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Icons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Item_Attribute_Characters_Health",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Max = table.Column<int>(nullable: false),
                    Value = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Item_Attribute_Characters_Health", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    ParentId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Items_Items_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LocationData",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    DataType = table.Column<string>(nullable: true),
                    DataItemId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Item_Attribute_Characters",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AttributeType = table.Column<string>(nullable: true),
                    ItemId = table.Column<Guid>(nullable: false),
                    HealthId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Item_Attribute_Characters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Item_Attribute_Characters_Item_Attribute_Characters_Health_HealthId",
                        column: x => x.HealthId,
                        principalTable: "Item_Attribute_Characters_Health",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Item_Attribute_Characters_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Item_Attribute_Physicals",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ItemId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    ImageIconId = table.Column<Guid>(nullable: true),
                    Height = table.Column<int>(nullable: false),
                    Width = table.Column<int>(nullable: false),
                    Depth = table.Column<int>(nullable: false),
                    Weight = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Item_Attribute_Physicals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Item_Attribute_Physicals_Icons_ImageIconId",
                        column: x => x.ImageIconId,
                        principalTable: "Icons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Item_Attribute_Physicals_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Item_Attribute_Players",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ItemId = table.Column<Guid>(nullable: false),
                    AccountId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Item_Attribute_Players", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Item_Attribute_Players_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Item_Attribute_Visibles",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ItemId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    IconId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Item_Attribute_Visibles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Item_Attribute_Visibles_Icons_IconId",
                        column: x => x.IconId,
                        principalTable: "Icons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Item_Attribute_Visibles_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Item_Attribute_Exits",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ItemId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    DestinationLocationId = table.Column<Guid>(nullable: false),
                    Time = table.Column<int>(nullable: false),
                    Effort = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Item_Attribute_Exits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Item_Attribute_Exits_LocationData_DestinationLocationId",
                        column: x => x.DestinationLocationId,
                        principalTable: "LocationData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Item_Attribute_Exits_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Item_Attribute_Characters_HealthId",
                table: "Item_Attribute_Characters",
                column: "HealthId");

            migrationBuilder.CreateIndex(
                name: "IX_Item_Attribute_Characters_ItemId",
                table: "Item_Attribute_Characters",
                column: "ItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Item_Attribute_Exits_DestinationLocationId",
                table: "Item_Attribute_Exits",
                column: "DestinationLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Item_Attribute_Exits_ItemId",
                table: "Item_Attribute_Exits",
                column: "ItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Item_Attribute_Physicals_ImageIconId",
                table: "Item_Attribute_Physicals",
                column: "ImageIconId");

            migrationBuilder.CreateIndex(
                name: "IX_Item_Attribute_Physicals_ItemId",
                table: "Item_Attribute_Physicals",
                column: "ItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Item_Attribute_Players_ItemId",
                table: "Item_Attribute_Players",
                column: "ItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Item_Attribute_Visibles_IconId",
                table: "Item_Attribute_Visibles",
                column: "IconId");

            migrationBuilder.CreateIndex(
                name: "IX_Item_Attribute_Visibles_ItemId",
                table: "Item_Attribute_Visibles",
                column: "ItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_ParentId",
                table: "Items",
                column: "ParentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Item_Attribute_Characters");

            migrationBuilder.DropTable(
                name: "Item_Attribute_Exits");

            migrationBuilder.DropTable(
                name: "Item_Attribute_Physicals");

            migrationBuilder.DropTable(
                name: "Item_Attribute_Players");

            migrationBuilder.DropTable(
                name: "Item_Attribute_Visibles");

            migrationBuilder.DropTable(
                name: "Item_Attribute_Characters_Health");

            migrationBuilder.DropTable(
                name: "LocationData");

            migrationBuilder.DropTable(
                name: "Icons");

            migrationBuilder.DropTable(
                name: "Items");
        }
    }
}
