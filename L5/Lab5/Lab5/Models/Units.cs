using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Lab5
{
    public class Units
    {
        [Key] // Позначаємо первинний ключ
        public int UnitCode { get; set; }
        public string UnitName { get; set; }

        // Зв'язок: у однієї одиниці виміру може бути багато товарів
        public virtual ICollection<Products> Products { get; set; }
    }
}