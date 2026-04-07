using System.Data;
using System.Windows;

namespace Lab4;

public partial class MainWindow : Window
{
    private readonly AdoAssistant _db = new();

    public MainWindow() => InitializeComponent();

    // Завдання 3 — завантаження при старті
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        ListNazva.DataContext = _db.TableLoad();
        ListNazva.SelectedIndex = 0;
        ListNazva.Focus();
    }

    private void RefreshList()
    {
        ListNazva.DataContext = _db.Reload();
        ListNazva.SelectedIndex = 0;
    }

    // Завдання 4 — Create
    private void BtnCreate_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new EditDialog { Owner = this };
        if (dlg.ShowDialog() == true)
        {
            _db.Insert(dlg.Nazva, dlg.OdVymiru, dlg.Kilkist, dlg.Tsina);
            RefreshList();
        }
    }

    // Завдання 4 — Update
    private void BtnUpdate_Click(object sender, RoutedEventArgs e)
    {
        if (ListNazva.SelectedItem is not DataRowView row) return;

        var dlg = new EditDialog
        {
            Owner = this,
            Nazva = TxtNazva.Text,
            OdVymiru = TxtOdVymiru.Text,
            Kilkist = decimal.TryParse(TxtKilkist.Text, out var k) ? k : 0,
            Tsina = decimal.TryParse(TxtTsina.Text, out var t) ? t : 0
        };

        if (dlg.ShowDialog() == true)
        {
            _db.Update((int)row["Артикул"], dlg.Nazva, dlg.OdVymiru, dlg.Kilkist, dlg.Tsina);
            RefreshList();
        }
    }

    // Завдання 4 — Delete
    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (ListNazva.SelectedItem is not DataRowView row) return;

        var result = MessageBox.Show(
            $"Видалити товар «{row["Назва"]}»?",
            "Підтвердження",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            _db.Delete((int)row["Артикул"]);
            RefreshList();
        }
    }
}