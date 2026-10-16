using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace SurveyAnalyzer.WPF.Windows
{
    public partial class StatsWindow : Window
    {
        private readonly SurveyStats _analyzer;
        private string _plainText = "";

        public StatsWindow(SurveyStats analyzer)
        {
            InitializeComponent();
            _analyzer = analyzer;
            BuildMetrics();
        }

        private void BuildMetrics()
        {
            var mean = _analyzer.Mean();
            var median = _analyzer.Median();
            var modes = _analyzer.Mode();
            var variance = _analyzer.Variance();
            var stddev = _analyzer.StandardDeviation();
            var min = _analyzer.Min();
            var max = _analyzer.Max();

            AddMetric("Количество наблюдений (n)", _analyzer.Count.ToString(),
                "Общее число респондентов в выборке.");

            AddMetric("Среднее арифметическое (x̄)", mean.ToString("F4"),
                "Сумма всех значений, делённая на их количество. Чувствительно к выбросам.");

            AddMetric("Медиана (Me)", median.ToString("F4"),
                "Значение, которое делит упорядоченный массив пополам. Устойчиво к выбросам.");

            AddMetric("Мода (Mo)", string.Join(", ", modes),
                "Наиболее часто встречающееся значение (или несколько при мультимодальности).");

            AddMetric("Дисперсия (D)", variance.ToString("F4"),
                "Средний квадрат отклонений от среднего. Мера разброса.");

            AddMetric("Стандартное отклонение (σ)", stddev.ToString("F4"),
                "Квадратный корень из дисперсии. Показывает типичное отклонение от среднего.");

            AddMetric("Минимум", min.ToString("F4"),
                "Наименьшее значение в выборке.");

            AddMetric("Максимум", max.ToString("F4"),
                "Наибольшее значение в выборке.");

            var sb = new StringBuilder();
            sb.AppendLine("=== СТАТИСТИЧЕСКИЕ ХАРАКТЕРИСТИКИ ===");
            sb.AppendLine();
            sb.AppendLine($"Количество наблюдений: {_analyzer.Count}");
            sb.AppendLine($"Среднее арифметическое: {mean:F4}");
            sb.AppendLine($"Медиана:                {median:F4}");
            sb.AppendLine($"Мода:                   {string.Join(", ", modes)}");
            sb.AppendLine($"Дисперсия:              {variance:F4}");
            sb.AppendLine($"Стандартное отклонение: {stddev:F4}");
            sb.AppendLine($"Минимум:                {min:F4}");
            sb.AppendLine($"Максимум:               {max:F4}");
            _plainText = sb.ToString();
        }

        private void AddMetric(string label, string value, string tooltip)
        {
            var border = new Border { Style = (Style)FindResource("MetricRow") };
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var lbl = new TextBlock
            {
                Text = label,
                Style = (Style)FindResource("MetricLabel"),
                ToolTip = tooltip
            };
            Grid.SetColumn(lbl, 0);

            var val = new TextBlock
            {
                Text = value,
                Style = (Style)FindResource("MetricValue"),
                ToolTip = tooltip
            };
            Grid.SetColumn(val, 1);

            grid.Children.Add(lbl);
            grid.Children.Add(val);
            border.Child = grid;
            MetricsPanel.Children.Add(border);
        }

        private void BtnCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(_plainText);
            MessageBox.Show("Все метрики скопированы в буфер обмена.",
                            "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog
            {
                Filter = "Текстовый файл (*.txt)|*.txt",
                FileName = "stats.txt"
            };
            if (dlg.ShowDialog() == true)
            {
                File.WriteAllText(dlg.FileName, _plainText, Encoding.UTF8);
                MessageBox.Show($"Сохранено в файл:\n{dlg.FileName}",
                                "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
    }
}