using Financial.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Financial.DAL
{
    public class FinancialContext : IdentityDbContext<AppUser>
    {
        private readonly DbContextOptions<FinancialContext> _options;
        public FinancialContext(DbContextOptions<FinancialContext> options) : base(options)
        {
            _options = options;
        }

        public DbSet<Item> Items { get; set; } = default!;
        public DbSet<Account> Accounts { get; set; } = default!;
        public DbSet<Transaction> Transactions { get; set; } = default!;
        public DbSet<Schedule> Schedules { get; set; } = default!;
        public DbSet<Generated> Generateds { get; set; } = default!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Item>()
                .HasOne(i => i.User)
                .WithMany(u => u.Items)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Account>()
                .HasOne(a => a.Item)
                .WithMany(i => i.Accounts)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Transaction>()
                .HasKey(t => new { t.Id, t.AccountId });

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Account)
                .WithMany(a => a.Transactions)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Schedule>()
                .HasOne(t => t.Account)
                .WithMany(a => a.Schedules)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Generated>()
                .HasOne(t => t.Schedule)
                .WithMany(a => a.Generateds)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}