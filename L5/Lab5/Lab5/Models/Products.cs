using System.ComponentModel.DataAnnotations;

namespace Lab5
{
    public class Products
    {
        [Key]
        public string Article { get; set; }
        public int UnitCode { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        // Навігаційна властивість (зв'язок з таблицею Units)
        public virtual Units Units { get; set; }
    }
}