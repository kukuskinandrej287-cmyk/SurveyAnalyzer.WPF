using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace SurveyAnalyzer.WPF.Windows
{
    public partial class ChartsWindow : Window
    {
        private readonly SurveyStats _analyzer;

        public ChartsWindow(SurveyStats analyzer)
        {
            InitializeComponent();
            _analyzer = analyzer;

            // Перерисовываем графики при изменении размера окна
            SizeChanged += (s, e) => DrawAll();
            Loaded += (s, e) => DrawAll();
        }

        /// <summary>
        /// Рисует все три графика.
        /// </summary>
        private void DrawAll()
        {
            DrawHistogram();
            DrawBoxPlot();
            DrawScatter();
        }

        // ============================================================
        // ГИСТОГРАММА
        // ============================================================
        private void DrawHistogram()
        {
            var canvas = CanvasHistogram;
            canvas.Children.Clear();

            double w = canvas.ActualWidth;
            double h = canvas.ActualHeight;
            if (w < 100 || h < 100) return;

            var freq = _analyzer.FrequencyDistribution();
            if (freq.Count == 0) return;

            var keys = freq.Keys.OrderBy(k => k).ToList();
            int maxCount = freq.Values.Max();

            double leftMargin = 60;
            double rightMargin = 30;
            double topMargin = 30;
            double bottomMargin = 50;
            double plotW = w - leftMargin - rightMargin;
            double plotH = h - topMargin - bottomMargin;

            double barWidth = plotW / keys.Count;

            // Оси
            AddLine(canvas, leftMargin, topMargin, leftMargin, topMargin + plotH, Colors.Black, 1);
            AddLine(canvas, leftMargin, topMargin + plotH, leftMargin + plotW, topMargin + plotH, Colors.Black, 1);

            // Подписи по оси Y (0 и max)
            AddText(canvas, "0", leftMargin - 25, topMargin + plotH - 10, 11);
            AddText(canvas, maxCount.ToString(), leftMargin - 35, topMargin - 5, 11);

            // Столбцы
            for (int i = 0; i < keys.Count; i++)
            {
                int count = freq[keys[i]];
                double barH = (double)count / maxCount * plotH;

                double x = leftMargin + i * barWidth + 1;
                double y = topMargin + plotH - barH;

                var rect = new System.Windows.Shapes.Rectangle
                {
                    Width = Math.Max(1, barWidth - 2),
                    Height = barH,
                    Fill = new SolidColorBrush(Color.FromRgb(74, 144, 194)),
                    Stroke = new SolidColorBrush(Color.FromRgb(44, 95, 141)),
                    StrokeThickness = 1
                };

                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, y);
                canvas.Children.Add(rect);

                // Подпись значения снизу (если столбцов не слишком много)
                if (keys.Count <= 30)
                {
                    AddText(canvas, keys[i].ToString("F0"),
                        x + barWidth / 2 - 6, topMargin + plotH + 5, 10);
                }
            }

            // Заголовок
            AddText(canvas, "Распределение частот", leftMargin, 5, 13, true);
        }

        // ============================================================
        // BOX PLOT
        // ============================================================
        private void DrawBoxPlot()
        {
            var canvas = CanvasBoxPlot;
            canvas.Children.Clear();

            double w = canvas.ActualWidth;
            double h = canvas.ActualHeight;
            if (w < 100 || h < 100) return;

            var data = _analyzer.GetSortedCopy();
            if (data.Length == 0) return;

            double min = data[0];
            double max = data[data.Length - 1];
            double q1 = Percentile(data, 0.25);
            double median = Percentile(data, 0.50);
            double q3 = Percentile(data, 0.75);
            double iqr = q3 - q1;
            double lowerFence = q1 - 1.5 * iqr;
            double upperFence = q3 + 1.5 * iqr;

            var outliers = _analyzer.FindOutliers();

            double leftMargin = 80;
            double rightMargin = 200;
            double topMargin = 40;
            double bottomMargin = 40;
            double plotH = h - topMargin - bottomMargin;
            double plotW = w - leftMargin - rightMargin;
            if (plotH <= 0 || plotW <= 0) return;

            double range = max - min;
            if (range < 1e-9) range = 1;

            Func<double, double> Y = v => topMargin + (1 - (v - min) / range) * plotH;

            double boxLeft = leftMargin + plotW * 0.25;
            double boxWidth = plotW * 0.5;

            // Вертикальная ось
            AddLine(canvas, leftMargin - 20, topMargin, leftMargin - 20, topMargin + plotH, Colors.Gray, 1);

            // Деления Y
            for (int i = 0; i <= 10; i++)
            {
                double val = min + range * i / 10.0;
                double y = Y(val);
                AddLine(canvas, leftMargin - 25, y, leftMargin - 15, y, Colors.Gray, 1);
                AddText(canvas, val.ToString("F1"), 5, y - 8, 10);
            }

            // Усы
            double whiskerLow = Math.Max(min, lowerFence);
            double whiskerHigh = Math.Min(max, upperFence);
            double centerX = boxLeft + boxWidth / 2;

            AddLine(canvas, centerX, Y(whiskerLow), centerX, Y(q1), Colors.Black, 2);
            AddLine(canvas, centerX, Y(q3), centerX, Y(whiskerHigh), Colors.Black, 2);

            // Горизонтальные усики
            AddLine(canvas, centerX - 20, Y(whiskerLow), centerX + 20, Y(whiskerLow), Colors.Black, 2);
            AddLine(canvas, centerX - 20, Y(whiskerHigh), centerX + 20, Y(whiskerHigh), Colors.Black, 2);

            // Ящик
            var box = new System.Windows.Shapes.Rectangle
            {
                Width = boxWidth,
                Height = Math.Abs(Y(q1) - Y(q3)),
                Fill = new SolidColorBrush(Color.FromArgb(180, 180, 210, 240)),
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            Canvas.SetLeft(box, boxLeft);
            Canvas.SetTop(box, Y(q3));
            canvas.Children.Add(box);

            // Медиана
            AddLine(canvas, boxLeft, Y(median), boxLeft + boxWidth, Y(median), Colors.Red, 2);

            // Подписи справа
            AddText(canvas, $"Q1 = {q1:F2}", boxLeft + boxWidth + 10, Y(q1) - 8, 11);
            AddText(canvas, $"Me = {median:F2}", boxLeft + boxWidth + 10, Y(median) - 8, 11, false, Colors.Red);
            AddText(canvas, $"Q3 = {q3:F2}", boxLeft + boxWidth + 10, Y(q3) - 8, 11);
            AddText(canvas, $"min = {min:F2}", boxLeft + boxWidth + 10, Y(whiskerLow) - 8, 11);
            AddText(canvas, $"max = {max:F2}", boxLeft + boxWidth + 10, Y(whiskerHigh) - 8, 11);

            // Выбросы
            foreach (var o in outliers.Distinct())
            {
                var ellipse = new System.Windows.Shapes.Ellipse
                {
                    Width = 8,
                    Height = 8,
                    Fill = Brushes.Red
                };
                Canvas.SetLeft(ellipse, centerX - 4);
                Canvas.SetTop(ellipse, Y(o) - 4);
                canvas.Children.Add(ellipse);
            }

            AddText(canvas, "Ящик с усами (Box Plot)", leftMargin, 5, 13, true);
            if (outliers.Count > 0)
                AddText(canvas, $"Найдено выбросов: {outliers.Count}", 10, h - 20, 11, false, Colors.Red);
        }

        // ============================================================
        // ДИАГРАММА РАССЕЯНИЯ
        // ============================================================
        private void DrawScatter()
        {
            var canvas = CanvasScatter;
            canvas.Children.Clear();

            double w = canvas.ActualWidth;
            double h = canvas.ActualHeight;
            if (w < 100 || h < 100) return;

            var data = _analyzer.Data;
            if (data.Length == 0) return;

            double min = data.Min();
            double max = data.Max();
            double range = max - min;
            if (range < 1e-9) range = 1;

            double leftMargin = 60;
            double rightMargin = 30;
            double topMargin = 30;
            double bottomMargin = 50;
            double plotW = w - leftMargin - rightMargin;
            double plotH = h - topMargin - bottomMargin;

            // Оси
            AddLine(canvas, leftMargin, topMargin, leftMargin, topMargin + plotH, Colors.Black, 1);
            AddLine(canvas, leftMargin, topMargin + plotH, leftMargin + plotW, topMargin + plotH, Colors.Black, 1);

            // Ограничиваем количество точек (макс 2000 для скорости)
            int step = Math.Max(1, data.Length / 2000);

            for (int i = 0; i < data.Length; i += step)
            {
                double x = leftMargin + (double)i / (data.Length - 1) * plotW;
                double y = topMargin + (1 - (data[i] - min) / range) * plotH;

                var dot = new System.Windows.Shapes.Ellipse
                {
                    Width = 4,
                    Height = 4,
                    Fill = new SolidColorBrush(Color.FromArgb(160, 74, 144, 194))
                };
                Canvas.SetLeft(dot, x - 2);
                Canvas.SetTop(dot, y - 2);
                canvas.Children.Add(dot);
            }

            // Подписи по Y
            AddText(canvas, max.ToString("F0"), 5, topMargin - 5, 10);
            AddText(canvas, min.ToString("F0"), 5, topMargin + plotH - 8, 10);
            AddText(canvas, $"Индекс (всего {data.Length}, показано {data.Length / step})",
                    leftMargin, topMargin + plotH + 10, 11);
            AddText(canvas, "Диаграмма рассеяния", leftMargin, 5, 13, true);
        }

        // ============================================================
        // ВСПОМОГАТЕЛЬНЫЕ
        // ============================================================
        private void AddLine(Canvas c, double x1, double y1, double x2, double y2, Color color, double thickness)
        {
            c.Children.Add(new System.Windows.Shapes.Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = new SolidColorBrush(color),
                StrokeThickness = thickness
            });
        }

        private void AddText(Canvas c, string text, double x, double y, double fontSize,
                             bool bold = false, Color? color = null)
        {
            var tb = new TextBlock
            {
                Text = text,
                FontSize = fontSize,
                FontWeight = bold ? FontWeights.Bold : FontWeights.Normal,
                Foreground = new SolidColorBrush(color ?? Colors.Black)
            };
            Canvas.SetLeft(tb, x);
            Canvas.SetTop(tb, y);
            c.Children.Add(tb);
        }

        private double Percentile(double[] sorted, double p)
        {
            double index = p * (sorted.Length - 1);
            int lower = (int)Math.Floor(index);
            int upper = (int)Math.Ceiling(index);
            if (lower == upper) return sorted[lower];
            return sorted[lower] + (index - lower) * (sorted[upper] - sorted[lower]);
        }

        // ============================================================
        // КНОПКИ
        // ============================================================
        private void BtnRefresh_Click(object sender, RoutedEventArgs e) => DrawAll();

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog
            {
                Filter = "PNG изображение (*.png)|*.png",
                FileName = "charts.png"
            };
            if (dlg.ShowDialog() != true) return;

            try
            {
                // Сохраняем всё окно как PNG
                var renderTarget = new RenderTargetBitmap(
                    (int)ActualWidth, (int)ActualHeight,
                    96, 96, PixelFormats.Pbgra32);
                renderTarget.Render(this);

                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderTarget));

                using var fs = new FileStream(dlg.FileName, FileMode.Create);
                encoder.Save(fs);

                MessageBox.Show($"Изображение сохранено:\n{dlg.FileName}",
                                "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message,
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
    }
}