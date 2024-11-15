using Ensek.Model;
using Microsoft.EntityFrameworkCore;

namespace Ensek.Context
{
    public class MeterRecordContext :DbContext
    {
        public DbSet<MeterRecord> meterRecords { get; set; }

        public MeterRecordContext (DbContextOptions<MeterRecordContext> options)
                : base(options)
        {

        }



    }


}
