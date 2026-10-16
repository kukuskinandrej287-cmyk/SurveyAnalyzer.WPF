using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Win32;

namespace SurveyAnalyzer.WPF.Windows
{
    public partial class DataPreviewWindow : Window
    {
        private readonly SurveyStats _analyzer;
        private string _plainText = "";

        public DataPreviewWindow(SurveyStats analyzer)
        {
            InitializeComponent();
            _analyzer = analyzer;
            BuildView();
        }

        /// <summary>
        /// Построение превью: массив + таблица частот.
        /// </summary>
        private void BuildView()
        {
            var data = _analyzer.Data;

            // Превью массива (первые 200 значений)
            var previewText = string.Join(", ", data.Take(200).Select(x => x.ToString("F0")));
            if (data.Length > 200)
                previewText += $"\n\n... и ещё {data.Length - 200} значений (всего {data.Length}).";

            TxtRaw.Text = previewText;

            // Таблица частот
            var freq = _analyzer.FrequencyDistribution();
            var rows = freq
                .OrderBy(kv => kv.Key)
                .Select(kv => new FrequencyRow
                {
                    Value = kv.Key.ToString("F0"),
                    Count = kv.Value,
                    Percent = $"{(double)kv.Value / data.Length * 100:F2}"
                })
                .ToList();

            GridFreq.ItemsSource = rows;

            // Plain-text для копирования / сохранения
            var sb = new StringBuilder();
            sb.AppendLine("=== ПРОСМОТР ДАННЫХ ===");
            sb.AppendLine();
            sb.AppendLine($"Всего значений: {data.Length}");
            sb.AppendLine();
            sb.AppendLine("Первые 200 значений:");
            sb.AppendLine(string.Join(", ", data.Take(200).Select(x => x.ToString("F0"))));
            sb.AppendLine();
            sb.AppendLine("Распределение частот:");
            sb.AppendLine("Значение\tЧастота\tДоля, %");
            foreach (var r in rows)
                sb.AppendLine($"{r.Value}\t{r.Count}\t{r.Percent}");
            _plainText = sb.ToString();
        }

        /// <summary>
        /// Копирование данных в буфер.
        /// </summary>
        private void BtnCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(_plainText);
            MessageBox.Show("Данные скопированы в буфер обмена.",
                            "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Сохранение в файл.
        /// </summary>
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog
            {
                Filter = "Текстовый файл (*.txt)|*.txt",
                FileName = "data.txt"
            };

            if (dlg.ShowDialog() == true)
            {
                File.WriteAllText(dlg.FileName, _plainText, Encoding.UTF8);
                MessageBox.Show($"Сохранено в файл:\n{dlg.FileName}",
                                "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Закрыть окно.
        /// </summary>
        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
    }

    /// <summary>
    /// Строка таблицы частот для привязки к DataGrid.
    /// </summary>
    public class FrequencyRow
    {
        public string Value { get; set; } = "";
        public int Count { get; set; }
        public string Percent { get; set; } = "";
    }
}