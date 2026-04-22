using System.ComponentModel.DataAnnotations;

namespace CurrencyConverter.Models
{
    public class ConversionHistory
    {
        [Key] public int Id { get; set; }

        public string CurrencyCode { get; set; } = string.Empty;
        public double ForeignAmount { get; set; }
        public double UahAmount { get; set; }
        public double RateUsed { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

        // Допоміжна властивість для відображення у DataGrid
        public string DisplayTime => Timestamp.ToString("dd.MM.yyyy HH:mm:ss");
    }
}