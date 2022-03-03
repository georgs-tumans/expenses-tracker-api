﻿using ExpensesTrackerAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpensesTrackerAPI.Data
{
    public class ExpenseDbContext : DbContext
    {
        public ExpenseDbContext(DbContextOptions<ExpenseDbContext> options) : base(options) { }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<ExpenseCategory> ExpensesCategories { get; set;}
    }
}