using Microsoft.EntityFrameworkCore;
using QuickBite.API.Data;

namespace QuickBite.API.Tests.Helpers
{
    /// <summary>
    /// Helper class for creating and managing test database contexts
    /// </summary>
    public static class TestDbContextFactory
    {
        /// <summary>
        /// Creates a new in-memory database context for testing
        /// </summary>
        /// <param name="databaseName">Optional unique database name. If not provided, a GUID will be used.</param>
        /// <returns>A new QuickBiteDbContext configured for in-memory testing</returns>
        public static QuickBiteDbContext CreateInMemoryContext(string? databaseName = null)
        {
            var options = new DbContextOptionsBuilder<QuickBiteDbContext>()
                .UseInMemoryDatabase(databaseName: databaseName ?? Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .Options;

            return new QuickBiteDbContext(options);
        }

        /// <summary>
        /// Creates a new in-memory database context and seeds it with test data
        /// </summary>
        /// <param name="seedAction">Action to seed the database with test data</param>
        /// <param name="databaseName">Optional unique database name</param>
        /// <returns>A new seeded QuickBiteDbContext</returns>
        public static async Task<QuickBiteDbContext> CreateSeededInMemoryContextAsync(
            Func<QuickBiteDbContext, Task> seedAction,
            string? databaseName = null)
        {
            var context = CreateInMemoryContext(databaseName);
            await seedAction(context);
            await context.SaveChangesAsync();
            return context;
        }

        /// <summary>
        /// Creates a new in-memory database context and seeds it with menu items
        /// </summary>
        /// <param name="menuItemCount">Number of menu items to seed</param>
        /// <param name="databaseName">Optional unique database name</param>
        /// <returns>A new seeded QuickBiteDbContext with menu items</returns>
        public static async Task<QuickBiteDbContext> CreateContextWithMenuItemsAsync(
            int menuItemCount = 10,
            string? databaseName = null)
        {
            return await CreateSeededInMemoryContextAsync(async context =>
            {
                var menuItems = TestDataFactory.CreateMenuItems(menuItemCount);
                await context.MenuItems.AddRangeAsync(menuItems);
            }, databaseName);
        }
    }
}