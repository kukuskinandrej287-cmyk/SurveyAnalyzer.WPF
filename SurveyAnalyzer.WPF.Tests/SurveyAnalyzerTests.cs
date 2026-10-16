using NUnit.Framework;
using SurveyAnalyzer;
using System;
using System.Collections.Generic;

namespace SurveyAnalyzer.WPF.Tests
{
    [TestFixture]
    public class SurveyAnalyzerTests
    {
        // ============================================================
        // КОНСТРУКТОР
        // ============================================================
        [Test]
        public void Constructor_NullData_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new SurveyStats(null!));
        }

        [Test]
        public void Constructor_CopiesData()
        {
            double[] data = { 1, 2, 3 };
            var a = new SurveyStats(data);
            data[0] = 99;
            Assert.That(a.Data[0], Is.EqualTo(1));
        }

        // ============================================================
        // СРЕДНЕЕ АРИФМЕТИЧЕСКОЕ
        // ============================================================
        [Test]
        public void Mean_TypicalData_ReturnsCorrect()
        {
            var a = new SurveyStats(new double[] { 1, 2, 3, 4, 5 });
            Assert.That(a.Mean(), Is.EqualTo(3.0).Within(1e-9));
        }

        [Test]
        public void Mean_EmptyArray_Throws()
        {
            var a = new SurveyStats(Array.Empty<double>());
            Assert.Throws<InvalidOperationException>(() => a.Mean());
        }

        [Test]
        public void Mean_SingleElement_ReturnsElement()
        {
            var a = new SurveyStats(new double[] { 7 });
            Assert.That(a.Mean(), Is.EqualTo(7.0).Within(1e-9));
        }

        [Test]
        public void Mean_NegativeValues_ReturnsCorrect()
        {
            var a = new SurveyStats(new double[] { -1, -2, -3 });
            Assert.That(a.Mean(), Is.EqualTo(-2.0).Within(1e-9));
        }

        [Test]
        public void Mean_LargeData_CompletesQuickly()
        {
            var data = SurveyStats.GenerateRandomData(100_000, 1, 10, 1);
            var a = new SurveyStats(data);
            var sw = System.Diagnostics.Stopwatch.StartNew();
            a.Mean();
            sw.Stop();
            Assert.That(sw.ElapsedMilliseconds, Is.LessThan(200));
        }

        // ============================================================
        // МЕДИАНА
        // ============================================================
        [Test]
        public void Median_OddCount_ReturnsMiddle()
        {
            var a = new SurveyStats(new double[] { 3, 1, 2 });
            Assert.That(a.Median(), Is.EqualTo(2.0).Within(1e-9));
        }

        [Test]
        public void Median_EvenCount_ReturnsAverage()
        {
            var a = new SurveyStats(new double[] { 1, 2, 3, 4 });
            Assert.That(a.Median(), Is.EqualTo(2.5).Within(1e-9));
        }

        [Test]
        public void Median_EmptyArray_Throws()
        {
            var a = new SurveyStats(Array.Empty<double>());
            Assert.Throws<InvalidOperationException>(() => a.Median());
        }

        [Test]
        public void Median_SingleElement_ReturnsElement()
        {
            var a = new SurveyStats(new double[] { 5 });
            Assert.That(a.Median(), Is.EqualTo(5.0).Within(1e-9));
        }

        [Test]
        public void Median_AllSameValues_ReturnsValue()
        {
            var a = new SurveyStats(new double[] { 4, 4, 4, 4 });
            Assert.That(a.Median(), Is.EqualTo(4.0).Within(1e-9));
        }

        // ============================================================
        // МОДА
        // ============================================================
        [Test]
        public void Mode_SingleMode_ReturnsCorrect()
        {
            var a = new SurveyStats(new double[] { 1, 2, 2, 3 });
            var modes = a.Mode();
            Assert.That(modes.Count, Is.EqualTo(1));
            Assert.That(modes[0], Is.EqualTo(2));
        }

        [Test]
        public void Mode_MultipleModes_ReturnsAll()
        {
            var a = new SurveyStats(new double[] { 1, 1, 2, 2, 3 });
            var modes = a.Mode();
            Assert.That(modes, Is.EquivalentTo(new List<double> { 1, 2 }));
        }

        [Test]
        public void Mode_EmptyArray_Throws()
        {
            var a = new SurveyStats(Array.Empty<double>());
            Assert.Throws<InvalidOperationException>(() => a.Mode());
        }

        [Test]
        public void Mode_AllSame_ReturnsSingle()
        {
            var a = new SurveyStats(new double[] { 5, 5, 5 });
            var modes = a.Mode();
            Assert.That(modes.Count, Is.EqualTo(1));
            Assert.That(modes[0], Is.EqualTo(5));
        }

        // ============================================================
        // ДИСПЕРСИЯ И СТ. ОТКЛОНЕНИЕ
        // ============================================================
        [Test]
        public void Variance_TypicalData_ReturnsCorrect()
        {
            var a = new SurveyStats(new double[] { 1, 2, 3, 4, 5 });
            Assert.That(a.Variance(), Is.EqualTo(2.0).Within(1e-9));
        }

        [Test]
        public void StdDev_TypicalData_ReturnsCorrect()
        {
            var a = new SurveyStats(new double[] { 1, 2, 3, 4, 5 });
            Assert.That(a.StandardDeviation(), Is.EqualTo(Math.Sqrt(2.0)).Within(1e-9));
        }

        [Test]
        public void Variance_SingleElement_ReturnsZero()
        {
            var a = new SurveyStats(new double[] { 7 });
            Assert.That(a.Variance(), Is.EqualTo(0.0).Within(1e-9));
        }

        // ============================================================
        // МИНИМУМ И МАКСИМУМ
        // ============================================================
        [Test]
        public void MinMax_TypicalData_ReturnsCorrect()
        {
            var a = new SurveyStats(new double[] { 3, 1, 4, 1, 5 });
            Assert.That(a.Min(), Is.EqualTo(1));
            Assert.That(a.Max(), Is.EqualTo(5));
        }

        [Test]
        public void MinMax_SingleElement_ReturnsElement()
        {
            var a = new SurveyStats(new double[] { 42 });
            Assert.That(a.Min(), Is.EqualTo(42));
            Assert.That(a.Max(), Is.EqualTo(42));
        }

        // ============================================================
        // QUICKSORT
        // ============================================================
        [Test]
        public void QuickSort_SortsCorrectly()
        {
            var a = new SurveyStats(new double[] { 5, 2, 8, 1 });
            var sorted = a.GetSortedCopy();
            Assert.That(sorted, Is.EqualTo(new double[] { 1, 2, 5, 8 }));
        }

        [Test]
        public void QuickSort_EmptyArray_NoChange()
        {
            var a = new SurveyStats(Array.Empty<double>());
            var sorted = a.GetSortedCopy();
            Assert.That(sorted.Length, Is.EqualTo(0));
        }

        [Test]
        public void QuickSort_AlreadySorted_NoChange()
        {
            var a = new SurveyStats(new double[] { 1, 2, 3 });
            var sorted = a.GetSortedCopy();
            Assert.That(sorted, Is.EqualTo(new double[] { 1, 2, 3 }));
        }

        [Test]
        public void QuickSort_ReverseSorted_SortsCorrectly()
        {
            var a = new SurveyStats(new double[] { 5, 4, 3, 2, 1 });
            var sorted = a.GetSortedCopy();
            Assert.That(sorted, Is.EqualTo(new double[] { 1, 2, 3, 4, 5 }));
        }

        // ============================================================
        // MERGESORT
        // ============================================================
        [Test]
        public void MergeSort_SortsCorrectly()
        {
            var a = new SurveyStats(new double[] { 3, 1, 4, 1, 5 });
            var sorted = a.GetSortedCopyMerge();
            Assert.That(sorted, Is.EqualTo(new double[] { 1, 1, 3, 4, 5 }));
        }

        [Test]
        public void MergeSort_ReverseSorted_SortsCorrectly()
        {
            var a = new SurveyStats(new double[] { 5, 4, 3, 2, 1 });
            var sorted = a.GetSortedCopyMerge();
            Assert.That(sorted, Is.EqualTo(new double[] { 1, 2, 3, 4, 5 }));
        }

        [Test]
        public void MergeSort_EmptyArray_NoChange()
        {
            var a = new SurveyStats(Array.Empty<double>());
            var sorted = a.GetSortedCopyMerge();
            Assert.That(sorted.Length, Is.EqualTo(0));
        }

        // ============================================================
        // ПОИСК АНОМАЛИЙ (IQR)
        // ============================================================
        [Test]
        public void FindOutliers_TypicalData_ReturnsOutliers()
        {
            var a = new SurveyStats(new double[] { 1, 2, 3, 4, 5, 100 });
            var outliers = a.FindOutliers();
            Assert.That(outliers, Contains.Item(100.0));
        }

        [Test]
        public void FindOutliers_NoOutliers_ReturnsEmpty()
        {
            var a = new SurveyStats(new double[] { 1, 2, 3, 4, 5 });
            var outliers = a.FindOutliers();
            Assert.That(outliers.Count, Is.EqualTo(0));
        }

        [Test]
        public void FindOutliers_EmptyArray_Throws()
        {
            var a = new SurveyStats(Array.Empty<double>());
            Assert.Throws<InvalidOperationException>(() => a.FindOutliers());
        }

        // ============================================================
        // ГЕНЕРАЦИЯ СЛУЧАЙНЫХ ДАННЫХ
        // ============================================================
        [Test]
        public void GenerateRandom_CorrectCount()
        {
            var data = SurveyStats.GenerateRandomData(100, 1, 10, 1);
            Assert.That(data.Length, Is.EqualTo(100));
        }

        [Test]
        public void GenerateRandom_InRange()
        {
            var data = SurveyStats.GenerateRandomData(1000, 1, 10, 1);
            foreach (var x in data)
            {
                Assert.That(x, Is.GreaterThanOrEqualTo(1));
                Assert.That(x, Is.LessThanOrEqualTo(10));
            }
        }

        [Test]
        public void GenerateRandom_SameSeed_SameData()
        {
            var d1 = SurveyStats.GenerateRandomData(50, 1, 10, 42);
            var d2 = SurveyStats.GenerateRandomData(50, 1, 10, 42);
            Assert.That(d1, Is.EqualTo(d2));
        }

        [Test]
        public void GenerateRandom_InvalidCount_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => SurveyStats.GenerateRandomData(0, 1, 10, 1));
        }

        // ============================================================
        // РАСПРЕДЕЛЕНИЕ ЧАСТОТ
        // ============================================================
        [Test]
        public void FrequencyDistribution_TypicalData_ReturnsCorrect()
        {
            var a = new SurveyStats(new double[] { 1, 1, 2, 3, 3, 3 });
            var freq = a.FrequencyDistribution();
            Assert.That(freq[1], Is.EqualTo(2));
            Assert.That(freq[2], Is.EqualTo(1));
            Assert.That(freq[3], Is.EqualTo(3));
        }

        [Test]
        public void FrequencyDistribution_EmptyArray_ReturnsEmpty()
        {
            var a = new SurveyStats(Array.Empty<double>());
            var freq = a.FrequencyDistribution();
            Assert.That(freq.Count, Is.EqualTo(0));
        }
    }
}