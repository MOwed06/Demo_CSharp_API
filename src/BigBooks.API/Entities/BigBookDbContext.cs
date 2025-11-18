using Microsoft.EntityFrameworkCore;
using BigBooks.API.Core;

namespace BigBooks.API.Entities
{
    public class BigBookDbContext : DbContext
    {
        public DbSet<Book> Books { get; set; }
        public DbSet<BookReview> BookReviews { get; set; }
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<AccountTransaction> Transactions { get; set; }


        public BigBookDbContext(DbContextOptions<BigBookDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>()
                .HasData(
                new Book
                {
                    Key = 1,
                    Title = "The Stinky Cheese Man and Other Fairly Stupid Tales",
                    Author = "Jon Scieszka",
                    Isbn = Guid.Parse("383EBBB6-8D11-4A7E-8411-07A2994BD3B1"),
                    Genre = Genre.Childrens,
                    Description = "A long time ago, people used to tell magical stories of wonder and enchantment. Those stories were called Fairy Tales.",
                    Price = 17.51m,
                    StockQuantity = 23
                },
                new Book
                {
                    Key = 2,
                    Title = "Where the Wild Things Are",
                    Author = "Maurice Sendak",
                    Isbn = Guid.Parse("31AB208D-EA2D-458B-B708-744E16BBDE5A"),
                    Genre = Genre.Childrens,
                    Description = "Let the wild rumpus continue as this classic comes to life like never before with new reproductions of Maurice Sendak's artwork.",
                    Price = 11.42m,
                    StockQuantity = 17
                },
                new Book
                {
                    Key = 3,
                    Title = "Too Many Frogs",
                    Author = "Sandy Asher",
                    Isbn = Guid.Parse("623BEB0C-3798-4629-9A4E-1894112B1122"),
                    Genre = Genre.Childrens,
                    Description = "Rabbit lives alone. He cooks for himself, cleans up for himself, and at the end of the day, reads himself a story. It's a simple life, and he likes it.",
                    Price = 13.70m,
                    StockQuantity = 45
                },
                new Book
                {
                    Key = 4,
                    Title = "Citizen Soldiers",
                    Author = "Stephen Ambrose",
                    Isbn = Guid.Parse("D454AF8A-FE4A-45AC-8FAB-C220677EA6ED"),
                    Genre = Genre.History,
                    Description = "Citizen Soldiers opens at 0001 hours, June 7, 1944, on the Normandy beaches, and ends at 0245 hours, May 7, 1945, with the allied victory.",
                    Price = 17.11m,
                    StockQuantity = 6
                },
                new Book
                {
                    Key = 5,
                    Title = "The Way of Kings",
                    Author = "Brandon Sanderson",
                    Isbn = Guid.Parse("C0CAD277-F103-4C93-8A15-513FDFEF2D5F"),
                    Genre = Genre.Fantasy,
                    Description = "It has been centuries since the fall of the 10 consecrated orders known as the Knights Radiant, but their Shardblades and Shardplate remain: mystical swords and suits of armor that transform ordinary men into near-invincible warriors. Men trade kingdoms for Shardblades. Wars were fought for them and won by them.",
                    Price = 21.09m,
                    StockQuantity = 21
                },
                new Book
                {
                    Key = 6,
                    Title = "A Gentleman in Moscow",
                    Author = "Amor Towles",
                    Isbn = Guid.Parse("FF347FE2-9B34-41E5-AD04-AFBD9CD2B568"),
                    Genre = Genre.Fiction,
                    Description = "In 1922, Count Alexander Rostov is deemed an unrepentant aristocrat by a Bolshevik tribunal, and is sentenced to house arrest in the Metropol, a grand hotel across the street from the Kremlin. Rostov, an indomitable man of erudition and wit, has never worked a day in his life, and must now live in an attic room.",
                    Price = 11.19m,
                    StockQuantity = 17
                },
                new Book
                {
                    Key = 7,
                    Title = "My Story in BIG LETTERS",
                    Author = "Some Guy",
                    Isbn = Guid.Parse("230ECA9F-474E-45D4-A412-977733CD8893"),
                    Genre = Genre.Fiction,
                    Description = null,
                    Price = 13.19m,
                    StockQuantity = 11
                },
                new Book
                {
                    Key = 8,
                    Title = "Gregor the Overlander",
                    Author = "Suzanne Collins",
                    Isbn = Guid.Parse("9909D495-CFFB-4CF1-B876-CBB9857323D2"),
                    Genre = Genre.Childrens,
                    Description = "When Gregor falls through a grate in the laundry room of his apartment building, he hurtles into the dark Underland, where spiders, rats, cockroaches coexist uneasily with humans. This world is on the brink of war, and Gregor's arrival is no accident. ",
                    Price = 8.29m,
                    StockQuantity = 21
                },
                new Book
                {
                    Key = 9,
                    Title = "Gregor and the Prophecy of Bane",
                    Author = "Suzanne Collins",
                    Isbn = Guid.Parse("09C64A27-F96C-4E3B-8361-DC3C2C8ADA7A"),
                    Genre = Genre.Childrens,
                    Description = "Months have passed since Gregor first fell into the strange Underland beneath New York City, and he swears he will never go back. But he is destined to be a key player in another prophecy, this one about an ominous white rat called the Bane.",
                    Price = 9.46m,
                    StockQuantity = 13
                },
                new Book
                {
                    Key = 10,
                    Title = "The Hunger Games",
                    Author = "Suzanne Collins",
                    Isbn = Guid.Parse("5672C761-C89D-460A-8877-DED4B48525F5"),
                    Genre = Genre.Fantasy,
                    Description = "In the ruins of a place once known as North America lies the nation of Panem, a shining Capitol surrounded by twelve outlying districts. The Capitol keeps the districts in line by forcing them all to send one boy and one girl between the ages of twelve and eighteen to participate in the annual Hunger Games, a fight to the death on live TV.",
                    Price = 13.19m,
                    StockQuantity = 11
                },
                new Book
                {
                    Key = 11,
                    Title = "Triptych: A Will Trent",
                    Author = "Karin Slaughter",
                    Isbn = Guid.Parse("4929C5A8-8F0C-4F1A-A742-154F037C0761"),
                    Genre = Genre.Fantasy,
                    Description = "In the city of Atlanta, young women are dying—at the hands of a killer who signs his work with a single, chilling act of mutilation. Leaving behind enough evidence to fuel a frenzied police hunt, this cunning madman is bringing together dozens of lives, crossing the boundaries of wealth and race.",
                    Price = 19.23m,
                    StockQuantity = 3
                });

            modelBuilder.Entity<BookReview>()
                .HasData(
                new BookReview
                {
                    Key = 1,
                    Score = 6,
                    BookKey = 1,
                    ReviewDate = DateTime.Parse("2023-08-17").Date,
                    UserKey = 4
                },
                new BookReview
                {
                    Key = 2,
                    Score = 9,
                    BookKey = 1,
                    UserKey = 5,
                    Description = "My kids loved this book. I read it to them every night!",
                    ReviewDate = DateTime.Parse("2024-11-03").Date,
                },
                new BookReview
                {
                    Key = 3,
                    Score = 9,
                    BookKey = 3,
                    ReviewDate = DateTime.Parse("2025-01-07").Date,
                },
                new BookReview
                {
                    Key = 4,
                    Score = 6,
                    BookKey = 3,
                    ReviewDate = DateTime.Parse("2025-04-23").Date,
                    Description = "The plot wandered."
                },
                new BookReview
                {
                    Key = 5,
                    Score = 8,
                    BookKey = 4,
                    ReviewDate = DateTime.Parse("2024-05-09").Date,
                },
                new BookReview
                {
                    Key = 6,
                    Score = 9,
                    BookKey = 5,
                    ReviewDate = DateTime.Parse("2024-05-11").Date,
                },
                new BookReview
                {
                    Key = 7,
                    Score = 8,
                    BookKey = 6,
                    ReviewDate = DateTime.Parse("2024-05-12").Date,
                },
                new BookReview
                {
                    Key = 8,
                    Score = 7,
                    BookKey = 6,
                    ReviewDate = DateTime.Parse("2024-05-13").Date,
                },
                new BookReview
                {
                    Key = 9,
                    Score = 7,
                    BookKey = 7,
                    ReviewDate = DateTime.Parse("2024-05-14").Date,
                },
                new BookReview
                {
                    Key = 10,
                    Score = 8,
                    BookKey = 7,
                    ReviewDate = DateTime.Parse("2024-05-15").Date,
                },
                new BookReview
                {
                    Key = 11,
                    Score = 3,
                    BookKey = 9,
                    UserKey = 5,
                    Description = "This book was way too dark for kids.",
                    ReviewDate = DateTime.Parse("2024-05-16").Date,
                },
                new BookReview
                {
                    Key = 12,
                    Score = 10,
                    BookKey = 9,
                    UserKey = 2,
                    Description = "Every child should read this book.",
                    ReviewDate = DateTime.Parse("2024-05-17").Date,
                },
                new BookReview
                {
                    Key = 13,
                    Score = 5,
                    BookKey = 7,
                    UserKey = 5,
                    Description = "This book was long and boring.",
                    ReviewDate = DateTime.Parse("2024-05-18").Date,
                });

            modelBuilder.Entity<AppUser>()
                .HasData(
                new AppUser
                {
                    Key = 1,
                    Role = Role.Admin,
                    UserEmail = "Clark.Kent@demo.com",
                    UserName = "Clark Kent",
                    Password = ApplicationConstant.USER_PASSWORD,
                    Wallet = 1m
                },
                new AppUser
                {
                    Key = 2,
                    Role = Role.Admin,
                    UserEmail = "Bruce.Wayne@demo.com",
                    UserName = "Bruce Wayne",
                    Password = ApplicationConstant.USER_PASSWORD,
                    Wallet = 1m
                },
                new AppUser
                {
                    Key = 3,
                    Role = Role.Customer,
                    UserEmail = "Diana.Prince@demo.com",
                    UserName = "Diana Prince",
                    Password = ApplicationConstant.USER_PASSWORD,
                    Wallet = 16.50m
                },
                new AppUser
                {
                    Key = 4,
                    Role = Role.Customer,
                    UserEmail = "Arthur.Anderson@demo.com",
                    UserName = "Arthur Anderson",
                    Password = ApplicationConstant.USER_PASSWORD,
                    Wallet = 100.0m
                },
                new AppUser
                {
                    Key = 5,
                    Role = Role.Customer,
                    UserEmail = "Bella.Barnes@demo.com",
                    UserName = "Bella Barnes",
                    Password = ApplicationConstant.USER_PASSWORD,
                    Wallet = 50.0m
                },
                new AppUser
                {
                    Key = 6,
                    Role = Role.Customer,
                    UserEmail = "Celeste.Cadwell@demo.com",
                    UserName = "Celeste Cadwell",
                    Password = ApplicationConstant.USER_PASSWORD,
                    Wallet = 20.0m
                });

            modelBuilder.Entity<AccountTransaction>()
                .HasData(
                new AccountTransaction
                {
                    Key = 1,
                    TransactionDate = DateTime.Parse("2025-03-15").Date,
                    UserKey = 4,
                    TransactionAmount = -17.23m,
                    TransactionConfirmation = Guid.Parse("962B4F1A-7520-4392-BFA7-11BEA52517E8"),
                    BookKey = 1,
                    PurchaseQuantity = 1
                },
                new AccountTransaction
                {
                    Key = 2,
                    TransactionDate = DateTime.Parse("2025-03-16").Date,
                    UserKey = 4,
                    TransactionAmount = 50.00m,
                    TransactionConfirmation = Guid.Parse("DB9B9784-CA26-4869-9A84-AF2DE7886F00"),
                    BookKey = null,
                    PurchaseQuantity = null
                },
                new AccountTransaction
                {
                    Key = 3,
                    TransactionDate = DateTime.Parse("2025-03-17").Date,
                    UserKey = 4,
                    TransactionAmount = -13.91m,
                    TransactionConfirmation = Guid.Parse("962B4F1A-7520-4392-BFA7-11BEA52517E8"),
                    BookKey = 3,
                    PurchaseQuantity = 1
                },
                new AccountTransaction
                {
                    Key = 4,
                    TransactionDate = DateTime.Parse("2025-04-01").Date,
                    UserKey = 5,
                    TransactionAmount = -33.21m,
                    TransactionConfirmation = Guid.Parse("491ADFCB-D596-4B46-8D6C-C395E8EC3611"),
                    BookKey = 2,
                    PurchaseQuantity = 3
                },
                new AccountTransaction
                {
                    Key = 5,
                    TransactionDate = DateTime.Parse("2025-04-02").Date,
                    UserKey = 5,
                    TransactionAmount = -16.71m,
                    TransactionConfirmation = Guid.Parse("10A61E73-FADA-472A-9589-3D784D190264"),
                    BookKey = 8,
                    PurchaseQuantity = 2
                },
                new AccountTransaction
                {
                    Key = 6,
                    TransactionDate = DateTime.Parse("2025-04-03").Date,
                    UserKey = 5,
                    TransactionAmount = -9.07m,
                    TransactionConfirmation = Guid.Parse("3A3532DE-CDB2-47B5-ADD3-209B4BA0991E"),
                    BookKey = 9,
                    PurchaseQuantity = 1
                },
                new AccountTransaction
                {
                    // same book, same user, different purchase date
                    Key = 7,
                    TransactionDate = DateTime.Parse("2025-06-03").Date,
                    UserKey = 5,
                    TransactionAmount = -9.17m,
                    TransactionConfirmation = Guid.Parse("69E93A9D-9D39-49FC-967A-4F0A3916A0BA"),
                    BookKey = 9,
                    PurchaseQuantity = 1
                },
                new AccountTransaction
                {
                    Key = 8,
                    TransactionDate = DateTime.Parse("2025-06-04").Date,
                    UserKey = 5,
                    TransactionAmount = 75.00m,
                    TransactionConfirmation = Guid.Parse("1A6B39CB-131A-4B34-AA98-1C3E00327302"),
                    BookKey = null,
                    PurchaseQuantity = null
                });

            base.OnModelCreating(modelBuilder);
        }

    }
}
