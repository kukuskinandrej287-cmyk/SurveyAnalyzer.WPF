using System;
using System.Windows;
using System.Windows.Controls;
using SurveyAnalyzer.WPF.Windows;

namespace SurveyAnalyzer.WPF
{
    public partial class MainWindow : Window
    {
        private SurveyStats? _analyzer;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SliderCount_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TxtCount != null)
                TxtCount.Text = ((int)e.NewValue).ToString();
        }

        private void TxtCount_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(TxtCount.Text, out int v) && v >= 10 && v <= 100000)
            {
                if (SliderCount != null)
                    SliderCount.Value = v;
            }
        }

        private void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(TxtCount.Text, out int count) || count < 10 || count > 100000)
            {
                MessageBox.Show("Введите количество респондентов от 10 до 100 000.",
                                "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(TxtMin.Text, out int min) ||
                !int.TryParse(TxtMax.Text, out int max) ||
                min >= max)
            {
                MessageBox.Show("Проверьте диапазон оценок: минимум должен быть меньше максимума.",
                                "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var data = SurveyStats.GenerateRandomData(count, min, max, seed: 42);
            _analyzer = new SurveyStats(data);

            TxtStatus.Text = $"✅ Сгенерировано {count} оценок в диапазоне {min}..{max}. " +
                             $"Среднее: {_analyzer.Mean():F2}, медиана: {_analyzer.Median():F2}.";
        }

        private bool EnsureData()
        {
            if (_analyzer == null)
            {
                MessageBox.Show("Данные ещё не сгенерированы. Нажмите «🎲 Сгенерировать данные».",
                                "Нет данных", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            return true;
        }

        private void BtnOpenStats_Click(object sender, RoutedEventArgs e)
        {
            if (!EnsureData()) return;
            var w = new StatsWindow(_analyzer!) { Owner = this };
            w.ShowDialog();
            TxtStatus.Text = "📈 Окно статистики закрыто.";
        }

        private void BtnOpenSort_Click(object sender, RoutedEventArgs e)
        {
            if (!EnsureData()) return;
            var w = new SortWindow(_analyzer!) { Owner = this };
            w.ShowDialog();
            TxtStatus.Text = "🔀 Окно сортировки закрыто.";
        }

        private void BtnOpenOutliers_Click(object sender, RoutedEventArgs e)
        {
            if (!EnsureData()) return;
            var w = new OutliersWindow(_analyzer!) { Owner = this };
            w.ShowDialog();
            TxtStatus.Text = "⚠️ Окно аномалий закрыто.";
        }

        private void BtnOpenData_Click(object sender, RoutedEventArgs e)
        {
            if (!EnsureData()) return;
            var w = new DataPreviewWindow(_analyzer!) { Owner = this };
            w.ShowDialog();
            TxtStatus.Text = "📋 Окно данных закрыто.";
        }

        private void BtnOpenCharts_Click(object sender, RoutedEventArgs e)
        {
            if (!EnsureData()) return;
            var w = new ChartsWindow(_analyzer!) { Owner = this };
            w.ShowDialog();
            TxtStatus.Text = "📊 Окно графиков закрыто.";
        }

        private void BtnHelp_Click(object sender, RoutedEventArgs e)
        {
            var w = new HelpWindow { Owner = this };
            w.ShowDialog();
            TxtStatus.Text = "❓ Окно справки закрыто.";
        }
    }
}