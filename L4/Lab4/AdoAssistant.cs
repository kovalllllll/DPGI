using System.Data;
using Microsoft.Data.SqlClient;
using System.Windows;

namespace Lab4;

public class AdoAssistant
{
    private const string ConnectionString =
        "Data Source=.;Initial Catalog=Lab4;Integrated Security=True;TrustServerCertificate=True;";

    private DataTable? _dt;

    // Завдання 2 — читання даних
    public DataTable TableLoad()
    {
        if (_dt != null) return _dt;

        _dt = new DataTable();
        using var connection = new SqlConnection(ConnectionString);
        var command = connection.CreateCommand();
        command.CommandText = "SELECT Артикул, Назва, ОдВиміру, Кількість, Ціна FROM Товари";
        var adapter = new SqlDataAdapter(command);
        try
        {
            adapter.Fill(_dt);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Помилка підключення до БД:\n" + ex.Message);
        }

        return _dt;
    }

    // Скинути кеш і перечитати
    public DataTable Reload()
    {
        _dt = null;
        return TableLoad();
    }

    // Завдання 4 — Додавання
    public void Insert(string nazva, string odVymiru, decimal kilkist, decimal tsina)
    {
        var sql = $"INSERT INTO Товари (Назва, ОдВиміру, Кількість, Ціна) " +
                  $"VALUES (N'{nazva}', N'{odVymiru}', " +
                  $"{kilkist.ToString(System.Globalization.CultureInfo.InvariantCulture)}, " +
                  $"{tsina.ToString(System.Globalization.CultureInfo.InvariantCulture)})";
        ExecuteNonQuery(sql);
    }

    // Завдання 4 — Оновлення
    public void Update(int artikul, string nazva, string odVymiru, decimal kilkist, decimal tsina)
    {
        var sql = $"UPDATE Товари SET Назва=N'{nazva}', ОдВиміру=N'{odVymiru}', " +
                  $"Кількість={kilkist.ToString(System.Globalization.CultureInfo.InvariantCulture)}, " +
                  $"Ціна={tsina.ToString(System.Globalization.CultureInfo.InvariantCulture)} " +
                  $"WHERE Артикул={artikul}";
        ExecuteNonQuery(sql);
    }

    // Завдання 4 — Видалення
    public void Delete(int artikul) =>
        ExecuteNonQuery($"DELETE FROM Товари WHERE Артикул={artikul}");

    private void ExecuteNonQuery(string sql)
    {
        using var connection = new SqlConnection(ConnectionString);
        var command = new SqlCommand(sql, connection);
        try
        {
            connection.Open();
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Помилка запиту:\n" + ex.Message);
        }
    }
}