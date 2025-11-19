using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BigBooks.API.Migrations
{
    /// <inheritdoc />
    public partial class AccountTransactionRestructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppUsers",
                columns: table => new
                {
                    Key = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    UserEmail = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Wallet = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUsers", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    Key = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Author = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Genre = table.Column<int>(type: "INTEGER", nullable: false),
                    Price = table.Column<decimal>(type: "TEXT", nullable: false),
                    StockQuantity = table.Column<int>(type: "INTEGER", nullable: false),
                    Isbn = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "BookReviews",
                columns: table => new
                {
                    Key = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Score = table.Column<int>(type: "INTEGER", nullable: false),
                    ReviewDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    UserKey = table.Column<int>(type: "INTEGER", nullable: true),
                    BookKey = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookReviews", x => x.Key);
                    table.ForeignKey(
                        name: "FK_BookReviews_AppUsers_UserKey",
                        column: x => x.UserKey,
                        principalTable: "AppUsers",
                        principalColumn: "Key");
                    table.ForeignKey(
                        name: "FK_BookReviews_Books_BookKey",
                        column: x => x.BookKey,
                        principalTable: "Books",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Key = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TransactionDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TransactionAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    TransactionConfirmation = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserKey = table.Column<int>(type: "INTEGER", nullable: false),
                    BookKey = table.Column<int>(type: "INTEGER", nullable: true),
                    PurchaseQuantity = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Key);
                    table.ForeignKey(
                        name: "FK_Transactions_AppUsers_UserKey",
                        column: x => x.UserKey,
                        principalTable: "AppUsers",
                        principalColumn: "Key",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transactions_Books_BookKey",
                        column: x => x.BookKey,
                        principalTable: "Books",
                        principalColumn: "Key");
                });

            migrationBuilder.InsertData(
                table: "AppUsers",
                columns: new[] { "Key", "Password", "Role", "UserEmail", "UserName", "Wallet" },
                values: new object[,]
                {
                    { 1, "N0tV3ryS3cret", -1, "Clark.Kent@demo.com", "Clark Kent", 1m },
                    { 2, "N0tV3ryS3cret", -1, "Bruce.Wayne@demo.com", "Bruce Wayne", 1m },
                    { 3, "N0tV3ryS3cret", 0, "Diana.Prince@demo.com", "Diana Prince", 16.50m },
                    { 4, "N0tV3ryS3cret", 0, "Arthur.Anderson@demo.com", "Arthur Anderson", 100.0m },
                    { 5, "N0tV3ryS3cret", 0, "Bella.Barnes@demo.com", "Bella Barnes", 50.0m },
                    { 6, "N0tV3ryS3cret", 0, "Celeste.Cadwell@demo.com", "Celeste Cadwell", 20.0m }
                });

            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "Key", "Author", "Description", "Genre", "Isbn", "Price", "StockQuantity", "Title" },
                values: new object[,]
                {
                    { 1, "Jon Scieszka", "A long time ago, people used to tell magical stories of wonder and enchantment. Those stories were called Fairy Tales.", 2, new Guid("383ebbb6-8d11-4a7e-8411-07a2994bd3b1"), 17.51m, 23, "The Stinky Cheese Man and Other Fairly Stupid Tales" },
                    { 2, "Maurice Sendak", "Let the wild rumpus continue as this classic comes to life like never before with new reproductions of Maurice Sendak's artwork.", 2, new Guid("31ab208d-ea2d-458b-b708-744e16bbde5a"), 11.42m, 17, "Where the Wild Things Are" },
                    { 3, "Sandy Asher", "Rabbit lives alone. He cooks for himself, cleans up for himself, and at the end of the day, reads himself a story. It's a simple life, and he likes it.", 2, new Guid("623beb0c-3798-4629-9a4e-1894112b1122"), 13.70m, 45, "Too Many Frogs" },
                    { 4, "Stephen Ambrose", "Citizen Soldiers opens at 0001 hours, June 7, 1944, on the Normandy beaches, and ends at 0245 hours, May 7, 1945, with the allied victory.", 5, new Guid("d454af8a-fe4a-45ac-8fab-c220677ea6ed"), 17.11m, 6, "Citizen Soldiers" },
                    { 5, "Brandon Sanderson", "It has been centuries since the fall of the 10 consecrated orders known as the Knights Radiant, but their Shardblades and Shardplate remain: mystical swords and suits of armor that transform ordinary men into near-invincible warriors. Men trade kingdoms for Shardblades. Wars were fought for them and won by them.", 3, new Guid("c0cad277-f103-4c93-8a15-513fdfef2d5f"), 21.09m, 21, "The Way of Kings" },
                    { 6, "Amor Towles", "In 1922, Count Alexander Rostov is deemed an unrepentant aristocrat by a Bolshevik tribunal, and is sentenced to house arrest in the Metropol, a grand hotel across the street from the Kremlin. Rostov, an indomitable man of erudition and wit, has never worked a day in his life, and must now live in an attic room.", 1, new Guid("ff347fe2-9b34-41e5-ad04-afbd9cd2b568"), 11.19m, 17, "A Gentleman in Moscow" },
                    { 7, "Some Guy", null, 1, new Guid("230eca9f-474e-45d4-a412-977733cd8893"), 13.19m, 11, "My Story in BIG LETTERS" },
                    { 8, "Suzanne Collins", "When Gregor falls through a grate in the laundry room of his apartment building, he hurtles into the dark Underland, where spiders, rats, cockroaches coexist uneasily with humans. This world is on the brink of war, and Gregor's arrival is no accident. ", 2, new Guid("9909d495-cffb-4cf1-b876-cbb9857323d2"), 8.29m, 21, "Gregor the Overlander" },
                    { 9, "Suzanne Collins", "Months have passed since Gregor first fell into the strange Underland beneath New York City, and he swears he will never go back. But he is destined to be a key player in another prophecy, this one about an ominous white rat called the Bane.", 2, new Guid("09c64a27-f96c-4e3b-8361-dc3c2c8ada7a"), 9.46m, 13, "Gregor and the Prophecy of Bane" },
                    { 10, "Suzanne Collins", "In the ruins of a place once known as North America lies the nation of Panem, a shining Capitol surrounded by twelve outlying districts. The Capitol keeps the districts in line by forcing them all to send one boy and one girl between the ages of twelve and eighteen to participate in the annual Hunger Games, a fight to the death on live TV.", 3, new Guid("5672c761-c89d-460a-8877-ded4b48525f5"), 13.19m, 11, "The Hunger Games" },
                    { 11, "Karin Slaughter", "In the city of Atlanta, young women are dying—at the hands of a killer who signs his work with a single, chilling act of mutilation. Leaving behind enough evidence to fuel a frenzied police hunt, this cunning madman is bringing together dozens of lives, crossing the boundaries of wealth and race.", 3, new Guid("4929c5a8-8f0c-4f1a-a742-154f037c0761"), 19.23m, 3, "Triptych: A Will Trent" }
                });

            migrationBuilder.InsertData(
                table: "BookReviews",
                columns: new[] { "Key", "BookKey", "Description", "ReviewDate", "Score", "UserKey" },
                values: new object[,]
                {
                    { 1, 1, null, new DateTime(2023, 8, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), 6, 4 },
                    { 2, 1, "My kids loved this book. I read it to them every night!", new DateTime(2024, 11, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), 9, 5 },
                    { 3, 3, null, new DateTime(2025, 1, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), 9, null },
                    { 4, 3, "The plot wandered.", new DateTime(2025, 4, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), 6, null },
                    { 5, 4, null, new DateTime(2024, 5, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), 8, null },
                    { 6, 5, null, new DateTime(2024, 5, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), 9, null },
                    { 7, 6, null, new DateTime(2024, 5, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), 8, null },
                    { 8, 6, null, new DateTime(2024, 5, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), 7, null },
                    { 9, 7, null, new DateTime(2024, 5, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), 7, null },
                    { 10, 7, null, new DateTime(2024, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 8, null },
                    { 11, 9, "This book was way too dark for kids.", new DateTime(2024, 5, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, 5 },
                    { 12, 9, "Every child should read this book.", new DateTime(2024, 5, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), 10, 2 },
                    { 13, 7, "This book was long and boring.", new DateTime(2024, 5, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, 5 }
                });

            migrationBuilder.InsertData(
                table: "Transactions",
                columns: new[] { "Key", "BookKey", "PurchaseQuantity", "TransactionAmount", "TransactionConfirmation", "TransactionDate", "UserKey" },
                values: new object[,]
                {
                    { 1, 1, 1, -17.23m, new Guid("962b4f1a-7520-4392-bfa7-11bea52517e8"), new DateTime(2025, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 4 },
                    { 2, null, null, 50.00m, new Guid("db9b9784-ca26-4869-9a84-af2de7886f00"), new DateTime(2025, 3, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), 4 },
                    { 3, 3, 1, -13.91m, new Guid("962b4f1a-7520-4392-bfa7-11bea52517e8"), new DateTime(2025, 3, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), 4 },
                    { 4, 2, 3, -33.21m, new Guid("491adfcb-d596-4b46-8d6c-c395e8ec3611"), new DateTime(2025, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5 },
                    { 5, 8, 2, -16.71m, new Guid("10a61e73-fada-472a-9589-3d784d190264"), new DateTime(2025, 4, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), 5 },
                    { 6, 9, 1, -9.07m, new Guid("3a3532de-cdb2-47b5-add3-209b4ba0991e"), new DateTime(2025, 4, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), 5 },
                    { 7, 9, 1, -9.17m, new Guid("69e93a9d-9d39-49fc-967a-4f0a3916a0ba"), new DateTime(2025, 6, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), 5 },
                    { 8, null, null, 75.00m, new Guid("1a6b39cb-131a-4b34-aa98-1c3e00327302"), new DateTime(2025, 6, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), 5 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookReviews_BookKey",
                table: "BookReviews",
                column: "BookKey");

            migrationBuilder.CreateIndex(
                name: "IX_BookReviews_UserKey",
                table: "BookReviews",
                column: "UserKey");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_BookKey",
                table: "Transactions",
                column: "BookKey");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_UserKey",
                table: "Transactions",
                column: "UserKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookReviews");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "AppUsers");

            migrationBuilder.DropTable(
                name: "Books");
        }
    }
}
