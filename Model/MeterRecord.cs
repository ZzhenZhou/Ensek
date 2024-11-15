using System.ComponentModel.DataAnnotations;

namespace Ensek.Model
{
    public class MeterRecord
    {
        [Key]
        public int AccountId { get; set; }

        public DateTime? MeterReadingDateTime { get; set; }

        public int? MeterReadValue { get; set; }
    }
}
