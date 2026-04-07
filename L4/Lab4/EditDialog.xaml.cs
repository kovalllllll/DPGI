using System.Windows;

namespace Lab4;

public partial class EditDialog : Window
{
    public string  Nazva    { get => TbNazva.Text;    set => TbNazva.Text    = value; }
    public string  OdVymiru { get => TbOdVymiru.Text; set => TbOdVymiru.Text = value; }
    public decimal Kilkist
    {
        get => decimal.TryParse(TbKilkist.Text, out var v) ? v : 0;
        set => TbKilkist.Text = value.ToString();
    }
    public decimal Tsina
    {
        get => decimal.TryParse(TbTsina.Text, out var v) ? v : 0;
        set => TbTsina.Text = value.ToString();
    }

    public EditDialog() => InitializeComponent();

    private void OkClick(object sender, RoutedEventArgs e) => DialogResult = true;
}