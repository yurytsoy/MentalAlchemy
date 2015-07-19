/*************************************************************************
    This file is part of the MentalAlchemy library.

    MentalAlchemy is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Foobar is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Foobar.  If not, see <http://www.gnu.org/licenses/>.
*************************************************************************/

using System;
using System.Collections.Generic;
using MathNet.Numerics;
using MathNet.Numerics.Distributions;

namespace MentalAlchemy.Atoms
{
	public class VectorMath
	{
		#region - Stats. -
		/// <summary>
		/// [atomic]
		/// 
		/// Calculates number of different elements in the given array.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="v">Input array.</param>
		/// <returns>Number of different elements.</returns>
		public static int CountDifferentElements<T> (T[] v)
		{
			var tempList = new List<T>();
			foreach (var elem in v)
			{
				if (!tempList.Contains(elem))
				{
					tempList.Add(elem);
				}
			}
			return tempList.Count;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates number of different entries in the given array.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="v"></param>
		/// <returns></returns>
		public static Dictionary<T, int> CountEntries<T> (T[] v)
		{
			var res = new Dictionary<T, int>();

			foreach (var elem in v)
			{
				if (!res.ContainsKey(elem)) {res.Add(elem, 1);}
				else { res[elem]++; }
			}

			return res;
		}

		#region - Stats for collection of bytes. -
		/// <summary>
		/// [atomic]
		/// 
		/// Calculate stats for the given collection.
		/// </summary>
		/// <param name="data">Collection of bytes.</param>
		/// <returns>Calculated [Stats] structure.</returns>
		public static Stats CalculateStats(byte[] data)
		{
			var stats = new Stats();
			stats.Min = Min(data);
			stats.Max = Max(data);
			stats.Total = Sum(data);
			stats.Mean = stats.Total/data.Length;
			stats.Variance = Variance(data);
			stats.Median = Median(data);

			return stats;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns minimal element in the given array of bytes.
		/// </summary>
		/// <param name="m">Input array.</param>
		/// <returns>Minimal element value.</returns>
		public static byte Min(byte[] m)
		{
			byte min = byte.MaxValue;
			foreach (var el in m) {if (min > el) min = el;}
			return min;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns maximal element in the given array of bytes.
		/// </summary>
		/// <param name="m">Input array.</param>
		/// <returns>Maximal element value.</returns>
		public static byte Max(byte[] m)
		{
			byte max = byte.MinValue;
			foreach (var el in m) { if (max < el) max = el; }
			return max;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns mean value in the given array.
		/// </summary>
		/// <param name="v">Input array.</param>
		/// <returns>Mean value.</returns>
		public static float Mean(byte[] v)
		{
			return Sum(v) / v.Length;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates sum of elements in the given array.
		/// </summary>
		/// <param name="mas">Input array.</param>
		/// <returns>Sum of elements.</returns>
		public static float Sum(byte[] mas)
		{
			var res = 0.0f;
			foreach (var d in mas) { res += d; }
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates unbiased variance for the given array.
		/// </summary>
		/// <param name="v">Input array.</param>
		/// <returns>Unbiased variance value.</returns>
		public static float Variance(byte[] v)
		{
			var meanV = Mean(v);

			var res = 0.0f;
			for (int i = 0; i < v.Length; i++)
			{
				var tempV = meanV - v[i];
				res += tempV * tempV;
			}

			return res / (v.Length - 1);
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates median value for the given array.
		/// </summary>
		/// <param name="v">Input array.</param>
		/// <returns>Median value.</returns>
		public static byte Median (byte[] v)
		{
			var hist = GetPartialHistogram(v, v.Length);
			var sum_2 = v.Length / 2;
			var tempSum = 0;
			for (byte i = 0; i < v.Length; i++)
			{
				tempSum += hist[i];
				if (tempSum >= sum_2) return i;
			}
			return 0;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates histogram for the first [count] entries from [data].
		/// </summary>
		/// <param name="data">Data array.</param>
		/// <param name="count">Number of entries to process.</param>
		/// <returns>Histogram for the first [count] entries.</returns>
		public static int[] GetPartialHistogram(byte[] data, int count)
		{
			var res = new int[Max(data) + 1];
			for (int i = 0; i < count; ++i)
			{
				++res[data[i]];
			}
			return res;
		}
		#endregion

		#region - Stats for collection of unsigned shorts. -
		/// <summary>
		/// [molecule]
		/// 
		/// Calculate stats for the given collection.
		/// </summary>
		/// <param name="data">Collection of bytes.</param>
		/// <returns>Calculated [Stats] structure.</returns>
		public static Stats CalculateStats(ushort[] data)
		{
			var stats = new Stats();
			stats.Min = Min(data);
			stats.Max = Max(data);
			stats.Total = Sum(data);
			stats.Mean = stats.Total / data.Length;
			stats.Variance = Variance(data);
			stats.Median = Median(data);

			return stats;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns minimal element in the given array.
		/// </summary>
		/// <param name="m">Input array.</param>
		/// <returns>Minimal element value.</returns>
		public static ushort Min(ushort[] m)
		{
			var min = ushort.MaxValue;
			foreach (var el in m) { if (min > el) min = el; }
			return min;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns maximal element in the given array.
		/// </summary>
		/// <param name="m">Input array.</param>
		/// <returns>Maximal element value.</returns>
		public static ushort Max(ushort[] m)
		{
			var max = ushort.MinValue;
			foreach (var el in m) { if (max < el) max = el; }
			return max;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns mean value in the given array.
		/// </summary>
		/// <param name="v">Input array.</param>
		/// <returns>Mean value.</returns>
		public static float Mean(ushort[] v)
		{
			return Sum(v) / v.Length;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates sum of elements in the given array.
		/// </summary>
		/// <param name="mas">Input array.</param>
		/// <returns>Sum of elements.</returns>
		public static float Sum(ushort[] mas)
		{
			var res = 0.0f;
			foreach (var d in mas) { res += d; }
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates unbiased variance for the given array.
		/// </summary>
		/// <param name="v">Input array.</param>
		/// <returns>Unbiased variance value.</returns>
		public static float Variance(ushort[] v)
		{
			var meanV = Mean(v);

			var res = 0.0f;
			for (int i = 0; i < v.Length; i++)
			{
				var tempV = meanV - v[i];
				res += tempV * tempV;
			}

			return res / (v.Length - 1);
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates median value for the given array.
		/// </summary>
		/// <param name="v">Input array.</param>
		/// <returns>Median value.</returns>
		public static ushort Median(ushort[] v)
		{
			var hist = GetPartialHistogram(v, v.Length);
			var sum_2 = v.Length / 2;
			var tempSum = 0;
			for (ushort i = 0; i < v.Length; i++)
			{
				tempSum += hist[i];
				if (tempSum >= sum_2) return i;
			}
			return 0;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates histogram for the first [count] entries from [data].
		/// </summary>
		/// <param name="data">Data array.</param>
		/// <param name="count">Number of entries to process.</param>
		/// <returns>Histogram for the first [count] entries.</returns>
		public static int[] GetPartialHistogram(ushort[] data, int count)
		{
			var res = new int[Max(data) + 1];
			for (int i = 0; i < count; ++i)
			{
				++res[data[i]];
			}
			return res;
		}
		#endregion

		#region - Stats for collection of floats. -
		/// <summary>
		/// [atomic]
		/// 
		/// Calculate stats for the given collection.
		/// Note: the median value is undefined for the collection of floats.
		/// </summary>
		/// <param name="data">Collection of floats.</param>
		/// <returns>Calculated [Stats] structure.</returns>
		public static Stats CalculateStats(float[] data)
		{
			var stats = new Stats();
			stats.Min = Min(data);
			stats.Max = Max(data);
			stats.Total = Sum(data);
			stats.Mean = stats.Total / data.Length;
			stats.Variance = Variance(data);
			stats.Median = data.Length > 0 ? Median(data) : 0;

			return stats;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns minimal element in the given array of floats.
		/// </summary>
		/// <param name="m">Input array.</param>
		/// <returns>Minimal element value.</returns>
		public static float Min(float[] m)
		{
			float min = float.MaxValue;
			foreach (var el in m)
			{
				if (min > el) min = el;
			}
			return min;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns minimal element in the given list of arrays of floats.
		/// </summary>
		/// <param name="m">Input list of arrays.</param>
		/// <returns>Minimal element value.</returns>
		public static float Min(List<float[]> m)
		{
			float min = float.MaxValue;
			foreach (var v in m)
			{
				float tempM = Min(v);
				if (min > tempM) min = tempM;
			}
			return min;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns maximal element in the given array of floats.
		/// </summary>
		/// <param name="m">Input array.</param>
		/// <returns>Maximal element value.</returns>
		public static float Max(float[] m)
		{
			float max = m[0];
			for (int i = 1; i < m.Length; ++i)
			{
				if (max < m[i]) max = m[i];
			}
			return max;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns maximal element in the given list of arrays of floats.
		/// </summary>
		/// <param name="m">Input list of arrays.</param>
		/// <returns>Maximal element value.</returns>
		public static float Max(List<float[]> m)
		{
			float max = float.MinValue;
			foreach (var v in m)
			{
				float tempM = Max(v);
				if (max < tempM) max = tempM;
			}
			return max;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns mean value in the given array.
		/// </summary>
		/// <param name="v">Input array.</param>
		/// <returns>Mean value.</returns>
		public static float Mean(float[] v)
		{
			return Sum(v) / v.Length;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates median value for the given array.
		/// </summary>
		/// <param name="v">Input array.</param>
		/// <returns>Median value.</returns>
		public static float Median(float[] v)
		{
			var sv = new List<float>(v);
			Sorting.Sort(sv);
			if (v.Length % 2 == 0)
			{
				var idx = sv.Count / 2;
				return 0.5f * (sv [idx] + sv[idx - 1]);
			}
			return sv[sv.Count / 2];
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns vector of means for each array in the given list.
		/// </summary>
		/// <param name="v">Input list of arrays.</param>
		/// <returns>Array of means for each array.</returns>
		public static float[] MeanList(List<float[]> v)
		{
			var size = v.Count;
			var res = new float[size];
			for (int i = 0; i < size; i++)
			{
				res[i] = Mean(v[i]);
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates unbiased variance for the given array.
		/// </summary>
		/// <param name="v">Input array.</param>
		/// <returns>Unbiased variance value.</returns>
		public static float Variance(float[] v)
		{
			var meanV = Mean(v);

			var res = 0.0f;
			for (var i = 0; i < v.Length; i++)
			{
				var tempV = meanV - v[i];
				res += tempV * tempV;
			}

			return res / (v.Length - 1);
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates biased variance for the given array.
		/// </summary>
		/// <param name="v">Input array.</param>
		/// <returns>Unbiased variance value.</returns>
		public static float VarianceBiased(float[] v)
		{
			var meanV = Mean(v);

			var res = 0.0f;
			for (var i = 0; i < v.Length; i++)
			{
				var tempV = meanV - v[i];
				res += tempV * tempV;
			}

			return res / v.Length;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates mean vector for the given list of 1D arrays.
		/// </summary>
		/// <param name="v">Input list of arrays.</param>
		/// <returns>Mean vector.</returns>
		public static float[] MeanVector(List<float[]> v)
		{
			var res = (float[])v[0].Clone();
			for (int i = 1; i < v.Count; i++)
			{
				AccumulateInplace(res, v[i]);
			}
			Divide(ref res, v.Count);
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculate entropy for the given array.
		/// </summary>
		/// <param name="v">Input array.</param>
		/// <returns>Entropy value.</returns>
		public static float Entropy(float[] v)
		{
			// calculate real neighborhood size.
			var size = Sum(v);
			float size_1 = 1.0f / size;

			//
			// calculate brightness entropy in a neighborhood.
			float entropy = 0.0f;
			foreach (var entry in v)
			{
				float p = entry * size_1;
				entropy -= (float)(p * Math.Log(p, 2));
			}
			return entropy;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates variance for the array of N-dimensional vectors.
		/// </summary>
		/// <param name="v">Array of input vectors.</param>
		/// <returns>Variance value.</returns>
		public static float Variance(List<float[]> v)
		{
			var meanV = MeanVector(v);

			var res = 0.0f;
			for (int i = 0; i < v.Count; i++)
			{
				var tempV = Sub(v[i], meanV);
				res += DotProduct(tempV, tempV);
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates array of variances for each array in the given list.
		/// </summary>
		/// <param name="v">Array of input vectors.</param>
		/// <returns>Array of variances.</returns>
		public static float[] VarianceList(List<float[]> v)
		{
			var size = v.Count;
			var res = new float[size];
			for (var i = 0; i < size; i++)
			{
				res[i] = Variance(v[i]);
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates variances for each variable in the given lists.
		/// Note: It's assumed that each vector has the same size
		/// Note:   or at least that the vectors are aligned by variables.
		/// Note:   Otherwise the result may be unpredictable.
		/// </summary>
		/// <param name="v">Array of input vectors.</param>
		/// <returns>Array of variances.</returns>
		public static float[] VarianceVector(List<float[]> v)
		{
			// convert into list of columns
			var tempM = MatrixMath.CreateFromRowsList(v);
			var tempCols = MatrixMath.ConvertToColumnsList(tempM);
			return VarianceList(tempCols);
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Computes unbiased standard deviation for a given array.
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		public static float StdDev(float[] v)
		{
			var tmp = Variance(v);
			return (float)Math.Sqrt(tmp);
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates array of standard deviations for each array in the given list.
		/// </summary>
		/// <param name="v">Array of input vectors.</param>
		/// <returns>Array of StdDev values.</returns>
		public static float[] StdDevList(List<float[]> v)
		{
			var res = VarianceList(v);
			return Sqrt(res);
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates correlation between two vectors given their means and standard deviations.
		/// </summary>
		/// <param name="v1">1st vector.</param>
		/// <param name="mean1">Mean of the 2st vector.</param>
		/// <param name="dev1">StdDev of the 1st vector.</param>
		/// <param name="v2">2nd vector.</param>
		/// <param name="mean2">Mean of the 2nd vector.</param>
		/// <param name="dev2">StdDev of the 2nd vector.</param>
		/// <returns>Correlation coefficient.</returns>
		public static float Correlation (float[] v1, float mean1, float dev1, float[] v2, float mean2, float dev2)
		{
			var size = v1.Length;

			var res = 0f;
			for (var i = 0; i < size; i++)
			{
				res += (v1[i] - mean1)*(v2[i] - mean2);
			}
			if (dev1 != 0 && dev2 != 0)
			{
				res /= (size * dev1 * dev2);
			}
			else if (dev2!= 0)
			{
				res /= (size * dev2);
			}
			else if (dev1 != 0)
			{
				res /= (size * dev1);
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates unbiased covariance between two vectors given their means.
		/// </summary>
		/// <param name="v1">1st vector.</param>
		/// <param name="mean1">Mean of the 2st vector.</param>
		/// <param name="v2">2nd vector.</param>
		/// <param name="mean2">Mean of the 2nd vector.</param>
		/// <returns>Covariance.</returns>
		public static float Covariance(float[] v1, float mean1, float[] v2, float mean2)
		{
			var size = v1.Length;

			var res = 0f;
			for (var i = 0; i < size; i++)
			{
				res += (v1[i] - mean1) * (v2[i] - mean2);
			}
			res /= (size - 1);

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates biased covariance between two vectors given their means.
		/// </summary>
		/// <param name="v1">1st vector.</param>
		/// <param name="mean1">Mean of the 2st vector.</param>
		/// <param name="v2">2nd vector.</param>
		/// <param name="mean2">Mean of the 2nd vector.</param>
		/// <returns>Covariance.</returns>
		public static float CovarianceBiased(float[] v1, float mean1, float[] v2, float mean2)
		{
			var size = v1.Length;

			var res = 0f;
			for (var i = 0; i < size; i++)
			{
				res += (v1[i] - mean1) * (v2[i] - mean2);
			}
			res /= size;

			return res;
		}
	
		/// <summary>
		/// [atomic]
		/// 
		/// Calculates number of elements in the array equal to the given [val].
		/// </summary>
		/// <param name="v">Input array.</param>
		/// <param name="val">Value to calculate.</param>
		/// <returns>Number of elements equal to [val].</returns>
		public static int Calculate(float[] v, float val)
		{
			var count = 0;
			foreach (var el in v)
			{
				if (el == val) ++count;
			}
			return count;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns index of the first array element, which is less or equal to the given number.
		/// If there is no such an element then returns -1.
		/// </summary>
		/// <param name="v">Input array.</param>
		/// <param name="val">Value to calculate.</param>
		/// <returns>Number of elements equal to [val].</returns>
		public static int FirstLessOrEqualIndex(float[] v, float val)
		{
			for (int i = 0; i < v.Length; i++)
			{
				var el = v[i];
				if (el <= val) return i;
			}
			return -1;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns index of the first array element, which is greater or equal to the given number.
		/// If there is no such an element then returns -1.
		/// </summary>
		/// <param name="v">Input array.</param>
		/// <param name="val">Value to calculate.</param>
		/// <returns>Number of elements equal to [val].</returns>
		public static int FirstGreaterOrEqualIndex(float[] v, float val)
		{
			for (int i = 0; i < v.Length; i++)
			{
				var el = v[i];
				if (el >= val) return i;
			}
			return -1;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns index of the first array element, which is greater than a given number.
		/// If there is no such an element then returns -1.
		/// </summary>
		/// <param name="v">Input array.</param>
		/// <param name="val">Value to calculate.</param>
		/// <returns>Number of elements greater than [val].</returns>
		public static int FirstGreaterIndex(IList<int> v, int val)
		{
			for (int i = 0; i < v.Count; i++)
			{
				var el = v[i];
				if (el > val) return i;
			}
			return -1;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns index of the first array element, which is greater than a given number.
		/// If there is no such an element then returns -1.
		/// </summary>
		/// <param name="v">Input array.</param>
		/// <param name="val">Value to calculate.</param>
		/// <returns>Number of elements greater than [val].</returns>
		public static int FirstGreaterIndex(IList<float> v, float val)
		{
			for (int i = 0; i < v.Count; i++)
			{
				var el = v[i];
				if (el > val) return i;
			}
			return -1;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Defines whether vector contains only zeroes or not.
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		public static bool IsZero (float[] v)
		{
			for (int i = 0; i < v.Length; i++)
			{
				if (v[i] != 0) return false;
			}
			return true;
		}
		#endregion

		#region - Stats for collection of ints. -
		/// <summary>
		/// [atomic]
		/// 
		/// Calculate stats for the given collection.
		/// </summary>
		/// <param name="data">Collection of ints.</param>
		/// <returns>Calculated [Stats] structure.</returns>
		public static Stats CalculateStats(int[] data)
		{
			var stats = new Stats();
			stats.Min = Min(data);
			stats.Max = Max(data);
			stats.Total = Sum(data);
			stats.Mean = data.Length > 0 ? stats.Total / data.Length : 0;
			stats.Variance = Variance(data);
			stats.Median = data.Length > 0 ? Median(data) : 0;

			return stats;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns minimal element in the given array.
		/// </summary>
		/// <param name="m">Input array.</param>
		/// <returns>Minimal element value.</returns>
		public static int Min(int[] m)
		{
			var min = int.MaxValue;
			foreach (var el in m) { if (min > el) min = el; }
			return min;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns maximal element in the given array of ints.
		/// </summary>
		/// <param name="m">Input array.</param>
		/// <returns>Maximal element value.</returns>
		public static int Max(int[] m)
		{
			int max = int.MinValue;
			foreach (var el in m) { if (max < el) max = el; }
			return max;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates unbiased variance for the given array.
		/// </summary>
		/// <param name="v">Input array.</param>
		/// <returns>Unbiased variance value.</returns>
		public static float Variance(int[] v)
		{
			var meanV = Mean(v);

			var res = 0.0f;
			for (int i = 0; i < v.Length; i++)
			{
				var tempV = meanV - v[i];
				res += tempV * tempV;
			}

			return res / (v.Length - 1);
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns mean value in the given array.
		/// </summary>
		/// <param name="v">Input array.</param>
		/// <returns>Mean value.</returns>
		public static float Mean(int[] v)
		{
			return Sum(v) / (float)v.Length;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculate entropy for the given array.
		/// </summary>
		/// <param name="v">Input array.</param>
		/// <returns>Entropy value.</returns>
		public static float Entropy(ICollection<int> v)
		{
			// calculate real neighborhood size.
			var size = Sum(v);
			float size_1 = 1.0f / size;

			//
			// calculate brightness entropy in a neighborhood.
			float entropy = 0.0f;
			foreach (var entry in v)
			{
				float p = entry * size_1;
				entropy -= (float)(p * Math.Log(p, 2));
			}
			return entropy;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates histogram for the first [count] entries from [data].
		/// </summary>
		/// <param name="data">Data array.</param>
		/// <param name="count">Number of entries to process.</param>
		/// <returns>Histogram for the first [count] entries.</returns>
		public static int[] GetPartialHistogram(int[] data, int count)
		{
			var res = new int[Max(data) + 1];
			for (int i = 0; i < count; ++i)
			{
				res[data[i]]++;
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates median value for the given array.
		/// </summary>
		/// <param name="v">Input array.</param>
		/// <returns>Median value.</returns>
		public static float Median(int[] v)
		{
			var sv = new List<int>(v);
			Sorting.Sort(sv);
			if (v.Length % 2 == 0)
			{
				var idx = sv.Count / 2;
				return 0.5f * (sv [idx] + sv[idx - 1]);
			}
			return sv[sv.Count / 2];
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates histogram for the first [count] entries from [data].
		/// </summary>
		/// <param name="data">Data array.</param>
		/// <param name="max">Max value in [data].</param>
		/// <param name="count">Number of entries to process.</param>
		/// <returns>Histogram for the first [count] entries.</returns>
		public static Dictionary<int, int> GetPartialHistogram(int[] data, int max, int count)
		{
			var res = new Dictionary<int, int>();
			for (var i = 0; i < count; ++i)
			{
				if (!res.ContainsKey(data[i])) res.Add(data[i], 0);
				res[data[i]]++;
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates number of elements in the array equal to the given [val].
		/// </summary>
		/// <param name="v">Input array.</param>
		/// <param name="val">Value to calculate.</param>
		/// <returns>Number of elements equal to [val].</returns>
		public static int Calculate (int[] v, int val)
		{
			var count = 0;
			foreach (var el in v)
			{
				if (el == val) ++count;
			}
			return count;
		}
		#endregion
		#endregion

		#region - Search. -
		/// <summary>
		/// [atomic]
		/// 
		/// Returns index of the first minimal element in the given array.
		/// </summary>
		/// <param name="data">Array.</param>
		/// <returns>Index of the first minimal element.</returns>
		public static int IndexOfMin(float[] data)
		{
			float min = data[0];
			int minIdx = 0;
			for (int i = 1; i < data.Length; ++i)
			{
				if (data[i] < min)
				{
					min = data[i];
					minIdx = i;
				}
			}
			return minIdx;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns index of the maximal element in the given array of ints.
		/// </summary>
		/// <param name="m">Input array.</param>
		/// <returns>Maximal element index value.</returns>
		public static int IndexOfMax(int[] m)
		{
			int idx = 0, maxCount = m[0];
			for (int i = 1; i < m.Length; ++i)
			{
				if (maxCount < m[i])
				{
					idx = i;
					maxCount = m[i];
				}
			}
			return idx;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns index of the maximal element in the given array of ints.
		/// </summary>
		/// <param name="m">Input array.</param>
		/// <returns>Maximal element index value.</returns>
		public static int IndexOfMax(float[] m)
		{
			if (m.Length == 0) return -1;

			int idx = 0;
			var max = m[0];
			for (int i = 1; i < m.Length; ++i)
			{
				if (max < m[i])
				{
					idx = i;
					max = m[i];
				}
			}
			return idx;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns index of the maximal element in the given array of ints.
		/// </summary>
		/// <param name="m">Input array.</param>
		/// <returns>Maximal element index value.</returns>
		public static int IndexOfMax(float[] m, int start)
		{
			if (m.Length == 0 || start >= m.Length) return -1;

			int idx = start;
			var max = m[start];
			for (int i = start + 1; i < m.Length; ++i)
			{
				if (max < m[i])
				{
					idx = i;
					max = m[i];
				}
			}
			return idx;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns index of the maximal element in the given array.
		/// </summary>
		/// <param name="m">Input array.</param>
		/// <returns>Index of the maximal element.</returns>
		public static int IndexOfMax(double[] m)
		{
			if (m.Length == 0) return -1;

			var idx = 0;
			var maxCount = m[0];
			for (var i = 1; i < m.Length; ++i)
			{
				if (maxCount < m[i])
				{
					idx = i;
					maxCount = m[i];
				}
			}
			return idx;
		}

		///// <summary>
		///// [atomic]
		///// 
		///// Returns index of the maximal element in the given array.
		///// </summary>
		///// <param name="m">Input array.</param>
		///// <returns>Index of the maximal element.</returns>
		//public static int IndexOfMax(double[] m)
		//{
		//	var idx = 0;
		//	var maxCount = m[0];
		//	for (var i = 1; i < m.Length; ++i)
		//	{
		//		if (maxCount < m[i])
		//		{
		//			idx = i;
		//			maxCount = m[i];
		//		}
		//	}
		//	return idx;
		//}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns index of the first element equal to the [val].
		/// </summary>
		/// <param name="data">Input array.</param>
		/// <param name="val">Value to search.</param>
		/// <returns>Index of the first entry or -1 if no entries are found.</returns>
		public static int FirstIndexOf (int[] data, int val)
		{
			for (int i = 0; i < data.Length; i++)
			{
				if (data[i] == val) return i;
			}
			return -1;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns index of the first element equal to the [val].
		/// </summary>
		/// <param name="data">Input array.</param>
		/// <param name="val">Value to search.</param>
		/// <returns>Index of the first entry or -1 if no entries are found.</returns>
		public static int FirstIndexOf(float[] data, float val)
		{
			return FirstIndexOf(data, val, 0);
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns index of the first element equal to the [val] starting from the given position.
		/// </summary>
		/// <param name="data">Input array.</param>
		/// <param name="val">Value to search.</param>
		/// <param name="pos">Starting position.</param>
		/// <returns>Index of the first entry or -1 if no entries are found.</returns>
		public static int FirstIndexOf(float[] data, float val, int pos)
		{
			for (var i = pos; i < data.Length; i++)
			{
				if (data[i] == val) return i;
			}
			return -1;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Samples random element from the given array.
		/// </summary>
		/// <param name="v">Input array.</param>
		/// <returns>Random value from the array.</returns>
		public static T PickRandom<T> (T[] v)
		{
			var idx = ContextRandom.Next(v.Length);
			return v[idx];
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Samples random element from the given array.
		/// </summary>
		/// <param name="v">Input array.</param>
		/// <returns>Random value from the array.</returns>
		public static T PickRandom<T>(List<T> v)
		{
			var idx = ContextRandom.Next(v.Count);
			return v[idx];
		}
		#endregion

		#region - Distance measure methods. -
		/// <summary>
		/// [atomic]
		/// 
		/// Calculates Euclidian distance between two vectors.
		/// If vectors are of different length then float.NaN is returned.
		/// </summary>
		/// <param name="v1">1-st vector.</param>
		/// <param name="v2">2-nd vector.</param>
		/// <returns>Distance value.</returns>
		public static float EuclidianDistance(float[] v1, float[] v2)
		{
			return (float)Math.Sqrt(EuclidianDistanceSqr(v1, v2));
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates squared Euclidian distance between two vectors.
		/// If vectors are of different length then float.NaN is returned.
		/// </summary>
		/// <param name="v1">1-st vector.</param>
		/// <param name="v2">2-nd vector.</param>
		/// <returns>Squared distance value.</returns>
		public static float EuclidianDistanceSqr(float[] v1, float[] v2)
		{
			if (v1.Length != v2.Length)
			{
				return float.NaN;
			}

			float result = 0.0f;
			for (int i = 0; i < v1.Length; ++i)
			{
				float temp = v1[i] - v2[i];
				result += temp * temp;
			}
			return result;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates max absolute difference between two vectors.
		/// If vectors are of different length then float.NaN is returned.
		/// </summary>
		/// <param name="v1">1-st vector.</param>
		/// <param name="v2">2-nd vector.</param>
		/// <returns>Squared distance value.</returns>
		public static float MaxAbsDistance(float[] v1, float[] v2)
		{
			if (v1.Length != v2.Length) return float.NaN;

			var res = 0f;
			for (int i=0; i<v1.Length; ++i)
			{
				var temp = Math.Abs(v1[i] - v2[i]);
				if (temp > res)
				{
					res = temp;
				}
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates L2 norm for the given vector.
		/// </summary>
		/// <param name="v">Input vector.</param>
		/// <returns>Norm value.</returns>
		public static float L2Norm (float[] v)
		{
			var res = DotProduct(v, v);
			return (float)Math.Sqrt(res);
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates L2 norm for the given vector.
		/// </summary>
		/// <param name="v">Input vector.</param>
		/// <returns>Norm value.</returns>
		public static double L2Norm(double[] v)
		{
			var res = DotProduct(v, v);
			return Math.Sqrt(res);
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Calculates minimal distance between given vector and the vectors set.
		/// </summary>
		/// <param name="v">Vector of interest.</param>
		/// <param name="data">Set of vectors.</param>
		/// <param name="dist">Distance measure.</param>
		/// <returns>Minimal distance value.</returns>
		public static float GetMinDistance (float[] v, List<float[]> data, DistanceMeasure1D dist)
		{
			var mindist = float.MaxValue;
			foreach (var entry in data)
			{
				var temp = dist(v, entry);
				if (temp < mindist)
				{
					mindist = temp;
				}
			}

			return mindist;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Calculates minimal distance between given vector and the vectors set and also find the index of the closest vector from the vector set.
		/// </summary>
		/// <param name="v">Reference vector.</param>
		/// <param name="data">Set of vectors.</param>
		/// <param name="idx">Index of the closest vector from the set.</param>
		/// <param name="dist">Distance measure.</param>
		/// <returns>Minimal distance value.</returns>
		public static float GetMinDistance(float[] v, List<float[]> data, out int idx, DistanceMeasure1D dist)
		{
			var mindist = float.MaxValue;
			idx = 0;
			for (int i = 0; i < data.Count; i++)
			{
				var entry = data[i];
				var temp = dist(v, entry);
				if (temp < mindist)
				{
					mindist = temp;
					idx = i;
				}
			}

			return mindist;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Calculates index of the vector which is the closest one to the given vector.
		/// </summary>
		/// <param name="v">Reference vector.</param>
		/// <param name="data">Set of vectors.</param>
		/// <param name="dist">Distance measure.</param>
		/// <returns>Minimal distance value.</returns>
		public static int GetClosestVectorIndex (float[] v, List<float[]> data, DistanceMeasure1D dist)
		{
			var mindist = float.MaxValue;
			var idx = 0;
			for (int i = 0; i < data.Count; i++)
			{
				var entry = data[i];
				var temp = dist(v, entry);
				if (temp < mindist)
				{
					mindist = temp;
					idx = i;
				}
			}

			return idx;
		}
		#endregion

		#region - Vector generation methods. -
		/// <summary>
		/// [atomic]
		/// 
		/// Generates random vector of the specified size.
		/// </summary>
		/// <param name="size"></param>
		/// <returns></returns>
		public static float[] CreateRandomVector (int size)
		{
			var res = new float[size];

			for (int i=0; i<size; ++i)
			{
				res[i] = (float)(ContextRandom.NextDouble());
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Generates random vector using given RNG, vector size and values constraints.
		/// </summary>
		/// <param name="rnd"></param>
		/// <param name="size"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public static float[] CreateRandomVector(Random rnd, int size, float min, float max)
		{
			var res = new float[size];

			double diff = max - min;
			for (int i = 0; i < size; ++i)
			{
				res[i] = (float)(rnd.NextDouble() * diff + min);
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Generates random vector using RNG from Math.NET and a vector's size.
		/// </summary>
		/// <param name="rnd"></param>
		/// <param name="size"></param>
		/// <returns></returns>
		public static float[] CreateRandomVector(ContinuousDistribution rnd, int size)
		{
			var res = new float[size];

			for (int i = 0; i < size; ++i)
			{
				res[i] = (float)(rnd.NextDouble());
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Creates zero-filled vector of floats of specified [size].
		/// </summary>
		/// <param name="length">Length of the created vector.</param>
		/// <returns>Created vector.</returns>
		public static float[] Zeros (int length)
		{
			var res = new float[length];
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Creates vector of floats of specified [size] filled with 1's.
		/// </summary>
		/// <param name="length">Length of the created vector.</param>
		/// <returns>Created vector.</returns>
		public static float[] Ones(int length)
		{
			var res = new float[length];
			for (int i = 0; i < length; ++i ) res[i] = 1;
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Creates a vector of floats parsed from the given array of strings.
		/// </summary>
		/// <param name="str">Array of strings to parse.</param>
		/// <returns>Created vector.</returns>
		public static float[] CreateFromStringsArray (string[] str)
		{
			int len = str.Length;
			var res = new float[len];
			for (int i=0; i<len; ++i)
			{
				res[i] = float.Parse(str[i]);
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Creates array of floats from the given [Stats] object.
		/// </summary>
		/// <param name="stats"></param>
		/// <returns></returns>
		public static float[] CreateFromStats (Stats stats)
		{
			var str = stats.GetStatsString();
			var strs = str.Split('\t');
			return CreateFromStringsArray(strs);
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Creates array of floats from the given array of doubles.
		/// </summary>
		/// <param name="v">Input vector.</param>
		/// <returns>Resulting vector.</returns>
		public static float[] CreateFromDoubles (double[] v)
		{
			var size = v.Length;
			var res = new float[size];
			for (var i = 0; i < size; i++)
			{
				res[i] = (float)v[i];
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Generates sequences of numbers from [start] to [end] inclusively.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public static float[] Sequence(float start, float end, int count)
		{
			var step = (end - start) / (count-1);
			var res = new float[count];
			for (int i=0; i<count; ++i)
			{
				res[i] = step * i + start;
			}
			return res;
		}

		/// <summary>
		/// Creates an array of given size filled with specified value.
		/// </summary>
		/// <param name="size"></param>
		/// <param name="val"></param>
		/// <returns></returns>
		public static float[] Create(int size, float val)
		{ 
			var res = new float[size];
			for (int i = 0; i < size; ++i )
			{
				res[i] = val;
			}
			return res;
		}
		#endregion

		#region - Arithmetics. -
		/// <summary>
		/// [atomic]
		/// 
		/// Element-wise addition of two vectors.
		/// </summary>
		/// <param name="v1">1st vector.</param>
		/// <param name="v2">2nd vector.</param>
		/// <returns>Resulting vector (v1 - v2).</returns>
		public static float[] Add (float[] v1, float[] v2)
		{
			int size = v1.Length;
			var res = new float[size];

			for (int i = 0; i < size; i++)
			{
				res[i] = v1[i] + v2[i];
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Element-wise addition of two vectors.
		/// Deals with possible situation when vectors are of unequal lengths.
		/// </summary>
		/// <param name="v1">1st vector.</param>
		/// <param name="v2">2nd vector.</param>
		/// <returns>Resulting vector (v1 - v2).</returns>
		public static float[] AddSafe(float[] v1, float[] v2)
		{
			var res = v1.Length > v2.Length ? (float[])v1.Clone() : (float[])v2.Clone();

			var tempSize = Math.Min(v1.Length, v2.Length);
			for (int i = 0; i < tempSize; i++)
			{
				res[i] = v1[i] + v2[i];
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Element-wise substraction of two vectors.
		/// </summary>
		/// <param name="v1">1st vector.</param>
		/// <param name="v2">2nd vector.</param>
		/// <returns>Resulting vector (v1 - v2).</returns>
		public static float[] Sub(float[] v1, float[] v2)
		{
			int size = v1.Length;
			var res = new float[size];

			for (int i = 0; i < size; i++)
			{
				res[i] = v1[i] - v2[i];
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Element-wise substraction of two vectors.
		/// The result is written into the first argument vector.
		/// </summary>
		/// <param name="v1">1st vector.</param>
		/// <param name="v2">2nd vector.</param>
		public static void SubInplace(float[] v1, float[] v2)
		{
			int size = v1.Length;

			for (int i = 0; i < size; i++)
			{
				v1[i] -= v2[i];
			}
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Sums given arrays. The result is written into input array.
		/// </summary>
		/// <param name="v">Input array.</param>
		/// <param name="arg">Summand array.</param>
		public static void AccumulateInplace(float[] v, float[] arg)
		{
			if (v.Length != arg.Length) return;

			for (int i = 0; i < v.Length; ++i)
			{
				v[i] += arg[i];
			}
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Sums given arrays. The result is written into input array.
		/// </summary>
		/// <param name="v">Input array.</param>
		/// <param name="arg">Summand array.</param>
		public static void Accumulate(ref int[] v, int[] arg)
		{
			if (v.Length != arg.Length) return;

			for (int i = 0; i < v.Length; ++i)
			{
				v[i] += arg[i];
			}
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Divides given array with the given number. The result is written into input array.
		/// </summary>
		/// <param name="v">Input array.</param>
		/// <param name="arg">The number to divide with.</param>
		public static void Divide(ref float[] v, float arg)
		{
			if (arg == 0.0) return;
			Mul(ref v, 1.0f / arg);
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Divides given array of complex numbers by the given number.
		/// </summary>
		/// <param name="v">Input array.</param>
		/// <param name="arg">The number to divide with.</param>
		/// <returns>Division result.</returns>
		public static Complex[] Divide(Complex[] v, float arg)
		{
			if (arg == 0.0) return v;
			return Mul(v, 1.0f / arg);
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Multiplies given array with the given number. The result is written into input array.
		/// </summary>
		/// <param name="v">Input array.</param>
		/// <param name="arg">The number to multiply with.</param>
		public static void Mul(ref float[] v, float arg)
		{
			for (int i = 0; i < v.Length; ++i)
			{
				v[i] *= arg;
			}
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Multiplies given array with the given number.
		/// </summary>
		/// <param name="v">Input array.</param>
		/// <param name="arg">The number to multiply with.</param>
		/// <returns>Multiplication result.</returns>
		public static float[] Mul(float[] v, float arg)
		{
			var size = v.Length;
			var res = new float[size];
			for (int i = 0; i < size; ++i)
			{
				res[i] = v[i] * arg;
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// Multiplies vector elements by a given number and updates input vector.
		/// </summary>
		public static void MulInplace(float[] v, float num)
		{
			for (int i = 0; i < v.Length; ++i)
			{
				v[i] *= num;
			}
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Multiplies given complex array with the given number.
		/// </summary>
		/// <param name="v">Input array.</param>
		/// <param name="arg">The number to multiply with.</param>
		/// <returns>Multiplication result.</returns>
		public static Complex[] Mul(Complex[] v, float arg)
		{
			var res = (Complex[])v.Clone();
			for (int i = 0; i < v.Length; ++i)
			{
				res[i].Real *= arg;
				res[i].Imag *= arg;
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns dot product of the given vectors.
		/// </summary>
		/// <param name="v1">1st vector.</param>
		/// <param name="v2">2nd vector.</param>
		/// <returns>Resulting vector = v1' * v2, where v1' -- is a transposed v1.</returns>
		public static float DotProduct (float[] v1, float[] v2)
		{
			var res = 0.0f;
			int size = v1.Length;
			for (int i = 0; i < size; i++)
			{
				res += v1[i]*v2[i];
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns dot product of the given vectors.
		/// </summary>
		/// <param name="v1">1st vector.</param>
		/// <param name="v2">2nd vector.</param>
		/// <returns>Resulting vector = v1' * v2, where v1' -- is a transposed v1.</returns>
		public static float DotProduct(IList<float> v1, IList<float> v2)
		{
			var res = 0.0f;
			var size = v1.Count;
			for (int i = 0; i < size; i++)
			{
				res += v1[i]*v2[i];
			}
			//int i = 0;
			//foreach (var v in v1)
			//{
			//    res += v*v2[i];
			//    ++i;
			//}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns pairwise dot products of the given arrays of vectors.
		/// </summary>
		/// <param name="v1">1st vector array.</param>
		/// <param name="v2">2nd vector array.</param>
		/// <returns>Resulting table of dot products.</returns>
		public static float[,] DotProduct(List<float[]> v1, List<float[]> v2)
		{
			var size1 = v1.Count;
			var size2 = v2.Count;
			var res = new float[size1, size2];

			for (int i = 0; i < size1; i++)
			{
				for (int j = 0; j < size2; j++)
				{
					res[i, j] = DotProduct(v1[i], v2[j]);
				}
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns dot product of the given vectors.
		/// </summary>
		/// <param name="v1">1st vector.</param>
		/// <param name="v2">2nd vector.</param>
		/// <returns>Resulting vector = v1' * v2, where v1' -- is a transposed v1.</returns>
		public static double DotProduct(double[] v1, double[] v2)
		{
			var res = 0.0;
			int size = v1.Length;
			for (int i = 0; i < size; i++)
			{
				res += v1[i] * v2[i];
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates dyadic product of vector v.
		/// </summary>
		/// <param name="v">Input vector.</param>
		/// <returns>Resulting matrix = v * v', where v' -- is a transposed v.</returns>
		public static float[,] OuterProduct(IList<float> v)
		{
			int size = v.Count;
			var res = new float[size, size];

			for (int i = 0; i < size; ++i)
			{
				var vi = v[i];
				res[i, i] = vi * vi;
				for (int j = i + 1; j < size; ++j)
				{
					res[i, j] = res[j, i] = vi * v[j];
				}
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates dyadic product of vector v.
		/// </summary>
		/// <param name="v">Input vector.</param>
		/// <returns>Resulting matrix = v * v', where v' -- is a transposed v.</returns>
		public static double[,] OuterProduct(double[] v)
		{
			int size = v.Length;
			var res = new double[size, size];

			for (int i = 0; i < size; ++i)
			{
				res[i, i] = v[i] * v[i];
				for (int j = i + 1; j < size; ++j)
				{
					res[i, j] = res[j, i] = v[i] * v[j];
				}
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates outer (dyadic) product of the given vectors.
		/// </summary>
		/// <param name="v">1st input vector.</param>
		/// <param name="w">2nd input vector.</param>
		/// <returns>Resulting matrix = v * w', where w' -- is a transposed w.</returns>
		public static float[][] OuterProduct(float[] v, float[] w)
		{
			int height = v.Length;
			int width = w.Length;
			var res = MatrixMath.Zeros(height, width);
			for (int i = 0; i < height; ++i)
			{
				var resi = res[i];
				for (int j = 0; j < width; ++j)
				{
					resi[j] = v[i] * w[j];
				}
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates outer (dyadic) product of the given vectors.
		/// </summary>
		/// <param name="v">1st input vector.</param>
		/// <param name="w">2nd input vector.</param>
		/// <returns>Resulting matrix = v * w', where w' -- is a transposed w.</returns>
		public static void OuterProduct(float[] v, float[] w, float[][] res)
		{
			int height = v.Length;
			int width = w.Length;
			for (int i = 0; i < height; ++i)
			{
				var resi = res[i];
				for (int j = 0; j < width; ++j)
				{
					resi[j] = v[i] * w[j];
				}
			}
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates sum of absolute values of elements in the given array.
		/// </summary>
		/// <param name="mas">Input array.</param>
		/// <returns>Sum of absolute values.</returns>
		public static float SumAbs(ICollection<float> mas)
		{
			float res = 0.0f;
			foreach (var d in mas)
			{
				res += Math.Abs(d);
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates sum of elements in the given array.
		/// </summary>
		/// <param name="mas">Input array.</param>
		/// <returns>Sum of elements.</returns>
		public static float Sum(float[] mas)
		{
			float res = 0.0f;
			foreach (var d in mas) { res += d; }
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates sum of elements in the given array.
		/// </summary>
		/// <param name="mas">Input array.</param>
		/// <returns>Sum of elements.</returns>
		public static int Sum(ICollection<int> mas)
		{
			int res = 0;
			foreach (var d in mas) { res += d; }
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates square root for each element of the given vector.
		/// </summary>
		/// <param name="v">Input vector.</param>
		/// <returns>Array of square root values.</returns>
		public static float[] Sqrt (float[] v)
		{
			var size = v.Length;
			var res = new float[size];
			for (var i = 0; i < size; i++)
			{
				res[i] = (float)Math.Sqrt(v[i]);
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Linearly rescales values in the array to fall within the interval [min, max]
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		public static float[] ToRange(float[] v, float min, float max)
		{
			var res = new float[v.Length];
			var minv = Min(v);
			var maxv = Max(v);
			if (minv == maxv) return res;

			var factor = (max - min) / (maxv - minv);
			for (int i = 0; i < v.Length; ++i)
			{
				res[i] = factor * (v[i] - minv) + min;
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Linearly rescales values in the array to fall within the interval [0, 1]
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		public static float[] ToRange01(float[] v)
		{
			var res = new float[v.Length];
			var min = Min(v);
			var max = Max(v);
			if (min == max) return res;

			var range = max - min;
			var factor = 1 / range;
			for (int i = 0; i < v.Length; ++i)
			{
				res[i] = factor * (v[i] - min);
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Performs normalization for the vector [v] so that sum of its components would be 1.
		/// </summary>
		/// <param name="v">Source vector.</param>
		/// <returns>Normalized vector.</returns>
		public static float[] NormalizeSum (float[] v)
		{
			var res = new float[v.Length];
			var sum = Sum(v);
			if (sum == 0f) return res;

			var sum_1 = 1f / sum;
			for (int i = 0; i < res.Length; i++)
			{
				res[i] = v[i]*sum_1;
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Performs normalization for the vector [v] so that its L2 measure is 1.
		/// </summary>
		/// <param name="v">Source vector.</param>
		/// <returns>Normalized vector.</returns>
		public static float[] NormalizeL2(float[] v)
		{
			var res = new float[v.Length];
			var l2 = L2Norm(v);
			if (l2 == 0) return res;

			var l2_1 = 1f / l2;
			for (int i = 0; i < res.Length; i++)
			{
				res[i] = v[i] * l2_1;
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns array of modulus values for elements from the given array.
		/// </summary>
		/// <param name="v">Input array.</param>
		/// <returns>Array of modulus values.</returns>
		public static float[] Abs (Complex[] v)
		{
			var res = new float[v.Length];
			for (int i = 0; i < v.Length; i++)
			{
				res[i] = (float) v[i].Modulus;
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns array of modulus values for elements from the given array.
		/// </summary>
		/// <param name="v">Input array.</param>
		/// <returns>Array of modulus values.</returns>
		public static float[] Abs(float[] v)
		{
			var res = (float[])v.Clone();
			for (int i = 0; i < v.Length; i++)
			{
				if (res[i] < 0)
				{
					res[i] = -res[i];
				}
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Computes reciprocals for each element of the input vector.
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		public static float[] Reciprocals (float[] v)
		{
			var size = v.Length;
			var res = new float[size];
			for (int i = 0; i < size; i++)
			{
				res[i] = 1f/v[i];
			}
			return res;
		}
		#endregion

		#region - Conversion. -
		/// <summary>
		/// [atomic]
		/// 
		/// Extracts real parts of given complex numbers.
		/// </summary>
		/// <param name="v">Input vector of complex entries.</param>
		/// <returns>Array of real parts.</returns>
		public static float[] Re (Complex[] v)
		{
			var res = new float[v.Length];
			for (int i = 0; i < v.Length; i++)
			{
				res[i] = (float)v[i].Real;
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Reverses the order of list elements.
		/// </summary>
		/// <typeparam name="T">Input array element type.</typeparam>
		/// <param name="v">Input array.</param>
		/// <returns>Resulting reversed array.</returns>
		public static List<T> Reverse<T>(List<T> v)
		{
			int len = v.Count;
			var res = new List<T>();

			for (int i = 0; i < len; ++i)
			{
				res.Add( v[len - i - 1] );
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Reverses the order of list elements.
		/// </summary>
		/// <typeparam name="T">Input array element type.</typeparam>
		/// <param name="v">Input array.</param>
		/// <returns>Resulting reversed array.</returns>
		public static T[] Reverse<T>(T[] v)
		{
			int len = v.Length;
			var res = new T[len];

			for (int i = 0; i < len; ++i)
			{
				res[i] = v[len - i - 1];
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Creates permutation of elements of the given vector.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="v"></param>
		/// <returns></returns>
		public static T[] Permutation<T> (T[] v)
		{
			int len = v.Length;
			var res = new T[len];
			var temp = new List<T>(v);

			for (int i = 0; i < len; i++)
			{
				var idx = ContextRandom.Next(temp.Count);
				res[i] = temp[idx];
				temp.RemoveAt(idx);
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Converst given 1D array into array of strings.
		/// </summary>
		/// <typeparam name="T">Input array element type.</typeparam>
		/// <param name="v">Input array.</param>
		/// <returns>Resulting array of strings.</returns>
		public static string[] ConvertToStringsArray<T> (T[] v)
		{
			int len = v.Length;
			var res = new string[len];

			for (int i=0; i<len; ++i)
			{
				res[i] = "" + v[i];
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Convert given vector into the vector of floats.
		/// </summary>
		/// <param name="v">Input vector.</param>
		/// <returns>Resulting vector.</returns>
		public static float[] ConvertToFloats(byte[] v)
		{
			var res = new float[v.Length];
			for (var i = 0; i < v.Length; i++) { res[i] = v[i]; }
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Convert given vector into the vector of floats.
		/// </summary>
		/// <param name="v">Input vector.</param>
		/// <returns>Resulting vector.</returns>
		public static float[] ConvertToFloats(double[] v)
		{
			var res = new float[v.Length];
			for (var i = 0; i < v.Length; i++) { res[i] = (float)v[i]; }
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Convert given vector of floats into the vector of doubles.
		/// </summary>
		/// <param name="v">Input vector.</param>
		/// <returns>Resulting vector.</returns>
		public static double[] ConvertToDoubles (float[] v)
		{
			var res = new double[v.Length];
			for (int i = 0; i < v.Length; i++) {res[i] = v[i];}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Converts given array to the single string using provided separator.
		/// </summary>
		/// <typeparam name="T">Array element type.</typeparam>
		/// <param name="v">Input array.</param>
		/// <param name="sep">Separator.</param>
		/// <returns>Resulting string.</returns>
		public static string ConvertToString<T> (T[] v, char sep)
		{
			var res = v[0].ToString();
			for (int i = 1; i < v.Length; i++)
			{
				res += sep + v[i].ToString();
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Splits given vector into [partsCount] non-intersecting parts of equal lengths.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="v"></param>
		/// <param name="partsCount"></param>
		/// <returns></returns>
		public static List<T[]> Split<T> (T[] v, int partsCount)
		{
			var res = new List<T[]>();
			var len = v.Length/partsCount;

			// get the first (partsCount-1) vectors.
			for (int i = 0; i < partsCount-1; i++)
			{
				var temp = new T[len];
				for (int j = 0; j < len; j++)
				{
					temp[j] = v[i*len + j];
				}
				res.Add(temp);
			}

			// the last vector is a remainder of what's left in [v].
			var last = new T[v.Length - (partsCount - 1) * len];
			for (int i = (partsCount - 1) * len, count = 0; i < v.Length; i++, count++)
			{
				last[count] = v[i];
			}
			res.Add(last);

			return res;
		}
		#endregion

		#region - Comparison. -
		/// <summary>
		/// [atomic]
		/// 
		/// Deines whether two given vectors are equal with respect to the given precision.
		/// </summary>
		/// <param name="v1">1st vector.</param>
		/// <param name="v2">2nd vector.</param>
		/// <param name="eps">Precision parameter.</param>
		/// <returns>[True] if vectors are equal and [False] otherwise.</returns>
		public static bool Equal (float[] v1, float[] v2, float eps)
		{
			if (v1.Length != v2.Length) return false;

			for (int i = 0; i < v1.Length; i++)
			{
				var diff = Math.Abs(v1[i] - v2[i]);
				if ((diff > eps && eps == 0f)
					|| (diff >= eps && eps > 0f))
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Method to define whether 1st number is less than the 2nd one.
		/// </summary>
		/// <param name="f1">1st number.</param>
		/// <param name="f2">2nd number.</param>
		/// <returns>[True] if (f1 less f2) and [False] otherwise.</returns>
		public static bool Less (float f1, float f2) {return f1 < f2;}

		/// <summary>
		/// Method to define whether 1st number is more than the 2nd one.
		/// </summary>
		/// <param name="f1">1st number.</param>
		/// <param name="f2">2nd number.</param>
		/// <returns>[True] if (f1 > f2) and [False] otherwise.</returns>
		public static bool More(float f1, float f2) { return f1 > f2; }

		/// <summary>
		/// [molecule]
		/// 
		/// Checks whether given vector is sorted in the descending order.
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		public static bool IsSorted (float[] v)
		{
			for (int i = 0; i < v.Length-1; i++)
			{
				if (v[i] < v[i + 1]) { return false; }
			}
			return true;
		}
		#endregion

		#region - Sampling. - 
		/// <summary>
		/// [molecule]
		/// 
		/// Returns result of roulette-like procedure for probabilistic selection from the given array.
		/// </summary>
		/// <param name="v">Input array.</param>
		/// <returns>Index of the selected element.</returns>
		public static int Roulette (float[] v)
		{
			var sum = Sum(v);

			var tempSum = ContextRandom.NextDouble() * sum;
			int count = 0;
			foreach (var val in v)
			{
				tempSum -= val;
				if (tempSum <= 0) return count;
				count++;
			}
			return (v.Length - 1);
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Returns result of roulette-like procedure for probabilistic selection from the given array.
		/// Each value is increased by the [shift] variable.
		/// This may be useful to enable selection of elements with zero weight.
		/// </summary>
		/// <param name="v">Input array.</param>
		/// <param name="shift">Default element weight (added to each entry in the input vector).</param>
		/// <returns>Index of the selected element.</returns>
		public static int Roulette(ICollection<float> v, float shift)
		{
			var tempV = new float[v.Count];
			int i = 0;
			float sum = 0;
			foreach (var elem in v)
			{
				tempV[i] = elem + shift;
				sum += tempV[i];
				++i;
			}
			//var sum = Sum(tempV);

			var tempSum = ContextRandom.NextDouble() * sum;
			int count = 0;
			foreach (var val in tempV)
			{
				tempSum -= val;
				if (tempSum <= 0) return count;
				count++;
			}
			return (v.Count - 1);
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns subarray sampled by indices from the given array.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="v">Input array.</param>
		/// <param name="inds">Indices to sample.</param>
		/// <returns>Sampled subarray.</returns>
		public static T[] SampleByIndices<T> (T[] v, int[] inds)
		{
			var res = new T[inds.Length];

			for (int i = 0; i < inds.Length; i++)
			{
				var idx = inds[i];
				res[i] = v[idx];
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns collection of randomly picked elements from a given array.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="v"></param>
		/// <param name="size"></param>
		/// <returns></returns>
		public static T[] RandomSample<T>(T[] v, int size)
		{
			var res = new T[size];
			for (int i = 0; i < size; ++i )
			{
				res[i] = v[ContextRandom.Next (v.Length)];
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns random element from the given array.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="v"></param>
		/// <returns></returns>
		public static T GetRandomElement<T> (T[] v)
		{
			var idx = ContextRandom.Next(v.Length);
			return v[idx];
		}
		#endregion

		#region - Validation. -
		/// <summary>
		/// [atomic]
		/// 
		/// Validates given vector and reports whether it contains valid non zero values.
		/// </summary>
		/// <param name="v">Input vector.</param>
		/// <returns>[True] if at least one element is non-zero, not infinite and not NaN, and [False] otherwise.</returns>
		public static bool ContainsNonZeros (float[] v)
		{
			foreach (var val in v)
			{
				if (val != 0 && !float.IsInfinity(val) && !float.IsNaN(val)) return true;
			}
			return false;
		}
		#endregion

		#region - Getting access to the elements. -
		/// <summary>
		/// [atomic]
		/// 
		/// Extracts subvector from the given vector including the elements at the interval's ends.
		/// </summary>
		/// <typeparam name="T">Type name.</typeparam>
		/// <param name="v">Input vector.</param>
		/// <param name="begin">Subvector beginning index.</param>
		/// <param name="end">Subvector ending index.</param>
		/// <returns>Subvector.</returns>
		public static T[] Subvector<T> (T[] v, int begin, int end)
		{
			var size = end - begin + 1;
			var res = new T[size];

			int count = 0;
			for (int i = begin; i <= end; ++i, ++count) res[count] = v[i];

			return res;
		}
		#endregion

		#region - Transforms and basic numerics. -
		/// <summary>
		/// [atomic]
		/// 
		/// Calculates Householder vector from the given vector.
		/// See: Golub, Van Loan 'Matrix Calculus', algorithm at p. 183
		/// </summary>
		/// <param name="x">Input vector.</param>
		/// <returns>Householder vector.</returns>
		public static float[] HouseholderVector(float[] x)
		{
			var v = (float[])x.Clone();
			float mu = L2Norm(v);
			if (mu != 0.0f)
			{
				float beta = x[0] + mu * (x[0] >= 0 ? 1 : -1);
				Mul(ref v, 1f/beta);
			}
			v[0] = 1f;
			return v;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates projection of vector [v2] on [v1].
		/// </summary>
		/// <param name="v1">Vector to project on.</param>
		/// <param name="v2">Vector to be projected.</param>
		/// <returns>Projection vector coordinates.</returns>
		public static float[] Projection (float[] v1, float[] v2)
		{
			var v1norm = DotProduct(v1, v1);
			var v1v2 = DotProduct(v1, v2);
			var coef = v1v2/v1norm;
			return Mul(v1, coef);
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Computes cosine between given vectors.
		/// </summary>
		/// <returns></returns>
		public static float Cosine (float[] v1, float[] v2)
		{
			float v1abs = L2Norm(v1), v2abs = L2Norm(v2);
			float dot = DotProduct(v1, v2);
			return dot/(v1abs*v2abs);
		}
		#endregion
	}
}