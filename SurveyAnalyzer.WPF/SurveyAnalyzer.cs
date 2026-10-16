using System;
using System.Collections.Generic;

namespace SurveyAnalyzer
{
    /// <summary>
    /// Ядро программы: статистический анализ данных опроса.
    /// Не зависит от UI, может тестироваться отдельно.
    /// </summary>
    public class SurveyStats
    {
        private readonly double[] _data;

        public SurveyStats(double[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            _data = (double[])data.Clone();
        }

        public int Count => _data.Length;
        public double[] Data => (double[])_data.Clone();

        public double Mean()
        {
            if (_data.Length == 0)
                throw new InvalidOperationException("Массив данных пуст.");
            double sum = 0;
            foreach (double x in _data) sum += x;
            return sum / _data.Length;
        }

        public double Median()
        {
            if (_data.Length == 0)
                throw new InvalidOperationException("Массив данных пуст.");
            double[] sorted = GetSortedCopy();
            int n = sorted.Length;
            if (n % 2 == 1)
                return sorted[n / 2];
            return (sorted[n / 2 - 1] + sorted[n / 2]) / 2.0;
        }

        public List<double> Mode()
        {
            if (_data.Length == 0)
                throw new InvalidOperationException("Массив данных пуст.");

            var freq = new Dictionary<double, int>();
            foreach (double x in _data)
            {
                if (freq.ContainsKey(x)) freq[x]++;
                else freq[x] = 1;
            }

            int maxFreq = 0;
            foreach (var kv in freq)
                if (kv.Value > maxFreq) maxFreq = kv.Value;

            var modes = new List<double>();
            foreach (var kv in freq)
                if (kv.Value == maxFreq) modes.Add(kv.Key);

            modes.Sort();
            return modes;
        }

        public double Variance()
        {
            if (_data.Length == 0)
                throw new InvalidOperationException("Массив данных пуст.");
            double mean = Mean();
            double sumSq = 0;
            foreach (double x in _data)
                sumSq += (x - mean) * (x - mean);
            return sumSq / _data.Length;
        }

        public double StandardDeviation() => Math.Sqrt(Variance());

        public double Min()
        {
            if (_data.Length == 0)
                throw new InvalidOperationException("Массив данных пуст.");
            double min = _data[0];
            foreach (double x in _data) if (x < min) min = x;
            return min;
        }

        public double Max()
        {
            if (_data.Length == 0)
                throw new InvalidOperationException("Массив данных пуст.");
            double max = _data[0];
            foreach (double x in _data) if (x > max) max = x;
            return max;
        }

        public double[] GetSortedCopy()
        {
            double[] copy = (double[])_data.Clone();
            QuickSort(copy, 0, copy.Length - 1);
            return copy;
        }

        public void QuickSort(double[] arr, int left, int right)
        {
            if (left >= right) return;
            int pivotIndex = Partition(arr, left, right);
            QuickSort(arr, left, pivotIndex - 1);
            QuickSort(arr, pivotIndex + 1, right);
        }

        private int Partition(double[] arr, int left, int right)
        {
            double pivot = arr[right];
            int i = left - 1;
            for (int j = left; j < right; j++)
            {
                if (arr[j] <= pivot)
                {
                    i++;
                    (arr[i], arr[j]) = (arr[j], arr[i]);
                }
            }
            (arr[i + 1], arr[right]) = (arr[right], arr[i + 1]);
            return i + 1;
        }

        public void MergeSort(double[] arr, int left, int right)
        {
            if (left >= right) return;
            int mid = (left + right) / 2;
            MergeSort(arr, left, mid);
            MergeSort(arr, mid + 1, right);
            Merge(arr, left, mid, right);
        }

        private void Merge(double[] arr, int left, int mid, int right)
        {
            int n1 = mid - left + 1;
            int n2 = right - mid;
            double[] L = new double[n1];
            double[] R = new double[n2];

            for (int i = 0; i < n1; i++) L[i] = arr[left + i];
            for (int j = 0; j < n2; j++) R[j] = arr[mid + 1 + j];

            int i1 = 0, j1 = 0, k = left;
            while (i1 < n1 && j1 < n2)
            {
                if (L[i1] <= R[j1]) arr[k++] = L[i1++];
                else arr[k++] = R[j1++];
            }
            while (i1 < n1) arr[k++] = L[i1++];
            while (j1 < n2) arr[k++] = R[j1++];
        }

        public double[] GetSortedCopyMerge()
        {
            double[] copy = (double[])_data.Clone();
            MergeSort(copy, 0, copy.Length - 1);
            return copy;
        }

        public List<double> FindOutliers()
        {
            if (_data.Length == 0)
                throw new InvalidOperationException("Массив данных пуст.");

            double[] sorted = GetSortedCopy();
            double q1 = Percentile(sorted, 0.25);
            double q3 = Percentile(sorted, 0.75);
            double iqr = q3 - q1;

            double lowerBound = q1 - 1.5 * iqr;
            double upperBound = q3 + 1.5 * iqr;

            var outliers = new List<double>();
            foreach (double x in _data)
                if (x < lowerBound || x > upperBound)
                    outliers.Add(x);

            return outliers;
        }

        private double Percentile(double[] sorted, double p)
        {
            double index = p * (sorted.Length - 1);
            int lower = (int)Math.Floor(index);
            int upper = (int)Math.Ceiling(index);
            if (lower == upper) return sorted[lower];
            return sorted[lower] + (index - lower) * (sorted[upper] - sorted[lower]);
        }

        public Dictionary<double, int> FrequencyDistribution()
        {
            var freq = new Dictionary<double, int>();
            foreach (double x in _data)
            {
                if (freq.ContainsKey(x)) freq[x]++;
                else freq[x] = 1;
            }
            return freq;
        }

        public static double[] GenerateRandomData(int count, int minValue, int maxValue, int seed = 42)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Количество должно быть > 0.");
            if (minValue > maxValue)
                throw new ArgumentException("minValue не может быть больше maxValue.");

            var rng = new Random(seed);
            double[] data = new double[count];
            for (int i = 0; i < count; i++)
                data[i] = rng.Next(minValue, maxValue + 1);
            return data;
        }
    }
}