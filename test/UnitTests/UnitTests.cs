using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SimpleInventory.Common.Classes;
using SimpleInventory.Common.Enums;
using SimpleInventory.DataAccess;
using SimpleInventory.DataAccess.Classes;
using SimpleInventory.DataAccess.DAOs;
using SimpleInventory.DataAccess.Intefaces;

namespace UnitTests
{
    [TestClass]
    public class InventoryTests
    {
        private static (SimpleInventoryContext ctx, ISimpleInventoryRepository repo, SqliteConnection conn)
            CreateRepo()
        {
            var conn = new SqliteConnection("Data Source=:memory:");
            conn.Open();

            var options = new DbContextOptionsBuilder<SimpleInventoryContext>()
                .UseSqlite(conn)
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging()
                .Options;

            var ctx = new SimpleInventoryContext(options);

            // Create schema based on model
            ctx.Database.EnsureCreated();

            var repo = new SimpleInventoryRepository(ctx);
            return (ctx, repo, conn);
        }

        [TestMethod]
        public async Task ProductCreation_ShouldReject_NegativePrice_And_DuplicateSku()
        {
            var (ctx, repo, conn) = CreateRepo();
            try
            {
                var cat = new CategoryDAO { Name = "Electronics" };
                ctx.Categories.Add(cat);
                await ctx.SaveChangesAsync();

                var bad = new Product
                {
                    Sku = "ABC123",
                    Name = "Bad Product",
                    Price = -1m,
                    Quantity = 1,
                    CategoryId = cat.Id
                };

                try
                {
                    await repo.CreateProductAsync(bad);
                    Assert.Fail("Expected an exception for negative price.");
                }
                catch (ArgumentException ex)
                {
                    StringAssert.Contains(ex.Message, "Price");
                }

                var p1 = new Product
                {
                    Sku = "SKU-001",
                    Name = "Phone",
                    Price = 199.99m,
                    Quantity = 10,
                    CategoryId = cat.Id
                };
                var created = await repo.CreateProductAsync(p1);
                Assert.IsTrue(created.Id > 0);

                var p2 = new Product
                {
                    Sku = "SKU-001",
                    Name = "Phone 2",
                    Price = 299.99m,
                    Quantity = 5,
                    CategoryId = cat.Id
                };

                try
                {
                    await repo.CreateProductAsync(p2);
                    Assert.Fail("Expected an exception for duplicate SKU.");
                }
                catch (InvalidOperationException ex)
                {
                    StringAssert.Contains(ex.Message, "unique", StringComparison.OrdinalIgnoreCase);
                }
                catch (DbUpdateException)
                {
                    Assert.IsTrue(true);
                }
            }
            finally
            {
                await conn.DisposeAsync();
                await ctx.DisposeAsync();
            }
        }

        [TestMethod]
        public async Task Products_Filtering_Search_Paging_Sorting_Works()
        {
            var (ctx, repo, conn) = CreateRepo();
            try
            {
                var catA = new CategoryDAO { Name = "A" };
                var catB = new CategoryDAO { Name = "B" };
                ctx.Categories.AddRange(catA, catB);
                await ctx.SaveChangesAsync();

                ctx.Products.AddRange(
                    new ProductDAO { Sku = "AAA-111", Name = "Pro Mouse", Price = 20, Quantity = 5, CategoryId = catA.Id },
                    new ProductDAO { Sku = "BBB-222", Name = "Basic Mouse", Price = 10, Quantity = 8, CategoryId = catA.Id },
                    new ProductDAO { Sku = "CCC-333", Name = "Pro Keyboard", Price = 50, Quantity = 3, CategoryId = catB.Id },
                    new ProductDAO { Sku = "DDD-444", Name = "Cable", Price = 5, Quantity = 50, CategoryId = catB.Id }
                );
                await ctx.SaveChangesAsync();

                var optsSearch = new ProductQueryOptions
                {
                    Search = "pro",
                    Page = 1,
                    PageSize = 10,
                    Sort = ProductSort.NameAsc
                };
                var resSearch = await repo.GetProductsAsync(optsSearch);
                Assert.AreEqual(2, resSearch.TotalItems);
                Assert.IsTrue(resSearch.Items.All(p => p.Name.Contains("Pro", StringComparison.OrdinalIgnoreCase)));

                var optsFilter = new ProductQueryOptions
                {
                    CategoryId = catB.Id,
                    Page = 1,
                    PageSize = 10,
                    Sort = ProductSort.PriceDesc
                };
                var resFilter = await repo.GetProductsAsync(optsFilter);
                Assert.AreEqual(2, resFilter.TotalItems);
                Assert.IsTrue(resFilter.Items.First().Price >= resFilter.Items.Last().Price);

                var optsPage = new ProductQueryOptions
                {
                    Page = 2,
                    PageSize = 1,
                    Sort = ProductSort.NameAsc
                };
                var resPage = await repo.GetProductsAsync(optsPage);
                Assert.AreEqual(4, resPage.TotalItems);
                Assert.AreEqual(1, resPage.Items.Count);
                Assert.AreEqual(2, resPage.Page);
            }
            finally
            {
                await conn.DisposeAsync();
                await ctx.DisposeAsync();
            }
        }

        [TestMethod]
        public async Task CategoryDeletion_ShouldBeBlocked_WhenProductsExist()
        {
            var (ctx, repo, conn) = CreateRepo();
            try
            {
                var cat = new CategoryDAO { Name = "Cables" };
                ctx.Categories.Add(cat);
                await ctx.SaveChangesAsync();

                ctx.Products.Add(new ProductDAO
                {
                    Sku = "CAB-001",
                    Name = "HDMI Cable",
                    Price = 9.99m,
                    Quantity = 100,
                    CategoryId = cat.Id
                });
                await ctx.SaveChangesAsync();

                var attempt = await repo.DeleteCategoryAsync(new Category { Id = cat.Id, Name = cat.Name });
                Assert.IsFalse(attempt, "Delete should be blocked when products exist.");

                var emptyCat = await repo.CreateCategoryAsync(new Category { Name = "Empty" });
                var deletedOk = await repo.DeleteCategoryAsync(emptyCat);
                Assert.IsTrue(deletedOk, "Delete should succeed when no products reference the category.");
            }
            finally
            {
                await conn.DisposeAsync();
                await ctx.DisposeAsync();
            }
        }
    }
}