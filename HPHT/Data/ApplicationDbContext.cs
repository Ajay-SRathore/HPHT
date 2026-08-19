using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HPHT.Models;
using System.Xml;

namespace HPHT.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<HPHT.Models.Clients> Clients { get; set; } = default!;
        public DbSet<HPHT.Models.Issues> Issues { get; set; } = default!;
        public DbSet<RepeatHistory> RepeatHistories
        {
            get;
            set;
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Issues>()
                .HasOne(i => i.Client)
                .WithMany(c => c.Issues)
                .HasForeignKey(i => i.ClientCode)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Clients>().HasKey(e => e.ClientCode);
            modelBuilder.Entity<RepeatHistory>()
.HasOne(x => x.Issue)
.WithMany(x => x.RepeatHistories)
.HasForeignKey(x => x.IssueId)
.OnDelete(DeleteBehavior.Cascade);
        }
    }
}
