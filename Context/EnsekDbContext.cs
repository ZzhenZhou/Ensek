using Ensek.Model;
using Microsoft.EntityFrameworkCore;

namespace Ensek
{
    public class EnsekDbContext :DbContext
    {

        public EnsekDbContext (DbContextOptions<EnsekDbContext> options)
                : base(options) 
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MeterRecord>(mr =>
            {
                mr.HasKey(mr => new { mr.AccountId,mr.MeterReadingDateTime});
            });

            modelBuilder.Entity<AccountRecord>().HasNoKey();
        
        }
        public DbSet<MeterRecord> MeterReadings { get; set; }
        public DbSet<AccountRecord> accountrecords { get; set; }

    }


}
