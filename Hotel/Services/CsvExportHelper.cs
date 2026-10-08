using System.Text;
using Hotel.Storage;

namespace Hotel.Services;

public static class CsvExportHelper
{
    public static void Export<T>(IEnumerable<T> data, string fileName, string userLogin, AuditStore audit)
    {
        var properties = typeof(T).GetProperties();
        var csvBuilder = new StringBuilder();
        var headers = string.Join(";", properties.Select(p => p.Name));
        csvBuilder.AppendLine(headers);
  
        foreach (var item in data)
        {
            var values = properties.Select(p => p.GetValue(item)?.ToString()?.Replace(";", ",") ?? "");
            csvBuilder.AppendLine(string.Join(";", values));
        }
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
        File.WriteAllText(path, csvBuilder.ToString(), Encoding.UTF8);
        audit.LogAction(userLogin, "EXPORT", typeof(T).Name, "All", $"Экспорт в CSV: {path}");
        MessageBox.Show($"Файл сохранён: {path}", "Экспорт", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}