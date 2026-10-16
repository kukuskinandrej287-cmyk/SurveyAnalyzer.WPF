using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace SurveyAnalyzer.WPF.Windows
{
    public partial class SortWindow : Window
    {
        private readonly SurveyStats _analyzer;

        public SortWindow(SurveyStats analyzer)
        {
            InitializeComponent();
            _analyzer = analyzer;
        }

        /// <summary>
        /// Запуск сортировки: замер времени QuickSort и/или MergeSort.
        /// </summary>
        private void BtnRun_Click(object sender, RoutedEventArgs e)
        {
            var mode = (CmbAlgo.SelectedItem as ComboBoxItem)?.Content?.ToString()
                       ?? "Оба (сравнение)";

            var sb = new StringBuilder();
            sb.AppendLine("=== СОРТИРОВКА ===");
            sb.AppendLine($"Размер массива: {_analyzer.Count}");
            sb.AppendLine();

            // QuickSort
            if (mode == "QuickSort" || mode == "Оба (сравнение)")
            {
                var sw = Stopwatch.StartNew();
                var sorted = _analyzer.GetSortedCopy();
                sw.Stop();

                sb.AppendLine($"QuickSort: {sw.Elapsed.TotalMilliseconds:F4} мс");

                if (mode == "QuickSort")
                {
                    sb.AppendLine();
                    sb.AppendLine("Первые 100 элементов:");
                    sb.AppendLine(string.Join(", ", sorted.Take(100)));
                }
            }

            // MergeSort
            if (mode == "MergeSort" || mode == "Оба (сравнение)")
            {
                var sw = Stopwatch.StartNew();
                var sorted = _analyzer.GetSortedCopyMerge();
                sw.Stop();

                sb.AppendLine($"MergeSort: {sw.Elapsed.TotalMilliseconds:F4} мс");

                if (mode == "MergeSort")
                {
                    sb.AppendLine();
                    sb.AppendLine("Первые 100 элементов:");
                    sb.AppendLine(string.Join(", ", sorted.Take(100)));
                }
            }

            // Сравнение — показываем результат QuickSort
            if (mode == "Оба (сравнение)")
            {
                sb.AppendLine();
                sb.AppendLine("--- Первые 100 элементов (QuickSort) ---");
                sb.AppendLine(string.Join(", ", _analyzer.GetSortedCopy().Take(100)));
            }

            TxtResult.Text = sb.ToString();
        }

        /// <summary>
        /// Копирование результата в буфер обмена.
        /// </summary>
        private void BtnCopy_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TxtResult.Text))
            {
                MessageBox.Show("Сначала запустите сортировку.",
                                "Нет данных",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                return;
            }

            Clipboard.SetText(TxtResult.Text);
            MessageBox.Show("Результат скопирован в буфер обмена.",
                            "Готово",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }

        /// <summary>
        /// Сохранение результата в текстовый файл.
        /// </summary>
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TxtResult.Text))
            {
                MessageBox.Show("Сначала запустите сортировку.",
                                "Нет данных",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                return;
            }

            var dlg = new SaveFileDialog
            {
                Filter = "Текстовый файл (*.txt)|*.txt",
                FileName = "sort.txt"
            };

            if (dlg.ShowDialog() == true)
            {
                File.WriteAllText(dlg.FileName, TxtResult.Text, Encoding.UTF8);
                MessageBox.Show($"Сохранено в файл:\n{dlg.FileName}",
                                "Готово",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Закрыть окно.
        /// </summary>
        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
    }
}