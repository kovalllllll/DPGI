using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Windows;

namespace Lab5
{
    public partial class MainWindow : Window
    {
        // Створюємо зв'язок з нашою базою
        Lab5Context db = new Lab5Context();

        public MainWindow()
        {
            InitializeComponent();
            InitializeDatabase();
        }

        // --- ЗАВДАННЯ 2: Ініціалізація та заповнення ---
        private void InitializeDatabase()
        {
            try
            {
                SeedData(); // Заповнюємо базу, якщо вона пуста
                RefreshBaseGrids(); // Виводимо базові таблиці на екран
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при роботі з БД: " + ex.Message);
            }
        }

        private void SeedData()
        {
            if (!db.Units.Any())
            {
                var unit1 = new Units { UnitName = "шт" };
                var unit2 = new Units { UnitName = "кг" };
                var unit3 = new Units { UnitName = "пачка" };

                db.Units.AddRange(new List<Units> { unit1, unit2, unit3 });
                db.SaveChanges();

                var products = new List<Products>
                {
                    new Products { Article = "T-100", UnitCode = unit1.UnitCode, Quantity = 50, Price = 120.50m },
                    new Products { Article = "T-205", UnitCode = unit2.UnitCode, Quantity = 15, Price = 85.00m },
                    new Products { Article = "T-500", UnitCode = unit1.UnitCode, Quantity = 5, Price = 1500.00m },
                    new Products { Article = "M-10", UnitCode = unit3.UnitCode, Quantity = 100, Price = 24.99m }
                };

                db.Products.AddRange(products);
                db.SaveChanges();
            }
        }

        private void RefreshBaseGrids()
        {
            dgProducts.ItemsSource = db.Products.ToList();
            dgUnits.ItemsSource = db.Units.ToList();
        }


        // --- ЗАВДАННЯ 3: ЛІНК-ЗАПИТИ (Кнопки) ---

        // 1. ЗАПИТ JOIN: Вивід товарів з назвою одиниці виміру
        private void BtnJoin_Click(object sender, RoutedEventArgs e)
        {
            var result = from p in db.Products
                         join u in db.Units on p.UnitCode equals u.UnitCode
                         select new
                         {
                             Артикул = p.Article,
                             Назва_Одиниці = u.UnitName,
                             Кількість = p.Quantity,
                             Ціна = p.Price
                         };

            dgQuery1.ItemsSource = result.ToList();
        }

        // 2. ЗАПИТ З ПАРАМЕТРОМ: Фільтрація за ціною
        private void BtnFilter_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(txtMinPrice.Text, out decimal minPrice))
            {
                var filtered = db.Products
                                 .Where(p => p.Price > minPrice)
                                 .ToList();
                dgQuery2.ItemsSource = filtered;
            }
            else
            {
                MessageBox.Show("Будь ласка, введіть коректне число у поле ціни.");
            }
        }

        // 3. АГРЕГАТНИЙ ЗАПИТ: Розрахунок статистики
        private void BtnStats_Click(object sender, RoutedEventArgs e)
        {
            if (db.Products.Any())
            {
                var totalSum = db.Products.Sum(p => p.Quantity * p.Price);
                var totalCount = db.Products.Sum(p => p.Quantity);

                txtStats.Text = $"Усього товарів: {totalCount} шт.\n" +
                                $"Загальна вартість складу: {totalSum:F2} грн.";
            }
        }
    }
}