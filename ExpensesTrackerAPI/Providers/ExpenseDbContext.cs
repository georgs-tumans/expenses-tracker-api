using ExpensesTrackerAPI.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace ExpensesTrackerAPI.Data
{
    public class ExpenseDbContext : DbContext
    {
        public ExpenseDbContext(DbContextOptions<ExpenseDbContext> options) : base(options) { }
        public ExpenseDbContext() : base() { }

        public DbSet<Expense> Expenses { get; set; }
        public DbSet<ExpenseCategory> ExpensesCategories { get; set;}
        public DbSet<Weblog> Weblogs { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserToCategory> UserToCategory { get; set; }
    }
}
