using System.IO;
using System.Windows;
using System.Windows.Input;

namespace lab2
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeCommands();
        }

        // Метод для реєстрації команд
        private void InitializeCommands()
        {
            // 1. Команда Save (для кнопки ОК)
            var saveCommand = new CommandBinding(ApplicationCommands.Save, execute_Save, canExecute_Save);
            CommandBindings.Add(saveCommand);

            // 2. Команда Open (для кнопки Відкрити)
            var openCommand = new CommandBinding(ApplicationCommands.Open, execute_Open, canExecute_Open);
            CommandBindings.Add(openCommand);

            // 3. Команда Delete (для кнопки Стерти)
            var deleteCommand =
                new CommandBinding(ApplicationCommands.Delete, execute_Delete, canExecute_Delete);
            CommandBindings.Add(deleteCommand);
        }

        //Логіка Save
        private void canExecute_Save(object sender, CanExecuteRoutedEventArgs e)
        {
            // Кнопка активна, тільки якщо в полі є текст
            if (txtEditor != null && txtEditor.Text.Trim().Length > 0)
                e.CanExecute = true;
            else
                e.CanExecute = false;
        }

        private void execute_Save(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Текстові файли (*.txt)|*.txt|Всі файли (*.*)|*.*",
                Title = "Зберегти файл",
                FileName = "myFile.txt"
            };

            if (dialog.ShowDialog() != true) return;
            try
            {
                File.WriteAllText(dialog.FileName, txtEditor.Text);
                MessageBox.Show("Файл збережено!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка збереження: " + ex.Message);
            }
        }

        //Логіка Open
        private static void canExecute_Open(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true; // Завжди доступна
        }

        private void execute_Open(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Текстові файли (*.txt)|*.txt|Всі файли (*.*)|*.*",
                Title = "Відкрити файл"
            };

            if (dialog.ShowDialog() != true) return;
            try
            {
                txtEditor.Text = File.ReadAllText(dialog.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка відкриття: " + ex.Message);
            }
        }

        //логіка Delete
        private void canExecute_Delete(object sender, CanExecuteRoutedEventArgs e)
        {
            // Активна, якщо є текст
            e.CanExecute = txtEditor is { Text.Length: > 0 };
        }

        private void execute_Delete(object sender, ExecutedRoutedEventArgs e)
        {
            txtEditor.Clear();
        }
    }
}