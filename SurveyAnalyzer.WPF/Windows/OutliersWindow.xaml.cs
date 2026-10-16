using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Win32;

namespace SurveyAnalyzer.WPF.Windows
{
    public partial class OutliersWindow : Window
    {
        private readonly SurveyStats _analyzer;

        public OutliersWindow(SurveyStats analyzer)
        {
            InitializeComponent();
            _analyzer = analyzer;
        }

        /// <summary>
        /// Поиск аномалий методом IQR.
        /// </summary>
        private void BtnFind_Click(object sender, RoutedEventArgs e)
        {
            var outliers = _analyzer.FindOutliers();

            TxtSummary.Text = $"Всего наблюдений: {_analyzer.Count}. " +
                              $"Найдено аномалий: {outliers.Count} " +
                              $"({(double)outliers.Count / _analyzer.Count * 100:F2}%).";

            var sb = new StringBuilder();
            sb.AppendLine("=== АНОМАЛИИ (метод межквартильного размаха, IQR) ===");
            sb.AppendLine();
            sb.AppendLine($"Всего наблюдений: {_analyzer.Count}");
            sb.AppendLine($"Найдено аномалий: {outliers.Count}");
            sb.AppendLine($"Доля аномалий:    {(double)outliers.Count / _analyzer.Count * 100:F2}%");
            sb.AppendLine();

            if (outliers.Count > 0)
            {
                sb.AppendLine("Список значений-аномалий:");
                sb.AppendLine(string.Join(", ", outliers.Take(200)));

                if (outliers.Count > 200)
                    sb.AppendLine($"... и ещё {outliers.Count - 200} значений.");
            }
            else
            {
                sb.AppendLine("Аномалий не обнаружено — все значения находятся в допустимом диапазоне.");
            }

            TxtOutliers.Text = sb.ToString();
        }

        /// <summary>
        /// Копирование результата.
        /// </summary>
        private void BtnCopy_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TxtOutliers.Text))
            {
                MessageBox.Show("Сначала найдите аномалии.",
                                "Нет данных", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Clipboard.SetText(TxtOutliers.Text);
            MessageBox.Show("Список аномалий скопирован в буфер обмена.",
                            "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Сохранение в файл.
        /// </summary>
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TxtOutliers.Text))
            {
                MessageBox.Show("Сначала найдите аномалии.",
                                "Нет данных", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dlg = new SaveFileDialog
            {
                Filter = "Текстовый файл (*.txt)|*.txt",
                FileName = "outliers.txt"
            };

            if (dlg.ShowDialog() == true)
            {
                File.WriteAllText(dlg.FileName, TxtOutliers.Text, Encoding.UTF8);
                MessageBox.Show($"Сохранено в файл:\n{dlg.FileName}",
                                "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Закрыть окно.
        /// </summary>
        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
    }
}