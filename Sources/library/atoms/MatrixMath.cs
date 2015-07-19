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
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
//using MentalAlchemy.Molecules;

namespace MentalAlchemy.Atoms
{
	/// <summary>
	/// Class for operations with matrices.
	/// </summary>
	public class MatrixMath
	{
		#region - Getting access to elements. -
		/// <summary>
		/// [atomic]
		/// 
		/// Returns requested row from the given 2D array.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		/// <param name="idx"></param>
		/// <returns></returns>
		public static T[] GetRow<T> (T[,] data, int idx)
		{
			int width = data.GetLength(1);
			T[] res = new T[width];
			for (int i = 0; i < width; ++i )
			{
				res[i] = data[idx, i];
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns requested column from the given 2D array.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		/// <param name="idx"></param>
		/// <returns></returns>
		public static T[] GetColumn<T>(T[,] data, int idx)
		{
			int height = data.GetLength(0);
			var res = new T[height];
			for (int j = 0; j < height; ++j)
			{
				res[j] = data[j, idx];
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Extracts main diagonal from the given 2D array.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		/// <returns></returns>
		public static T[] Diag<T>(T[,] data)
		{
			int height = data.GetLength(0), width = data.GetLength(1);
			var size = Math.Min(height, width);
			var res = new T[size];
			for (int i = 0; i < size; i++) {res[i] = data[i, i];}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Extracts main diagonal from the given 2D array.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		/// <returns></returns>
		public static T[][] Diag<T>(T[] data)
		{
			int size = data.Length;
			var res = new T[size][];
			for (int i = 0; i < size; i++) { res[i][i] = data[i]; }
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns main submatrix of the given matrix.
		/// </summary>
		/// <typeparam name="T">Type name.</typeparam>
		/// <param name="m">Input matrix.</param>
		/// <param name="begin">Submatrix beginning index.</param>
		/// <param name="end">Submatrix ending index.</param>
		/// <returns>Submatrix.</returns>
		public static T[,] Submatrix<T>(T[,] m, int begin, int end)
		{
			int size = end - begin + 1;
			var res = new T[size, size];

			for (int i = begin, rowIdx = 0; i <= end; i++, rowIdx++)
			{
				for (int j = begin, colIdx = 0; j <= end; j++, colIdx++)
				{
					res[rowIdx, colIdx] = m[i, j];
				}
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns arbitrary submatrix of the given matrix.
		/// </summary>
		/// <typeparam name="T">Type name.</typeparam>
		/// <param name="m">Input matrix.</param>
		/// <param name="beginRow">Beginning row index.</param>
		/// <param name="endRow">Ending row index.</param>
		/// <param name="beginCol">Beginning column index.</param>
		/// <param name="endCol">Ending column index.</param>
		/// <returns>Submatrix.</returns>
		public static T[,] Submatrix<T>(T[,] m, int beginRow, int endRow, int beginCol, int endCol)
		{
			int rows = endRow - beginRow + 1;
			int cols = endCol - beginCol + 1;
			var res = new T[rows, cols];

			for (int i = beginRow, rowIdx = 0; i <= endRow; i++, rowIdx++)
			{
				for (int j = beginCol, colIdx = 0; j <= endCol; j++, colIdx++)
				{
					res[rowIdx, colIdx] = m[i, j];
				}
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns arbitrary submatrix of the given matrix.
		/// </summary>
		/// <typeparam name="T">Type name.</typeparam>
		/// <param name="m">Input matrix.</param>
		/// <param name="beginRow">Beginning row index.</param>
		/// <param name="endRow">Ending row index.</param>
		/// <param name="beginCol">Beginning column index.</param>
		/// <param name="endCol">Ending column index.</param>
		/// <returns>Submatrix.</returns>
		public static T[] Submatrix<T>(T[] m, int width, int height, int beginRow, int beginCol, int subHeight, int subWidth)
		{
			var res = new T[subHeight * subWidth];

			for (int i = beginRow; i < beginRow + subHeight; i++)
			{
				var offset = i * width + beginCol;
				var endRowOffset = (i + 1) * width;
				var destOffset = (i - beginRow) * subWidth;
				for (int j = offset, j1 = 0; j < endRowOffset && j1 < subWidth; ++j, ++j1 )
				{
					res[destOffset + j1] = m[j];
				}
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Inserts submatrix into the given matrix at the prescribed indices.
		/// </summary>
		/// <typeparam name="T">Type name.</typeparam>
		/// <param name="m">Input matrix.</param>
		/// <param name="subM">Submatrix.</param>
		/// <param name="beginRow">Row beginning index.</param>
		/// <param name="beginCol">Column ending index.</param>
		public static void SetSubmatrix<T>(ref T[,] m, T[,] subM, int beginRow, int beginCol)
		{
			int height = m.GetLength(0), width = m.GetLength(1);
			int subHeight = subM.GetLength(0), subWidth = subM.GetLength(1);

			for (int i = beginRow, row = 0; i < height && row <= subHeight; i++, row++)
			{
				for (int j = beginCol, col = 0; j < width && col <= subWidth; j++, col++)
				{
					m[i, j] = subM[row, col];
				}
			}


			//throw new NotImplementedException();
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Inserts submatrix into the given matrix at the prescribed indices.
		/// </summary>
		/// <typeparam name="T">Type name.</typeparam>
		/// <param name="m">Input matrix.</param>
		/// <param name="subM">Submatrix.</param>
		/// <param name="beginRow">Row beginning index.</param>
		/// <param name="beginCol">Column ending index.</param>
		public static T[,] SetSubmatrix<T>(T[,] m, T[,] subM, int beginRow, int beginCol)
		{
			var res = (T[,])m.Clone();
			SetSubmatrix(ref res, subM, beginRow, beginCol);
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Inserts submatrix into the given matrix at the prescribed indices.
		/// </summary>
		/// <typeparam name="T">Type name.</typeparam>
		/// <param name="m">Input matrix.</param>
		/// <param name="subM">Submatrix.</param>
		/// <param name="beginRow">Row beginning index.</param>
		/// <param name="beginCol">Column ending index.</param>
		public static void SetSubmatrix<T>(T[] m, int width, int height, T[] subM, int subWidth, int subHeight, int beginRow, int beginCol)
		{
			for (int i = beginRow; i < beginRow + subHeight; ++i )
			{
				var offsetRow = i * width;
				var offsetRowSrc = (i - beginRow) * subWidth;
				for (int j = beginCol, j1 = 0; j < beginCol + subWidth && j1 < subWidth; ++j, ++j1 )
				{
					m[offsetRow + j] = subM[offsetRowSrc + j1];
				}
			}
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Inserts given column into the matrix at the prescribed position.
		/// </summary>
		/// <typeparam name="T">Type name.</typeparam>
		/// <param name="m">Input matrix.</param>
		/// <param name="col"></param>
		/// <param name="pos"></param>
		public static T[,] SetColumn<T>(T[,] m, T[] col, int pos)
		{
			var res = (T[,])m.Clone();
			int height = m.GetLength(0);

			for (int i = 0; i < height; i++)
			{
				res[i, pos] = col[i];
			}

			return res;
		}
		#endregion

		#region - Stats. -
		#region - Min, Max, & Avg. -
		/// <summary>
		/// Returns maximal element in the given matrix.
		/// </summary>
		/// <param name="m">Input matrix.</param>
		/// <returns>Maximal matrix element.</returns>
		public static float Max(float[,] m)
		{
			IEnumerator e = m.GetEnumerator();
			float max = float.MinValue;
			while (e.MoveNext())
			{
				var temp = (float)e.Current;
				if (temp > max)
				{
					max = temp;
				}
			}

			return max;
		}

		/// <summary>
		/// Returns maximal element in the given matrix.
		/// </summary>
		/// <param name="m">Input matrix.</param>
		/// <returns>Maximal matrix element.</returns>
		public static byte Max(byte[,] m)
		{
			IEnumerator e = m.GetEnumerator();
			byte max = byte.MinValue;
			while (e.MoveNext())
			{
				var temp = (byte)e.Current;
				if (temp > max)
				{
					max = temp;
				}
			}

			return max;
		}

		/// <summary>
		/// Returns array of maximal elements for each matrix in the list.
		/// </summary>
		/// <param name="ms">List of matrices.</param>
		/// <returns>Array of maximal matrix elements.</returns>
		public static byte[] MaxList(List<byte[,]> ms)
		{
			var res = new byte[ms.Count];
			for (int i = 0; i < ms.Count; ++i)
			{
				res[i] = Max(ms[i]);
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns minimal element in the given matrix.
		/// </summary>
		/// <param name="m">Input matrix.</param>
		/// <returns>Minimal matrix element.</returns>
		public static float Min(float[,] m)
		{
			IEnumerator e = m.GetEnumerator();
			float min = float.MaxValue;
			while (e.MoveNext())
			{
				var temp = (float)e.Current;
				if (temp < min)
				{
					min = temp;
				}
			}

			return min;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns minimal element in the given matrix.
		/// </summary>
		/// <param name="m">Input matrix.</param>
		/// <returns>Minimal matrix element.</returns>
		public static byte Min(byte[,] m)
		{
			IEnumerator e = m.GetEnumerator();
			byte min = byte.MaxValue;
			while (e.MoveNext())
			{
				var temp = (byte)e.Current;
				if (temp < min)
				{
					min = temp;
				}
			}

			return min;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns array of minimal elements for each matrix in the list.
		/// </summary>
		/// <param name="ms">List of matrices.</param>
		/// <returns>Array of minimal matrix elements.</returns>
		public static byte[] MinList(List<byte[,]> ms)
		{
			var res = new byte[ms.Count];
			for (int i = 0; i < ms.Count; ++i)
			{
				res[i] = Min(ms[i]);
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns mean value in the given matrix.
		/// </summary>
		/// <param name="m">Input matrix.</param>
		/// <returns>Mean value of elements.</returns>
		public static float Mean(float[,] m)
		{
			var sum = Sum(m);
			return sum/m.Length;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns averaged row of the given matrix.
		/// </summary>
		/// <param name="m">Input matrix.</param>
		/// <returns>Averaged row.</returns>
		public static float[] MeanRow(float[,] m)
		{
			var sum = SumRows(m);
			VectorMath.Mul(ref sum, 1.0f / m.GetLength(0));
			return sum;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Counts number of matrix elements above the main diagonal
		/// which equal to the given value.
		/// </summary>
		/// <param name="m"></param>
		/// <param name="val"></param>
		/// <returns></returns>
		public static int CountValuesTriUIgnoreDiag (float[][] m, float val)
		{
			float height = m.Length, width = m[0].Length;

			int count = 0;
			for (int i = 0; i < height; i++)
			{
				for (int j = i+1; j < width; j++)
				{
					if (m[i][j] == val) {++count;}
				}
			}

			return count;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates baseline row-wise statistics: min, max, and mean values, without taking into account diagonal elements.
		/// </summary>
		/// <param name="m">Input matrix.</param>
		/// <returns>List of stats per each row.</returns>
		public static List<Stats> CalculateRowStatsIgnoreDiag (float[,] m)
		{
			var rows = ConvertToRowsListIgnoreDiag(m);

			var res = new List<Stats>();
			foreach (var row in rows)
			{
				var tempStats = new Stats
				                	{
				                		Max = VectorMath.Max(row),
										Min = VectorMath.Min(row),
										Mean = VectorMath.Mean(row)
				                	};

				res.Add(tempStats);
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates stats for the upper triangle part of the given matrix including the main diagonal.
		/// </summary>
		/// <returns></returns>
		public static Stats CalculateTriUStats (float[,] m)
		{
			return VectorMath.CalculateStats(ConvertToVectorTriU(m));
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates stats for the upper triangle part of the given matrix ignoring the main diagonal.
		/// </summary>
		/// <returns></returns>
		public static Stats CalculateTriUStatsIgnoreDiag(float[,] m)
		{
			return VectorMath.CalculateStats(ConvertToVectorTriUIgnoreDiag(m));
		}
		#endregion

		#region - Correlation. -
		/// <summary>
		/// [atomic]
		/// 
		/// Calculates matrix of correlation coefficients for the given list of vectors.
		/// </summary>
		/// <param name="vs">List of vectors.</param>
		/// <returns>Correlation matrix.</returns>
		public static float[,] ComputeCorrelationMatrix (List<float[]> vs)
		{
			int size = vs.Count;

			// 1. calculate means and stddevs.
			var means = VectorMath.MeanList(vs);
			var devs = VectorMath.StdDevList(vs);

			var res = new float[size, size];
			for (var i = 0; i < size; ++i )
			{
				res[i, i] = 1f;
				for (var j=i+1; j<size; ++j)
				{
					res[i, j] = res[j, i] = VectorMath.Correlation(vs[i], means[i], devs[i], vs[j], means[j], devs[j]);
				}
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates matrix of covariances for the given list of vectors.
		/// </summary>
		/// <param name="vs">List of vectors.</param>
		/// <returns>Correlation matrix.</returns>
		public static float[,] ComputeCovarianceMatrix(List<float[]> vs)
		{
			int size = vs.Count;

			// 1. calculate means and stddevs.
			var means = VectorMath.MeanList(vs);
			//var devs = VectorMath.StdDevList(vs);

			var res = new float[size, size];
			for (var i = 0; i < size; ++i)
			{
				res[i, i] = VectorMath.Variance(vs[i]);
				for (var j = i + 1; j < size; ++j)
				{
					res[i, j] = res[j, i] = VectorMath.Covariance(vs[i], means[i], vs[j], means[j]);
				}
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates matrix of biased covariances for the given list of vectors.
		/// </summary>
		/// <param name="vs">List of vectors.</param>
		/// <returns>Correlation matrix.</returns>
		public static float[,] ComputeCovarianceMatrixBiased(List<float[]> vs)
		{
			int size = vs.Count;

			// 1. calculate means and stddevs.
			var means = VectorMath.MeanList(vs);
			//var devs = VectorMath.StdDevList(vs);

			var res = new float[size, size];
			for (var i = 0; i < size; ++i)
			{
				res[i, i] = VectorMath.VarianceBiased(vs[i]);
				for (var j = i + 1; j < size; ++j)
				{
					res[i, j] = res[j, i] = VectorMath.CovarianceBiased(vs[i], means[i], vs[j], means[j]);
				}
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates matrix of correlation coefficients for the given list of vectors.
		/// </summary>
		/// <param name="vs">List of vectors.</param>
		/// <returns>Correlation matrix.</returns>
		public static float[,] ComputeCorrelationMatrixMod (List<float[]> vs)
		{
			int size = vs.Count;

			// 1. calculate means and stddevs.
			var means = VectorMath.MeanList(vs);
			var devs = VectorMath.StdDevList(vs);

			var res = new float[size, size];
			for (var i = 0; i < size; ++i )
			{
				res[i, i] = 0f;
				for (var j=i+1; j<size; ++j)
				{
					res[i, j] = res[j, i] = VectorMath.Correlation(vs[i], means[i], devs[i], vs[j], means[j], devs[j]);
				}
			}
			return res;
		}
		#endregion

		#region - Neighborhood stats. -
		/// <summary>
		/// [atomic]
		/// 
		/// Calculate local means of the given [data] in required [points] with predefined neighborhood [radius].
		/// </summary>
		/// <param name="data">Data.</param>
		/// <param name="points">Points of interest.</param>
		/// <param name="radius">Neighborhood radius.</param>
		/// <returns>2d map of mean values.</returns>
		public static float[,] GetLocalMean(byte[,] data, float[,] points, int radius)
		{
			int height = data.GetLength(0), width = data.GetLength(1);

			var res = new float[height, width];
			for (int i = 0; i < height; ++i)
			{
				for (int j = 0; j < width; ++j)
				{
					if (points[i, j] <= 0.0) { continue; }

					var nStats = GetNeighborhoodStats(data, new Point(j, i), radius);

					//
					// calculate mean in a neighborhood.
					res[i, j] = StructMath.Mean(nStats);
				}	// for (int j = 0; j < width; ++j)
			}	// for (int i = 0; i < height; ++i)
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculate local means of the given [data] in all points with predefined neighborhood [radius].
		/// </summary>
		/// <param name="data">Data.</param>
		/// <param name="radius">Neighborhood radius.</param>
		/// <returns>2d map of mean values.</returns>
		public static float[,] GetLocalMean(byte[,] data, int radius)
		{
			int height = data.GetLength(0), width = data.GetLength(1);

			var res = new float[height, width];
			for (int i = 0; i < height; ++i)
			{
				for (int j = 0; j < width; ++j)
				{
					var nStats = GetNeighborhoodStats(data, new Point(j, i), radius);

					//
					// calculate mean in a neighborhood.
					res[i, j] = StructMath.Mean(nStats);
				}	// for (int j = 0; j < width; ++j)
			}	// for (int i = 0; i < height; ++i)
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculate local variance of the given [data] in required [points] with predefined neighborhood [radius].
		/// </summary>
		/// <param name="data">Data.</param>
		/// <param name="points">Points of interest.</param>
		/// <param name="radius">Neighborhood radius.</param>
		/// <returns>2d map of variance values.</returns>
		public static float[,] GetLocalVariance(byte[,] data, float[,] points, int radius)
		{
			int height = data.GetLength(0), width = data.GetLength(1);

			var res = new float[height, width];
			for (int i = 0; i < height; ++i)
			{
				for (int j = 0; j < width; ++j)
				{
					if (points[i, j] <= 0.0) { continue; }

					var nStats = GetNeighborhoodStats(data, new Point(j, i), radius);

					//
					// calculate variance in a neighborhood.
					float mean = StructMath.Mean(nStats);
					res[i, j] = StructMath.Variance(nStats, mean);
				}	// for (int j = 0; j < width; ++j)
			}	// for (int i = 0; i < height; ++i)
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculate local variance of the given [data] in all points with predefined neighborhood [radius].
		/// </summary>
		/// <param name="data">Data.</param>
		/// <param name="radius">Neighborhood radius.</param>
		/// <returns>2d map of variance values.</returns>
		public static float[,] GetLocalVariance(byte[,] data, int radius)
		{
			int height = data.GetLength(0), width = data.GetLength(1);

			var res = new float[height, width];
			for (int i = 0; i < height; ++i)
			{
				for (int j = 0; j < width; ++j)
				{
					var nStats = GetNeighborhoodStats(data, new Point(j, i), radius);

					//
					// calculate variance in a neighborhood.
					float mean = StructMath.Mean(nStats);
					res[i, j] = StructMath.Variance(nStats, mean);
				}	// for (int j = 0; j < width; ++j)
			}	// for (int i = 0; i < height; ++i)
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculate local entropy of the given [data] in required [points] with predefined neighborhood [radius].
		/// </summary>
		/// <param name="data">Data.</param>
		/// <param name="points">Points of interest.</param>
		/// <param name="radius">Neighborhood radius.</param>
		/// <returns>2d map of entropy values.</returns>
		public static float[,] GetLocalEntropy(byte[,] data, float[,] points, int radius)
		{
			int height = data.GetLength(0), width = data.GetLength(1);
			var res = new float[height,width];

			for (int i = 0; i < height; ++i)
			{
				for (int j = 0; j < width; ++j)
				{
					if (points[i, j] <= 0.0) { continue; }

					//
					// get distribution of values within neighborhood.
					var nStats = GetNeighborhoodStats(data, new Point(j, i), radius);
					res[i, j] = VectorMath.Entropy(nStats.Values);
				}	// for (int j = 0; j < width; ++j)
			}	// for (int i = 0; i < height; ++i)
			return res;
		}

		/// <summary>
		/// [atomic] 
		/// 
		/// Calculate local entropy of the given [data] in all points with predefined neighborhood [radius].
		/// </summary>
		/// <param name="data">Data.</param>
		/// <param name="radius">Neighborhood radius.</param>
		/// <returns>2d map of entropy values.</returns>
		public static float[,] GetLocalEntropy(byte[,] data, int radius)
		{
			int height = data.GetLength(0), width = data.GetLength(1);
			var res = new float[height, width];

			for (int i = 0; i < height; ++i)
			{
				for (int j = 0; j < width; ++j)
				{
					//
					// get distribution of values within neighborhood.
					var nStats = GetNeighborhoodStats(data, new Point(j, i), radius);

					// calculate real neighborhood size.
					int size = StructMath.SumCounts(nStats);
					float size_1 = 1.0f / size;

					//
					// calculate brightness entropy in a neighborhood.
					float entropy = 0.0f;
					foreach (var pair in nStats)
					{
						float p = pair.Value * size_1;
						entropy -= (float)(p * Math.Log(p, 2));
					}
					res[i, j] = entropy;
				}	// for (int j = 0; j < width; ++j)
			}	// for (int i = 0; i < height; ++i)
			return res;
		}

		///// <summary>
		///// Returns distribution of frequencies of values in [src] at the given [coord] within the square neighborhood of the given [radius].
		///// </summary>
		///// <param name="src">Source table.</param>
		///// <param name="coord">Neighborhood center.</param>
		///// <param name="radius">Neighborhood radius.</param>
		///// <returns>Dictionary of frequencies.</returns>
		//public static Dictionary<float, int> GetNeighborhoodStats(float[,] src, Point coord, int radius)
		//{
		//    var res = new Dictionary<float, int>();

		//    int width = src.GetLength(1), height = src.GetLength(0);
		//    int left = coord.X - radius, right = coord.X + radius,
		//        top = coord.Y - radius, bottom = coord.Y + radius;

		//    if (left < 0) left = 0;
		//    if (right >= width) right = width - 1;
		//    if (top < 0) top = 0;
		//    if (bottom >= height) bottom = height - 1;

		//    for (int i = top; i < bottom; ++i)
		//    {
		//        for (int j = left; j <= right; ++j)
		//        {
		//            float temp = src[i, j];
		//            if (!res.ContainsKey(temp)) { res.Add(temp, 0); }
		//            ++res[temp];
		//        }
		//    }

		//    return res;
		//}

		///// <summary>
		///// [atomic]
		///// 
		///// Returns distribution of frequencies of values in [src] at the given [coord] within the square neighborhood of the given [radius].
		///// </summary>
		///// <param name="src">Source table.</param>
		///// <param name="coord">Neighborhood center.</param>
		///// <param name="radius">Neighborhood radius.</param>
		///// <returns>Dictionary of frequencies.</returns>
		//public static Dictionary<byte, int> GetNeighborhoodStats(byte[,] src, Point coord, int radius)
		//{
		//    var res = new Dictionary<byte, int>();

		//    int width = src.GetLength(1), height = src.GetLength(0);
		//    int left = coord.X - radius, right = coord.X + radius,
		//        top = coord.Y - radius, bottom = coord.Y + radius;

		//    if (left < 0) left = 0;
		//    if (right >= width) right = width - 1;
		//    if (top < 0) top = 0;
		//    if (bottom >= height) bottom = height - 1;

		//    for (int i = top; i < bottom; ++i)
		//    {
		//        for (int j = left; j <= right; ++j)
		//        {
		//            byte temp = src[i, j];
		//            if (!res.ContainsKey(temp)) { res.Add(temp, 0); }
		//            ++res[temp];
		//        }
		//    }

		//    return res;
		//}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns distribution of frequencies of values in [src] at the given [coord] within the square neighborhood of the given [radius].
		/// </summary>
		/// <param name="src">Source table.</param>
		/// <param name="coord">Neighborhood center.</param>
		/// <param name="radius">Neighborhood radius.</param>
		/// <returns>Dictionary of frequencies.</returns>
		public static Dictionary<T, int> GetNeighborhoodStats<T>(T[,] src, Point coord, int radius)
		{
			var res = new Dictionary<T, int>();

			int width = src.GetLength(1), height = src.GetLength(0);
			int left = coord.X - radius, right = coord.X + radius,
				top = coord.Y - radius, bottom = coord.Y + radius;

			if (left < 0) left = 0;
			if (right >= width) right = width - 1;
			if (top < 0) top = 0;
			if (bottom >= height) bottom = height - 1;

			for (int i = top; i < bottom; ++i)
			{
				for (int j = left; j <= right; ++j)
				{
					T temp = src[i, j];
					if (!res.ContainsKey(temp)) { res.Add(temp, 0); }
					++res[temp];
				}
			}

			return res;
		}
		#endregion

		#region - Checking matrix state. -
		/// <summary>
		/// [atomic]
		/// 
		/// Defines whether given matrix contains only zeroes (true) or not (false).
		/// </summary>
		/// <param name="m">Input matrix.</param>
		/// <returns>[True] is [m] contains all-zeroes and [False] otherwise.</returns>
		public static bool IsZero (float[,] m)
		{
			int height = m.GetLength(0), width = m.GetLength(1);
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					if (m[i,j] != 0.0f)
					{
						return false;
					}
				}
			}

			return true;
		}
		#endregion
		#endregion

		#region - Arithmetics. -
		/// <summary>
		/// [atomic]
		/// 
		/// Calculates sum of elements in the given matrix.
		/// </summary>
		/// <param name="m">Input matrix.</param>
		/// <returns>Sum of elements.</returns>
		public static float Sum (float[,] m)
		{
			var e = m.GetEnumerator();
			var sum = 0.0f;
			while (e.MoveNext())
			{
				sum += (float)e.Current;
			}

			return sum;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates sum of elements in the given matrix.
		/// </summary>
		/// <param name="m">Input matrix.</param>
		/// <returns>Sum of elements.</returns>
		public static int Sum(byte[,] m)
		{
			var e = m.GetEnumerator();
			var sum = 0;
			while (e.MoveNext())
			{
				sum += (byte)e.Current;
			}

			return sum;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates sum of elements in the given matrix ignoring the main diagonal.
		/// </summary>
		/// <param name="m">Input matrix.</param>
		/// <returns>Sum of elements.</returns>
		public static int SumIgnoreDiag (int[,] m)
		{
			var height = m.GetLength(0);
			var width = m.GetLength(1);

			var res = 0;
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					if (j == i) continue;
					res += m[i,j];
				}
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates sum of elements in the given matrix ignoring the main diagonal.
		/// </summary>
		/// <param name="m">Input matrix.</param>
		/// <returns>Sum of elements.</returns>
		public static float SumIgnoreDiag(float[,] m)
		{
			var height = m.GetLength(0);
			var width = m.GetLength(1);

			var res = 0f;
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					if (j == i) continue;
					res += m[i, j];
				}
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates row-wise sum of the given matrix.
		/// </summary>
		/// <param name="m">Input matrix.</param>
		/// <returns>Row-wise sum.</returns>
		public static float[] SumRows (float[,] m)
		{
			var rows = ConvertToRowsList(m);
			return SumRows(rows);
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates row-wise sum of the given matrix.
		/// </summary>
		/// <param name="m">Input matrix.</param>
		/// <returns>Row-wise sum.</returns>
		public static float[] SumRows(List<float[]> m)
		{
			var sum = m[0];
			for (int i = 1; i < m.Count; i++)
			{
				VectorMath.AccumulateInplace(sum, m[i]);
			}
			return sum;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates column-wise sum of the given matrix.
		/// </summary>
		/// <param name="m">Input matrix.</param>
		/// <returns>Column-wise sum.</returns>
		public static float[] SumColumns(float[][] m)
		{
			var res = new float[m.Length];
			for (int row = 0; row < m.Length; ++row )
			{
				res[row] = VectorMath.Sum(m[row]);
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Element-wise multiplication of the given matrix on the given number.
		/// </summary>
		/// <param name="m">Input matrix.</param>
		/// <param name="val">Multiplier.</param>
		/// <returns>Resulting matrix.</returns>
		public static float[,] Mul (float[,] m, float val)
		{
			var res = (float[,])m.Clone();
			int height = res.GetLength(0), width = res.GetLength(1);
			for (int i = 0; i < height; ++i)
			{
				for (int j = 0; j < width; ++j)
				{
					res[i, j] *= val;
				}
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Element-wise multiplication of the given matrix on the given number.
		/// </summary>
		/// <param name="m">Input matrix.</param>
		/// <param name="val">Multiplier.</param>
		/// <returns>Resulting matrix.</returns>
		public static double[,] Mul(double[,] m, double val)
		{
			var res = (double[,])m.Clone();
			int height = res.GetLength(0), width = res.GetLength(1);
			for (int i = 0; i < height; ++i)
			{
				for (int j = 0; j < width; ++j)
				{
					res[i, j] *= val;
				}
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Performs right multiplication of the given matrix on the given vector-column.
		/// </summary>
		/// <param name="m">Input matrix.</param>
		/// <param name="val">Multiplier.</param>
		/// <returns>Resulting matrix.</returns>
		public static float[] Mul(float[,] m, float[] val)
		{
			int size = m.GetLength(0);
			var res = new float[size];
			var rows = ConvertToRowsList(m);
			
			for (int i = 0; i < size; ++i)
			{
				res[i] = VectorMath.DotProduct(rows[i], val);
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Performs multiplication of two given matrices.
		/// C = AB.
		/// </summary>
		/// <param name="a">1st matrix.</param>
		/// <param name="b">2nd matrix.</param>
		/// <returns>Resulting matrix.</returns>
		public static float[,] Mul(float[,] a, float[,] b)
		{
			int height = a.GetLength(0), width = b.GetLength(1);
			var res = new float[height, width];
			var rows = ConvertToRowsList(a);
			var cols = ConvertToColumnsList(b);

			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					res[i, j] = VectorMath.DotProduct(rows[i], cols[j]);
				}
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Performs multiplication of two given matrices.
		/// C = AB.
		/// </summary>
		/// <param name="a">1st matrix.</param>
		/// <param name="b">2nd matrix.</param>
		/// <returns>Resulting matrix.</returns>
		public static float[][] Mul(float[][] a, float b)
		{
			int height = a.Length;
			var res = new float[height][];
			for (int i = 0; i < height; i++)
			{
				res[i] = VectorMath.Mul (a[i], b);
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Performs multiplication of two given matrices.
		/// C = AB.
		/// </summary>
		/// <param name="a">1st matrix.</param>
		/// <param name="b">2nd matrix.</param>
		/// <returns>Resulting matrix.</returns>
		public static float[][] Mul(float[][] a, float[][] b)
		{
			int height = a.Length, width = b.Length;
			var res = new float[height][];
			for (int i = 0; i < height; ++i) { res[i] = new float[width]; }
			var rows = ConvertToRowsList(a);
			var cols = ConvertToColumnsList(b);

			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					res[i][j] = VectorMath.DotProduct(rows[i], cols[j]);
				}
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// Multiplies matrix elements by a given number and updates input matrix.
		/// </summary>
		public static void MulInplace(float[][] m, float num)
		{
			for (int i = 0; i < m.Length; ++i )
			{
				VectorMath.MulInplace(m[i], num);
			}
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Performs multiplication of two given matrices.
		/// C = AB.
		/// </summary>
		/// <param name="a">1st matrix.</param>
		/// <param name="b">2nd matrix.</param>
		/// <returns>Resulting matrix.</returns>
		public static double[,] Mul(double[,] a, double[,] b)
		{
			int height = a.GetLength(0), width = b.GetLength(1);
			var res = new double[height, width];
			var rows = ConvertToRowsList(a);
			var cols = ConvertToColumnsList(b);

			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					res[i, j] = VectorMath.DotProduct(rows[i], cols[j]);
				}
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Multiplies a row from the given matrix by a factor starting from a specified column.
		/// The result is stored in the input matrix.
		/// </summary>
		/// <param name="matr">Input matrix.</param>
		/// <param name="row">Row to multiply.</param>
		/// <param name="col">Starting column.</param>
		/// <param name="factor">Multiplication factor.</param>
		public static void MulRow (float[,] matr, int row, int col, float factor)
		{
			var width = matr.GetLength(1);
			for (int j = col; j < width; j++)
			{
				matr[row, j] *= factor;
			}
		}

		/// <summary>
		/// [molecule]
		///  
		/// Performs linear combination of two rows as
		///		row2 = row1 * row1Factor + row2
		/// starting from the specified column.
		/// The result is stored in the input matrix.
		/// </summary>
		/// <param name="matr">Input matrix.</param>
		/// <param name="row1">First row.</param>
		/// <param name="row2">Second row.</param>
		/// <param name="col">Starting column.</param>
		/// <param name="row1Factor">Row 1 factor.</param>
		public static void CombineRows(float[,] matr, int row1, int row2, int col, float row1Factor)
		{
			var width = matr.GetLength(1);
			for (int j = col; j < width; j++)
			{
				matr[row2, j] += matr[row1, j] * row1Factor;
			}
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Computes square root of each element in the given matrix.
		/// </summary>
		/// <param name="m">Input matrix.</param>
		/// <returns>Resulting matrix.</returns>
		public static float[,] Sqrt(float[,] m)
		{
			int size = m.GetLength(0);
			var res = (float[,])m.Clone();
			for (int i = 0; i < size; i++)
			{
				for (int j = 0; j < size; j++)
				{
					if (res[i,j] == 0) continue;
					res[i, j] = (float)Math.Sqrt(res[i, j]);
				}
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Performs multiplication of the given square symmetric matrix on itself.
		/// </summary>
		/// <param name="m">Input matrix.</param>
		/// <returns>Resulting matrix.</returns>
		public static float[,] SqrSym(float[,] m)
		{
			int size = m.GetLength(0);
			var rows = ConvertToRowsList(m);
			var cols = ConvertToColumnsList(m);

			var res = new float[size, size];
			for (int i = 0; i < size; i++)
			{
				res[i, i] = VectorMath.DotProduct(rows[i], rows[i]);
				for (int j = i+1; j < size; ++j)
				{
					res[i, j] = res[j, i] = VectorMath.DotProduct(rows[i], cols[j]);
				}
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Performs normalization to project all elements into [0; 1] range.
		/// The following formula is used:
		///		m[i,j] = (m[i,j] - min) * (max  - min)^{-1},
		/// where min and max -- are minimal and maximal matrix elements.
		/// If min == max then the matrix is not changed.
		/// 
		/// The result is stored in the input martix.
		/// </summary>
		/// <param name="m">Matrix to normalize.</param>
		public static void NormalizeSelf (ref float[,] m)
		{
			float max = Max(m), min = Min(m), delta = max - min;
			if (delta == 0) return;

			float delta_1 = 1.0f/delta;
			int height = m.GetLength(0), width = m.GetLength(1);
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					m[i, j] = (m[i, j] - min) * delta_1;
				}
			}
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Adds given value to each element of the input matrix.
		/// </summary>
		/// <param name="m">Input matrix.</param>
		/// <param name="val">Value to add.</param>
		/// <returns>Addition result.</returns>
        public static float[,] Add (float[,] m, float val)
        {
			int height = m.GetLength(0), width = m.GetLength(1);
			var res = new float[height,width];

        	for (int i = 0; i < height; i++)
        	{
        		for (int j = 0; j < width; j++)
        		{
        			res[i, j] = m[i, j] + val;
        		}
        	}
        	return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Adds two given matrices.
		/// </summary>
		/// <param name="a">1st matrix.</param>
		/// <param name="b">2nd matrix.</param>
		/// <returns>Addition result.</returns>
		public static float[,] Add(float[,] a, float[,] b)
		{
			int height = a.GetLength(0), width = a.GetLength(1);
			var res = (float[,])a.Clone();

			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					res[i, j] += b[i, j];
				}
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Adds two given matrices.
		/// </summary>
		/// <param name="a">1st matrix.</param>
		/// <param name="b">2nd matrix.</param>
		/// <returns>Addition result.</returns>
		public static float[,] Add(float[,] a, float[][] b)
		{
			int height = a.GetLength(0), width = a.GetLength(1);
			var res = (float[,])a.Clone();

			for (int i = 0; i < height; i++)
			{
				var bi = b[i];
				for (int j = 0; j < width; j++)
				{
					res[i, j] += bi[j];
				}
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Performs subtraction of the given matrices.
		/// </summary>
		/// <param name="m1">1st matrix.</param>
		/// <param name="m2">Matrix to sub.</param>
		/// <returns>Addition result.</returns>
		public static float[,] Sub(float[,] m1, float[,] m2)
		{
			int height = m1.GetLength(0), width = m2.GetLength(1);
			var res = new float[height, width];

			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					res[i, j] = m1[i, j] - m2[i, j];
				}
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Performs subtraction of the given matrices.
		/// </summary>
		/// <param name="m1">1st matrix.</param>
		/// <param name="m2">Matrix to sub.</param>
		/// <returns>Addition result.</returns>
		public static float[,] Sub(float[,] m1, float[][] m2)
		{
			int height = m1.GetLength(0), width = m2.GetLength(1);
			var res = new float[height, width];

			for (int i = 0; i < height; i++)
			{
				var m2i = m2[i];
				for (int j = 0; j < width; j++)
				{
					res[i, j] = m1[i, j] - m2i[j];
				}
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Performs subtraction of the given matrices.
		/// </summary>
		/// <param name="m1">1st matrix.</param>
		/// <param name="m2">Matrix to sub.</param>
		/// <returns>Addition result.</returns>
		public static float[][] Sub(float[][] m1, float[][] m2)
		{
			int height = m1.Length, width = m1[0].Length;
			var res = new float[height][];

			for (int i = 0; i < height; i++)
			{
				res[i] = new float[width];
				for (int j = 0; j < width; j++)
				{
					res[i][j] = m1[i][j] - m2[i][j];
				}
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Performs inplace subtraction of the value from a given matrix.
		/// </summary>
		/// <param name="m1">1st matrix.</param>
		/// <param name="m2">Matrix to sub.</param>
		/// <returns>Addition result.</returns>
		public static void SubInplace(float[][] m, float[][] m2)
		{
			int height = m.Length, width = m[0].Length;

			for (int i = 0; i < height; i++)
			{
				var mi = m[i];
				var m2i = m2[i];
				for (int j = 0; j < width; j++)
				{
					mi[j] -= m2i[j];
				}
			}
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Performs inplace subtraction of the value from a given matrix.
		/// </summary>
		/// <param name="m1">1st matrix.</param>
		/// <param name="m2">Matrix to sub.</param>
		/// <returns>Addition result.</returns>
		public static void SubInplace(float[][] m, float v)
		{
			int height = m.Length, width = m[0].Length;

			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					m[i][j] -= v;
				}
			}
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Performs subtraction of the given matrices.
		/// </summary>
		/// <param name="m1">1st matrix.</param>
		/// <param name="m2">Matrix to sub.</param>
		/// <returns>Addition result.</returns>
		public static double[,] Sub(double[,] m1, double[,] m2)
		{
			int height = m1.GetLength(0), width = m2.GetLength(1);
			var res = new double[height, width];

			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					res[i, j] = m1[i, j] - m2[i, j];
				}
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates absolute values for each elemnt in the given matrix.
		/// </summary>
		/// <param name="m">Input matrix.</param>
		/// <returns>Matrix of absolute values.</returns>
		public static float[,] Abs (float[,] m)
		{
			int height = m.GetLength(0), width = m.GetLength(1);
			var res = new float[height, width];

			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					res[i, j] = Math.Abs(m[i, j]);
				}
			}
			return res;
		}

		///// <summary>
		///// [molecule]
		///// 
		///// Performs transposition of the given matrix.
		///// </summary>
		///// <param name="m">Input matrix.</param>
		///// <returns>Transposed matrix.</returns>
		//public static float[,] Transpose (float[,] m)
		//{
		//    var cols = ConvertToColumnsList(m);
		//    return CreateFromRowsList(cols);
		//}

		/// <summary>
		/// [molecule]
		/// 
		/// Performs transposition of the given matrix.
		/// </summary>
		/// <param name="m">Input matrix.</param>
		/// <returns>Transposed matrix.</returns>
		public static T[,] Transpose<T>(T[,] m)
		{
			var cols = ConvertToColumnsList(m);
			return CreateFromRowsList(cols);
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Performs transposition of the given matrix.
		/// </summary>
		/// <param name="m">Input matrix.</param>
		/// <returns>Transposed matrix.</returns>
		public static List<T[]> Transpose<T>(List<T[]> m)
		{
			var res = new List<T[]>();
			int width = m[0].Length, height = m.Count;
			for (int i = 0; i < width; ++i )
			{
				res.Add(new T[height]);
				for (int j = 0; j < height; ++j )
				{
					res[i][j] = m[j][i];
				}
			}
			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Performs transposition of the given matrix.
		/// </summary>
		/// <param name="m">Input matrix.</param>
		/// <returns>Transposed matrix.</returns>
		public static T[][] Transpose<T>(T[][] m)
		{
			int width = m[0].Length, height = m.Length;
			var res = new T[width][];
			for (int i = 0; i < width; ++i)
			{
				res[i] = new T[height];
				for (int j = 0; j < height; ++j)
				{
					res[i][j] = m[j][i];
				}
			}
			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Performs transposition of the given matrix represented by rows.
		/// Works faster than a transpose.
		/// </summary>
		/// <param name="m">Input matrix.</param>
		/// <returns>Transposed matrix.</returns>
		public static T[,] TransposeRows<T>(List<T[]> m)
		{
			var width = m.Count;
			var height = m[0].Length;
			var res = new T[height,width];
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					res[i, j] = m[j][i];
				}
			}
			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Adds two matrices and writes result into the first matrix.
		/// </summary>
		/// <param name="m"></param>
		/// <param name="arg"></param>
		public static void AccumulateInplace (IList<float[]> m, IList<float[]> arg)
		{
			if (m != null && m.Count != 0)
			{
				var size = m.Count;
				for (int i = 0; i < size; i++)
				{
					m[i] = VectorMath.Add(m[i], arg[i]);
				}
			}
			else
			{	// m == null.
				m = new List<float[]>();

				foreach (var v in arg)
				{
					m.Add((float[])v.Clone());
				}
			}
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Adds two matrices and writes result into the first matrix.
		/// This safe version can deal with situation when rows are of different lengths.
		/// </summary>
		/// <param name="m"></param>
		/// <param name="arg"></param>
		public static void AccumulateSafe(ref List<float[]> m, List<float[]> arg)
		{
			if (m != null && m.Count != 0)
			{
				var size = m.Count;
				for (int i = 0; i < size; i++)
				{
					m[i] = VectorMath.AddSafe(m[i], arg[i]);
				}
			}
			else
			{	// m == null.
				m = new List<float[]>();

				foreach (var v in arg)
				{
					m.Add((float[])v.Clone());
				}
			}
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Adds two matrices and writes result into the first matrix.
		/// In this version 'signs alignment' is used which means, that before the addition
		/// the sign of the 2nd term is made the same as the sign of the 2st term.
		/// </summary>
		/// <param name="m"></param>
		/// <param name="arg"></param>
		public static void AccumulateSigned(ref List<float[]> m, List<float[]> arg)
		{
			if (m != null && m.Count != 0)
			{
				var size = m.Count;
				for (int i = 0; i < size; i++)
				{
					for (int j = 0; j < m[i].Length; j++)
					{
						m[i][j] += (m[i][j]*arg[i][j]) > 0 ? arg[i][j] : -arg[i][j];
					}
				}
			}
			else
			{	// m == null.
				m = new List<float[]>();

				foreach (var v in arg)
				{
					m.Add((float[])v.Clone());
				}
			}
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Computes reciprocals for every element of the given matrix.
		/// </summary>
		/// <param name="m"></param>
		/// <returns></returns>
		public static float[,] Reciprocals (float[,] m)
		{
			var res = (float[,])m.Clone();

			for (int i = 0; i < res.GetLength(0); i++)
			{
				for (int j = 0; j < res.GetLength(1); j++)
				{
					res[i, j] = 1f/res[i, j];
				}
			}

			return res;
		}
		#endregion

		#region - Conversion. -
		/// <summary>
		/// [atomic]
		/// 
		/// Converts given 2d array into list of rows.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data">Input 2d array.</param>
		/// <returns>List of rows.</returns>
		public static List<T[]> ConvertToRowsList<T> (T[,] data)
		{
			var res = new List<T[]>();
			int size = data.GetLength(0), width = data.GetLength(1);
			for (int i=0; i<size; ++i)
			{
				var temp = new T[width];
				for (int j = 0; j < width; ++j)
				{
					temp[j] = data[i,j];
				}
				res.Add(temp);
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Converts given 2d array into list of rows.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data">Input 2d array.</param>
		/// <returns>List of rows.</returns>
		public static List<T[]> ConvertToRowsList<T>(T[][] data)
		{
			var res = new List<T[]>();
			int size = data.Length, width = data[0].Length;
			for (int i = 0; i < size; ++i)
			{
				var temp = new T[width];
				for (int j = 0; j < width; ++j)
				{
					temp[j] = data[i][j];
				}
				res.Add(temp);
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Converts given 2d array into list of columns.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data">Input 2d array.</param>
		/// <returns>List of columns.</returns>
		public static List<T[]> ConvertToColumnsList<T>(T[,] data)
		{
			var res = new List<T[]>();
			int height = data.GetLength(0), width = data.GetLength(1);
			for (int j = 0; j < width; j++)
			{
				var temp = new T[height];
				for (int i = 0; i < height; i++)
				{
					temp[i] = data[i, j];
				}
				res.Add(temp);
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Converts given 2d array into list of columns.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data">Input 2d array.</param>
		/// <returns>List of columns.</returns>
		public static List<T[]> ConvertToColumnsList<T>(T[][] data)
		{
			var res = new List<T[]>();
			int height = data.Length, width = data[0].Length;
			for (int j = 0; j < width; j++)
			{
				var temp = new T[height];
				for (int i = 0; i < height; i++)
				{
					temp[i] = data[i][j];
				}
				res.Add(temp);
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Converts given 2d array into list of columns.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data">Input 2d array.</param>
		/// <returns>List of columns.</returns>
		public static List<string> ConvertToColumnsStringsList<T>(T[,] data)
		{
			var res = new List<string>();
			var cols = ConvertToColumnsList(data);
			foreach (var col in cols)
			{
				res.Add(VectorMath.ConvertToString(col, '\t'));
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Converts given 2d array into list of rows ignoring the diagonal elements.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data">Input 2D array.</param>
		/// <returns>List of rows without diagonal elements.</returns>
		public static List<T[]> ConvertToRowsListIgnoreDiag<T>(T[,] data)
		{
			var res = new List<T[]>();
			int size = data.GetLength(0), width = data.GetLength(1);
			for (int i = 0; i < size; ++i)
			{
				var temp = new T[width-1];
				for (int j = 0, count = 0; j < width; ++j)
				{
					if (j == i) continue;
					temp[count] = data[i, j];
					++count;
				}
				res.Add(temp);
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Converts given 2d array into an 1d vector.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data">Input 2d array.</param>
		/// <returns>Vector containing input array's elements.</returns>
		public static T[] ConvertToVector<T>(IList<T[]> data)
		{
			int height = data.Count, width = data[0].Length;
			var res = new T[height * width];
			for (int i = 0; i < height; i++)
			{
				Array.Copy(data[i], 0, res, i * width, width);
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Converts given 2d array into an 1d vector.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data">Input 2d array.</param>
		/// <returns>Vector containing input array's elements.</returns>
		public static T[] ConvertToVector<T>(T[,] data)
		{
			var res = new T[data.Length];
			int height = data.GetLength(0), width = data.GetLength(1);
			for (int i = 0, count = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++, ++count)
				{
					res[count] = data[i, j];
				}
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Converts upper triangle submatrix of the given 2d array including the main diagonal into an 1d vector.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data">Input 2d array.</param>
		/// <returns>Vector containing input array's elements.</returns>
		public static T[] ConvertToVectorTriU<T>(T[,] data)
		{
			//
			// calculate number of elements in the resulting array using formula for the sum of arithmetic series.
			int width = data.GetLength(1), height = data.GetLength(0);
			var minDim = Math.Min(width, height);
			int count = (2 * width - minDim + 1) * minDim / 2;
			
			var res = new T[count];
			int idx = 0;
			for (int i = 0; i < height; i++)
			{
				for (int j = i; j < width; j++, idx++)
				{
					res[idx] = data[i, j];
				}
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Converts upper triangle submatrix of the given 2d array ignoring the main diagonal into an 1d vector.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data">Input 2d array.</param>
		/// <returns>Vector containing input array's elements.</returns>
		public static T[] ConvertToVectorTriUIgnoreDiag<T>(T[,] data)
		{
			//
			// calculate number of elements in the resulting array using formula for the sum of arithmetic series.
			int width = data.GetLength(1), height = data.GetLength(0);
			var minDim = Math.Min(width, height);
			int count = (2 * width - minDim - 1) * minDim / 2;

			var res = new T[count];
			int idx = 0;
			for (int i = 0; i < height; i++)
			{
				for (int j = i+1; j < width; j++, idx++)
				{
					res[idx] = data[i, j];
				}
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		///
		/// Converts given list of 1D arrays into the list of string arrays.
		/// </summary>
		/// <typeparam name="T">Input array element type.</typeparam>
		/// <param name="list">Input list of 1D arrays.</param>
		/// <returns>List of string arrays.</returns>
		public static List<string[]> ConvertToStringsList<T> (List<T[]> list)
		{
			int height = list.Count;
			var res = new List<string[]>();

			for (int i = 0; i < height; i++)
			{
				res.Add(VectorMath.ConvertToStringsArray(list[i]));
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		///
		/// Converts given 2D array into the list of row string arrays.
		/// </summary>
		/// <typeparam name="T">Input array element type.</typeparam>
		/// <param name="data">Input 2D array.</param>
		/// <param name="separator">Symbol separating entries within a row.</param>
		/// <returns>List of row string arrays.</returns>
		public static List<string> ConvertToRowsStringsList<T>(T[,] data, char separator)
		{
			var rows = ConvertToRowsList(data);
			var res = new List<string>();
			foreach (var ts in rows)
			{
				res.Add( VectorMath.ConvertToString(ts, separator) );
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		///
		/// Converts given 2D array into the list of row string arrays.
		/// </summary>
		/// <typeparam name="T">Input array element type.</typeparam>
		/// <param name="data">Input 2D array.</param>
		/// <param name="separator">Symbol separating entries within a row.</param>
		/// <returns>List of row string arrays.</returns>
		public static List<string> ConvertToRowsStringsList<T>(T[][] data, char separator)
		{
			int height = data.Length;
			var res = new List<string>();
			for (int i = 0; i < height; ++i )
			{
				res.Add (VectorMath.ConvertToString(data[i], separator));
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		///
		/// Converts given 2D array into the list of row string arrays.
		/// </summary>
		/// <typeparam name="T">Input array element type.</typeparam>
		/// <param name="data">Input 2D array.</param>
		/// <param name="separator">Symbol separating entries within a row.</param>
		/// <returns>List of row string arrays.</returns>
		public static List<string> ConvertToRowsStringsList<T>(List<List<T>> data, char separator)
		{
			var m = CreateFromRowsList(data);
			return ConvertToRowsStringsList(m, separator);
		}

		/// <summary>
		/// [atomic]
		///
		/// Converts given 2D array into the list of row string arrays.
		/// </summary>
		/// <typeparam name="T">Input array element type.</typeparam>
		/// <param name="data">Input 2D array.</param>
		/// <param name="separator">Symbol separating entries within a row.</param>
		/// <returns>List of row string arrays.</returns>
		public static List<string> ConvertToRowsStringsList<T>(List<T[]> data, char separator)
		{
			var m = CreateFromRowsList(data);
			return ConvertToRowsStringsList(m, separator);
		}

		/// <summary>
		/// [atomic]
		///
		/// Converts given 2D array into the list of row string arrays.
		/// Can process matrices with rows of different lengths.
		/// </summary>
		/// <typeparam name="T">Input array element type.</typeparam>
		/// <param name="data">Input 2D array.</param>
		/// <param name="separator">Symbol separating entries within a row.</param>
		/// <returns>List of row string arrays.</returns>
		public static List<string> ConvertToRowsStringsListUneq<T>(List<T[]> data, char separator)
		{
			var res = new List<string>();
			foreach (var row in data)
			{
				var rowStr = VectorMath.ConvertToString(row, separator);
				res.Add(rowStr);
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Converts given list of values into the 2D table of integeres of specified [size].
		/// </summary>
		/// <param name="data">Source list of values.</param>
		/// <param name="size">Resulting table size.</param>
		/// <returns>Resulting table.</returns>
		public static int[,] ConvertTo2DTableInt(IList data, Size size)
		{
			var res = new int[size.Height, size.Width];

			int count = 0;
			for (int i = 0; i < size.Height; ++i)
			{
				for (int j = 0; j < size.Width; ++j, ++count)
				{
					var value = data[count].ToString();
					res[i, j] = int.Parse(value);
				}
			}

			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Convert given matrix into the surface representation with default numerical labels for each entry.
		/// Note: TAB is used to separate labels and data entry.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="m"></param>
		/// <returns></returns>
		public static List<string> ConvertToSurfaceDescr<T>(T[,] m)
		{
			var res = new List<string>();
			const char SEP = '\t';

			int width = m.GetLength(1), height = m.GetLength(0);
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					res.Add(j.ToString() + SEP + i + SEP + m[i, j]);
				}
			}

			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Convert given matrix into the surface representation with correspondent labels for each entry.
		/// Note: TAB is used to separate labels and data entry.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="m"></param>
		/// <param name="lx">X-axis labels.</param>
		/// <param name="ly">Y-axis labels.</param>
		/// <returns></returns>
		public static List<string> ConvertToSurfaceDescr<T> (T[,] m, string[] lx, string[] ly)
		{
			var res = new List<string>();
			const char SEP = '\t';

			int width = m.GetLength(1), height = m.GetLength(0);
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					res.Add(lx[j] + SEP + ly[i] + SEP + m[i,j]);
				}
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Composes a matrix from non-overlapping square patches.
		/// The patches are ordered from Top-Left to Bottom-Right.
		/// </summary>
		/// <param name="patches"></param>
		/// <param name="countWidth">Number of patches in horizontal direction.</param>
		/// <param name="countHeight">Number of patches in vertical direction.</param>
		/// <returns></returns>
		public static float[] ToMatrix(IList<float[]> patches, int patchSize, int countWidth, int countHeight)
		{
			int width = patchSize * countWidth, height = patchSize * countHeight;
			var res = new float[width * height];

			for (int i = 0; i < patches.Count; ++i )
			{
				var row = i / countWidth;
				var col = i % countWidth;
				SetSubmatrix(res, width, height, patches[i], patchSize, patchSize, row * patchSize, col * patchSize);
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Composes a matrix from non-overlapping square patches.
		/// The patches are ordered from Top-Left to Bottom-Right.
		/// </summary>
		/// <param name="patches"></param>
		/// <param name="countWidth">Number of patches in horizontal direction.</param>
		/// <param name="countHeight">Number of patches in vertical direction.</param>
		/// <returns></returns>
		public static float[] ToMatrix(IList<float[]> patches, int patchWidth, int patchHeight, int countWidth, int countHeight)
		{
			int width = patchWidth * countWidth, height = patchHeight * countHeight;
			var res = new float[width * height];

			for (int i = 0; i < patches.Count; ++i)
			{
				var row = i / countWidth;
				var col = i % countWidth;
				SetSubmatrix(res, width, height, patches[i], patchWidth, patchHeight, row * patchHeight, col * patchWidth);
			}

			return res;
		}

		/// <summary>
		/// Breaks the given matrix into non-overlaping adjacent square patches of size [patchSize].
		/// The patches are ordered from Top-Left to Bottom-Right.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="patchSize"></param>
		/// <returns></returns>
		public static List<T[]> ToPatches<T>(T[] data, int width, int height, int patchSize)
		{ 
			var res = new List<T[]> ();
			if (data == null) return res;

			for (int i = 0; i < height; i += patchSize )
			{
				for (int j = 0; j < width; j += patchSize )
				{
					var tmp = Submatrix(data, width, height, i, j, patchSize, patchSize);
					res.Add(tmp);
				}
			}
			return res;
		}
		#endregion

		#region - Modification. -
		#region - Borders adding/removal. -
		/// <summary>
		/// [atomic]
		/// 
		/// Create borders for the given 2d array.
		/// </summary>
		/// <param name="src">Input array.</param>
		/// <param name="hBorder">Vertical border width.</param>
		/// <param name="wBorder">Horizontal border width.</param>
		/// <returns>Modified 2d array.</returns>
		public static float[,] CreateBorders(float[,] src, int hBorder, int wBorder)
		{
			int height = src.GetLength(0), width = src.GetLength(1);
			int resH = height + 2 * hBorder, resW = width + 2 * wBorder;
			var res = new float[resH, resW];

			for (int i = 0; i < height; ++i)
			{
				int idxI = i + hBorder, idxJ = wBorder;
				for (int j = 0; j < width; ++j, ++idxJ)
				{
					res[idxI, idxJ] = src[i, j];
				}
			}

			return res;
		}

		public static float[] CreateBorders(float[] data, int width, int height, int hBorder, int wBorder)
		{ 
			int resW = width + wBorder * 2, resH = height + hBorder * 2;
			var res = new float[resW * resH];
			for (int i = 0; i < height; ++i )
			{
				var offsetSrc = i* width;
				var offsetDest = (i + hBorder) * resW + wBorder;
				Array.Copy (data, offsetSrc, res, offsetDest, width);
			}
			return res;
		}

		public static float[] CreateBorders(float[] data, int width, int height, int hBorder, int wBorder, float borderFill)
		{
			int resW = width + wBorder * 2, resH = height + hBorder * 2;
			var res = VectorMath.Create (resW * resH, borderFill);
			for (int i = 0; i < height; ++i)
			{
				var offsetSrc = i * width;
				var offsetDest = (i + hBorder) * resW + wBorder;
				Array.Copy(data, offsetSrc, res, offsetDest, width);
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Removes borders from the given 2d array.
		/// </summary>
		/// <param name="src">Input array.</param>
		/// <param name="hBorder">Vertical border width.</param>
		/// <param name="wBorder">Horizontal border width.</param>
		/// <returns>Modified 2d array.</returns>
		public static float[,] RemoveBorders(float[,] src, int hBorder, int wBorder)
		{
			int height = src.GetLength(0), width = src.GetLength(1);
			int resH = height - 2 * hBorder, resW = width - 2 * wBorder;
			var res = new float[resH, resW];

			for (int i = 0; i < resH; ++i)
			{
				int idxI = i + hBorder, idxJ = wBorder;
				for (int j = 0; j < resW; ++j, ++idxJ)
				{
					res[i, j] = src[idxI, idxJ];
				}
			}

			return res;
		}

		public static float[] RemoveBorders(float[] data, int width, int height, int hBorder, int wBorder)
		{
			int resW = width - wBorder * 2, resH = height - hBorder * 2;
			var res = new float[resW * resH];
			for (int i = 0; i < resH; ++i)
			{
				var offsetSrc = (i + hBorder) * width + wBorder;
				var offsetDest = i * resW;
				Array.Copy(data, offsetSrc, res, offsetDest, resW);
			}
			return res;
		}
		#endregion

		#region - Removal of columns. -
		/// <summary>
		/// [atomic]
		/// 
		/// Removes specified row from the given matrix.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="m"></param>
		/// <param name="row"></param>
		/// <returns>Modified matrix.</returns>
		public static T[][] RemoveRow<T> (T[][] m, int row)
		{
			int height = m.Length, width = m[0].Length;
			var res = new List<T[]> ();

			for (int i = 0; i < height; i++)
			{
				if (i == row) continue;

				res.Add(m[i]);
			}

			return res.ToArray ();
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Removes specified column from the given matrix.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="m"></param>
		/// <param name="col"></param>
		/// <returns>Modified matrix.</returns>
		public static T[][] RemoveColumn<T>(T[][] m, int col)
		{
			int height = m.Length, width = m[0].Length;
			var res = new T[height][];

			for (int i = 0; i < height; i++)
			{
				res[i] = new T[width - 1];
				for (int j = 0, colCount = 0; j < width; j++)
				{
					if (j == col) continue;

					res[i][colCount] = m[i][j];
					++colCount;
				}
			}

			return res;
		}
		#endregion

		#region - Exchanges. -
		/// <summary>
		/// [atomic]
		/// 
		/// Exchnages rows row1 and row2 in a given matrix. The result is written in the input matrix.
		/// </summary>
		/// <param name="m">Input matrix.</param>
		/// <param name="row1">Row 1 to exchange.</param>
		/// <param name="row2">Row 2 to exchange.</param>
		public static void ExchangeRows<T> (T[,] m, int row1, int row2)
		{
			int width = m.GetLength(1);
			for (int j = 0; j < width; j++)
			{
				T temp = m[row1, j];
				m[row1, j] = m[row2, j];
				m[row2, j] = temp;
			}
		}
		#endregion

		#region - Rotation. -
		/// <summary>
		/// [atomic]
		/// 
		/// Rotates given 2D matrix 90 degrees CCW.
		/// </summary>
		/// <param name="src">Input matrix.</param>
		/// <returns>Resulting matrix.</returns>
		public static float[,] Rotate90CCW (float[,] src)
		{
			int height = src.GetLength(0), width = src.GetLength(1);
			if (height != width || height % 2 == 0)
			{
				throw new NotImplementedException("[Rotate90CCW]: Can't operate with non-square or even sided matrix.");
			}

			var res = new float[width, height];
			for (int i = 0; i < height; i++)
			{
				int idxJ = i;
				for (int j = 0; j < width; j++)
				{
					int idxI = height - j-1;
					res[idxI, idxJ] = src[i, j];
				}
			}
			return res;
		}
		#endregion
		#endregion

		#region - Generation and Creation. -
		/// <summary>
		/// [atomic]
		/// 
		/// Creates matrix from the given set of row vectors.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data">Set of rows.</param>
		/// <returns>Resulting matrix.</returns>
		public static T[,] CreateFromRowsList<T>(List<T[]> data)
		{
			if (data == null || data.Count == 0 || data[0].Length == 0) { return null; }

			int height = data.Count, width = data[0].Length;
			var res = new T[height, width];
			for (int i = 0; i < height; ++i)
			{
				for (int j = 0; j < width; ++j)
				{
					res[i, j] = data[i][j];
				}
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Creates matrix from the given set of row vectors.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data">Set of rows.</param>
		/// <returns>Resulting matrix.</returns>
		public static T[,] CreateFromRowsListSafe<T>(List<T[]> data)
		{
			if (data == null || data.Count == 0 || data[0].Length == 0) { return null; }

			int height = data.Count, width = data[0].Length;
			var res = new T[height, width];
			for (int i = 0; i < height; ++i)
			{
				width = data[i].Length;
				for (int j = 0; j < width; ++j)
				{
					res[i, j] = data[i][j];
				}
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Creates matrix from the given set of row vectors.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data">Set of rows.</param>
		/// <returns>Resulting matrix.</returns>
		public static T[,] CreateFromRowsList<T>(List<List<T>> data)
		{
			if (data == null || data.Count == 0 || data[0].Count == 0) { return null; }

			int height = data.Count, width = data[0].Count;
			var res = new T[height, width];
			for (int i = 0; i < height; ++i)
			{
				for (int j = 0; j < width; ++j)
				{
					res[i, j] = data[i][j];
				}
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Creates matrix from the given set of column vectors.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data">Set of columns.</param>
		/// <returns>Resulting matrix.</returns>
		public static T[,] CreateFromColsList<T>(List<List<T>> data)
		{
			if (data == null || data.Count == 0 || data[0].Count == 0) { return null; }

			int width = data.Count, height = data[0].Count;
			var res = new T[height, width];
			for (int i = 0; i < height; ++i)
			{
				for (int j = 0; j < width; ++j)
				{
					res[i,j] = data[j][i];
				}
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Creates matrix from the given set of column vectors.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="data">Set of columns.</param>
		/// <returns>Resulting matrix.</returns>
		public static T[,] CreateFromColsList<T>(List<T[]> data)
		{
			if (data == null || data.Count == 0 || data[0].Length == 0) { return null; }

			int width = data.Count, height = data[0].Length;
			var res = new T[height, width];
			for (int i = 0; i < height; ++i)
			{
				for (int j = 0; j < width; ++j)
				{
					res[i, j] = data[j][i];
				}
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Creates matrix with the given number of columns from vector.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="vector">Source vector.</param>
		/// <param name="cols">Number of columns.</param>
		/// <returns>Resulting matrix.</returns>
		public static T[,] CreateFromVector<T> (T[] vector, int cols)
		{
			int rows = vector.Length / cols;
			if (rows == 0) { rows = 1; }

			var res = new T[rows, cols];
			if (rows != 1)
			{
				int count = 0;
				for (int i = 0; i < rows; ++i)
				{
					for (int j = 0; j < cols; ++j, ++count)
					{
						res[i, j] = vector[count];
					}
				}
			}
			else
			{
				for (int j = 0; j < cols; ++j)
				{
					res[0, j] = vector[j];
				}
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Create 2D array where cells with coordinates from array of points have value [presVal].
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="height">Table height.</param>
		/// <param name="width">Table width.</param>
		/// <param name="pts">Set of points.</param>
		/// <param name="presVal">Value for the cell with coordinates from [pts].</param>
		/// <returns>Resulting 2D array.</returns>
		public static T[,] CreateFromPoints<T>(int height, int width, List<Point> pts, T presVal)
		{
			var res = new T[height,width];
			foreach (var point in pts)
			{
				res[point.Y, point.X] = presVal;
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Creates matrix of floats parsed from the given list of string arrays.
		/// </summary>
		/// <param name="rows">List of string arrays.</param>
		/// <returns>Created matrix.</returns>
		public static float[][] CreateFromStringsList(List<string[]> rows)
		{
			if (rows == null || rows.Count == 0 || rows[0].Length == 0)
			{
				throw new Exception("[CreateFromStringsList]: Invalid argument.");
			}

			int height = rows.Count, width = rows[0].Length;
			var res = new float[height][];
			
			for (int i = 0; i < height; i++)
			{
				var row = VectorMath.CreateFromStringsArray(rows[i]);
				res[i] = row;
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Creates list of FP array from the list of [Stats] variables.
		/// </summary>
		/// <param name="stats">List of stats data.</param>
		/// <returns>List of floats.</returns>
		public static List<float[]> CreateFromStatsList (List<Stats> stats)
		{
			var res = new List<float[]>();
			foreach (var stat in stats)
			{
				res.Add(VectorMath.CreateFromStats(stat));
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Creates clone of the given matrix.
		/// </summary>
		/// <typeparam name="T">Type name.</typeparam>
		/// <param name="m">Matrix to clone.</param>
		/// <returns>Clone matrix.</returns>
		public static T[,] Clone<T> (T[,] m)
		{
			int height = m.GetLength(0), width = m.GetLength(1);
			var res = new T[height, width];

			for (int i = 0; i < height; ++i)
			{
				for (int j = 0; j < width; j++) {res[i, j] = m[i, j];}
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Creates identity matrix of the requested size.
		/// </summary>
		/// <param name="size">Matrix size.</param>
		/// <returns>Identity matrix.</returns>
		public static float[,] Identity (int size)
		{
			var res = new float[size,size];
			for (int i = 0; i < size; i++) {res[i, i] = 1;}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Creates identity matrix of the requested size.
		/// </summary>
		/// <param name="size">Matrix size.</param>
		/// <returns>Identity matrix.</returns>
		public static double[,] IdentityD(int size)
		{
			var res = new double[size, size];
			for (int i = 0; i < size; i++) { res[i, i] = 1; }
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Create matrix of 1's.
		/// </summary>
		/// <param name="height">Number of rows.</param>
		/// <param name="width">Number of columns.</param>
		/// <returns>Resulting matrix.</returns>
		public static float[,] Ones (int height, int width)
		{
			var res = new float[height, width];
			for (int i = 0; i < height; ++i )
			{
				for (int j = 0; j < width; ++j)
				{
					res[i, j] = 1.0f;
				}
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Create matrix of 1's.
		/// </summary>
		/// <param name="height">Number of rows.</param>
		/// <param name="width">Number of columns.</param>
		/// <returns>Resulting matrix.</returns>
		public static float[][] Zeros(int height, int width)
		{
			var res = new float[height][];
			for (int i = 0; i < height; ++i)
			{
				res[i] = new float[width];
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Creates random martix of the given size using provided RNG.
		/// [Random.NextDouble] method is used to fill the matrix.
		/// </summary>
		/// <param name="height">Height.</param>
		/// <param name="width">Width.</param>
		/// <param name="rand">Random numbers generator.</param>
		/// <returns>Resulting matrix.</returns>
		public static float[][] Random(int height, int width, Random rand)
		{
			if (height <= 0 || width <= 0) { throw new Exception("[Random]: Matrix deimension can not be below 0."); }

			var res = new float[height][];
			for (int i = 0; i < height; ++i)
			{
				var row = new float[width];
				for (int j = 0; j < width; ++j)
				{
					row[j] = (float)rand.NextDouble();
				}
				res[i] = row;
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Creates random symmetric martix of the given size using provided RNG.
		/// [Random.NextDouble] method is used to fill the matrix.
		/// </summary>
		/// <param name="size">Matrix size.</param>
		/// <param name="rand">Random numbers generator.</param>
		/// <returns>Resulting matrix.</returns>
		public static float[,] RandomSymmetric(int size, Random rand)
		{
			if (size <= 0) { throw new Exception("[Random]: Matrix deimension can not be below 1."); }

			var res = new float[size, size];
			for (int i = 0; i < size; ++i)
			{
				res[i, i] = (float)rand.NextDouble();
				for (int j = i + 1; j < size; ++j)
				{
					res[i, j] = res[j, i] = (float)rand.NextDouble();
				}
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Creates random symmetric martix of the given size using provided RNG.
		/// [Random.NextDouble] method is used to fill the matrix.
		/// 
		/// The matrix is created as A * A', where A -- random matrix.
		/// </summary>
		/// <param name="size">Matrix size.</param>
		/// <param name="rand">Random numbers generator.</param>
		/// <returns>Resulting matrix.</returns>
		public static float[][] RandomCovariance(int size, Random rand)
		{
			if (size <= 0) { throw new Exception("[Random]: Matrix dimension can not be below 1."); }

			// create random matrix.
			var temp = Random(size, size, rand);
			var transp = Transpose(temp);

			var res = Mul(temp, transp);
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Creates random symmetric martix of the given size using provided RNG.
		/// [Random.NextDouble] method is used to fill the matrix.
		/// </summary>
		/// <param name="size">Matrix size.</param>
		/// <param name="rand">Random numbers generator.</param>
		/// <returns>Resulting matrix.</returns>
		public static float[,] RandomCovarianceEx(int size, Random rand)
		{
			if (size <= 0) { throw new Exception("[Random]: Matrix dimension can not be below 1."); }

			var res = new float[size, size];
			var max = 0f;
			for (int i = 0; i < size; ++i)
			{
				for (int j = i + 1; j < size; ++j)
				{
					var val = (float)rand.NextDouble();
					res[i, j] = res[j, i] = ContextRandom.NextBoolean() ? val : -val;
					val = Math.Abs(val);
					if (val > max) { max = val; }
				}
			}

			//
			// set diagonal elements, knowing that they can't be less then non-diagonal.
			for (int i = 0; i < size; i++)
			{
				res[i, i] = max + (float)Math.Abs(rand.NextDouble());
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Creates diagonal matrix with the given main diagonal.
		/// </summary>
		/// <param name="diag">Diagonal values.</param>
		/// <returns>Resulting matrix.</returns>
		public static T[,] Diagonal<T> (T[] diag)
		{
			var size = diag.Length;
			var res = new T[size,size];

			for (int i = 0; i < size; i++)
			{
				res[i, i] = diag[i];
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Converts matrix of ints into matrix of floats.
		/// </summary>
		/// <param name="m">Input matrix.</param>
		/// <returns>Converted matrix.</returns>
		public static float[][] ConvertToFloats (int[][] m)
		{
			int height = m.Length, width = m[0].Length;
			var res = new float[height][];

			for (int i = 0; i < height; i++)
			{
				res[i] = new float[width];
				for (int j = 0; j < width; j++)
				{
					res[i][j] = m[i][j];
				}
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Converts matrix of doubles into matrix of floats.
		/// </summary>
		/// <param name="m">Input matrix.</param>
		/// <returns>Converted matrix.</returns>
		public static float[,] ConvertToFloats(double[,] m)
		{
			int height = m.GetLength(0), width = m.GetLength(1);
			var res = new float[height, width];

			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					res[i, j] = (float)m[i, j];
				}
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Converts matrix of floats into matrix of doubles.
		/// </summary>
		/// <param name="m">Input matrix.</param>
		/// <returns>Converted matrix.</returns>
		public static double[][] ConvertToDoubles(float[][] m)
		{
			int height = m.Length, width = m[0].Length;
			var res = new double[height][];

			for (int i = 0; i < height; i++)
			{
				res[i] = new double[width];
				for (int j = 0; j < width; j++)
				{
					res[i][j] = m[i][j];
				}
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Converts matrix of floats into matrix of doubles.
		/// </summary>
		/// <param name="m">Input matrix.</param>
		/// <returns>Converted matrix.</returns>
		public static double[,] ConvertToDoubles(float[,] m)
		{
			int height = m.GetLength(0), width = m.GetLength(1);
			var res = new double[height,width];

			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					res[i,j] = m[i,j];
				}
			}

			return res;
		}
		#endregion

		#region - Distance measure methods. -
		/// <summary>
		/// [atomic]
		/// 
		/// Calculates all pair-wise distances for given objects, represented by matrices, using prescribed distance measure.
		/// </summary>
		/// <param name="objs">List of objects descriptions.</param>
		/// <param name="dist">Distance measure.</param>
		/// <returns>Distances table.</returns>
		public static float[,] CalculateDistances (List<float[,]> objs, DistanceMeasure2D dist)
		{
			var count = objs.Count;
			var res = new float [count,count];
			for (int i = 0; i < count; i++)
			{
				res[i, i] = 0;
				for (int j = i+1; j < count; j++)
				{
					res[i, j] = res[j, i] = dist(objs[i], objs[j]);
				}
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates Euclidian distance between given 2d arrays.
		/// </summary>
		/// <param name="arg1">1st array.</param>
		/// <param name="arg2">2nd array.</param>
		/// <returns>Euclidian distance value.</returns>
		public static float EuclidianDistance(float[,] arg1, float[,] arg2)
		{
			return (float)Math.Sqrt (EuclidianDistanceSqr(arg1, arg2));
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates squared Euclidian distance between given 2d arrays.
		/// </summary>
		/// <param name="arg1">1st array.</param>
		/// <param name="arg2">2nd array.</param>
		/// <returns>Squared Euclidian distance value.</returns>
		public static float EuclidianDistanceSqr(float[,] arg1, float[,] arg2)
		{
			if (arg1.GetLength(1) != arg2.GetLength(1) || arg1.GetLength(0) != arg2.GetLength(0))
			{
				return float.NaN;
			}

			double result = 0.0;
			int height = arg1.GetLength(0), width = arg1.GetLength(1);
			for (int i = 0; i < height; ++i)
			{
				for (int j = 0; j < width; ++j)
				{
					double temp = arg1[i, j] - arg2[i, j];
					result += temp * temp;
				}
			}

			return (float)result;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates distance between two tables of possibly different heights but with equal widths.
		/// The returned value is a distance measure and satisfies all demands to be a metric.
		/// </summary>
		/// <param name="arg1">1-st table.</param>
		/// <param name="arg2">2-nd table.</param>
		/// <returns>Distance measure.</returns>
		public static float TableDistance(float[,] arg1, float[,] arg2)
		{
			return 0.5f * (TableDistanceOneSide(arg1, arg2) + TableDistanceOneSide(arg2, arg1));
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates assymetric distance between two tables of possibly different heights but with equal widths.
		/// The returned value is not a metric!
		/// </summary>
		/// <param name="arg1">1-st table.</param>
		/// <param name="arg2">2-nd table.</param>
		/// <returns>Assymetric distance measure.</returns>
		public static float TableDistanceOneSide(float[,] arg1, float[,] arg2)
		{
			var res = 0.0f;

			int sizeArg2 = arg2.GetLength(0);	// number of rows.
			if (sizeArg2 > 0)
			{
				var rowsArg1 = ConvertToRowsList(arg1);
				var rowsArg2 = ConvertToRowsList(arg2);
				foreach (var rowArg1 in rowsArg1)
				{
					float minDist = float.MaxValue;

					for (int i = 0; i < sizeArg2; ++i)
					{
						var temp = VectorMath.EuclidianDistance(rowArg1, rowsArg2[i]);
						if (minDist > temp)
						{
							minDist = temp;
						}
					}

					res += minDist;
				}
			}
			else
			{
				throw new Exception("[TableDistanceOneSide]: Can't calculate distance between d1 and an empty description d2");
			}

			return res / arg1.GetLength(0);	// normalize over [arg1] height.
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates distance between two tables of possibly different heights but with equal widths.
		/// The returned value is a distance measure and satisfies all demands to be a metric.
		/// </summary>
		/// <param name="arg1">1-st table.</param>
		/// <param name="arg2">2-nd table.</param>
		/// <returns>Distance measure.</returns>
		public static float TableDistanceAvg(float[,] arg1, float[,] arg2)
		{
			return 0.5f * (TableDistanceOneSideAvg(arg1, arg2) + TableDistanceOneSideAvg(arg2, arg1));
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates assymetric distance between two tables of possibly different heights but with equal widths.
		/// The returned value is not a metric!
		/// </summary>
		/// <param name="arg1">1-st table.</param>
		/// <param name="arg2">2-nd table.</param>
		/// <returns>Assymetric distance measure.</returns>
		public static float TableDistanceOneSideAvg(float[,] arg1, float[,] arg2)
		{
			var res = 0.0f;

			int sizeArg2 = arg2.GetLength(0);	// number of rows.
			float size2_1 = 1f/sizeArg2;
			if (sizeArg2 > 0)
			{
				var rowsArg1 = ConvertToRowsList(arg1);
				var rowsArg2 = ConvertToRowsList(arg2);
				foreach (var rowArg1 in rowsArg1)
				{
					float avgDist = 0;

					for (int i = 0; i < sizeArg2; ++i)
					{
						var temp = VectorMath.EuclidianDistance(rowArg1, rowsArg2[i]);
						avgDist += temp;
					}

					res += avgDist * size2_1;
				}
			}
			else
			{
				throw new Exception("[TableDistanceOneSide]: Can't calculate distance between d1 and an empty description d2");
			}

			return res / arg1.GetLength(0);	// normalize over [arg1] height.
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates distance between two tables of possibly different heights but with equal widths.
		/// The returned value is a distance measure and satisfies all demands to be a metric.
		/// </summary>
		/// <param name="arg1">1-st table.</param>
		/// <param name="arg2">2-nd table.</param>
		/// <returns>Distance measure.</returns>
		public static float TableDistanceMax(float[,] arg1, float[,] arg2)
		{
			return 0.5f * (TableDistanceOneSideMax(arg1, arg2) + TableDistanceOneSideMax(arg2, arg1));
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates assymetric distance between two tables of possibly different heights but with equal widths.
		/// The returned value is not a metric!
		/// </summary>
		/// <param name="arg1">1-st table.</param>
		/// <param name="arg2">2-nd table.</param>
		/// <returns>Assymetric distance measure.</returns>
		public static float TableDistanceOneSideMax(float[,] arg1, float[,] arg2)
		{
			var res = 0.0f;

			int sizeArg2 = arg2.GetLength(0);	// number of rows.
			if (sizeArg2 > 0)
			{
				var rowsArg1 = ConvertToRowsList(arg1);
				var rowsArg2 = ConvertToRowsList(arg2);
				foreach (var rowArg1 in rowsArg1)
				{
					float maxDist = float.MinValue;

					for (int i = 0; i < sizeArg2; ++i)
					{
						var temp = VectorMath.EuclidianDistance(rowArg1, rowsArg2[i]);
						if (maxDist < temp)
						{
							maxDist = temp;
						}
					}

					res += maxDist;
				}
			}
			else
			{
				throw new Exception("[TableDistanceOneSide]: Can't calculate distance between d1 and an empty description d2");
			}

			return res / arg1.GetLength(0);	// normalize over [arg1] height.
		}
		#endregion

		#region - Search. -
		/// <summary>
		/// [atomic]
		/// 
		/// Indicates out whether given list contains the vector or not.
		/// </summary>
		/// <param name="list">List of vectors.</param>
		/// <param name="v">Vector to find.</param>
		/// <returns>[True] if vector is found and [False] otherwise.</returns>
		public static bool Contains (List<float[]> list, float[] v)
		{
			foreach (var vec in list)
			{
				if (VectorMath.Equal(vec, v, 0f)) {return true;}
			}
			return false;
		}
		#endregion

		#region - Filtering. -
		/// <summary>
		/// [atomic]
		/// 
		/// Nulls all elements in the [src] matrix in positions where points[i,j] = 0.
		/// </summary>
		/// <param name="src">Input matrix.</param>
		/// <param name="points">Matrix of points.</param>
		/// <returns>Resulting matrix.</returns>
		public static float[,] PointsFilter (float[,] src, float[,] points)
		{
			int height = src.GetLength(0), width = src.GetLength(1);
			var res = new float[height, width];

			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					res[i, j] = points[i, j] == 0.0f? 0.0f : src[i,j];
				}
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Nulls all elements from the [src] which are less than a given threshold [thr].
		/// </summary>
		/// <param name="src">Source matrix.</param>
		/// <param name="thr">Threshold value.</param>
		/// <returns>Modified matrix.</returns>
		public static float[,] CutDown(float[,] src, float thr)
        {
			int height = src.GetLength(0), width = src.GetLength(1);
			var res = new float[height, width];
			for (int i = 0; i < height; ++i)
			{
				for (int j = 0; j < width; j++)
				{
					res[i, j] = src[i, j] < thr ? 0f : src[i, j];
				}
			}
			return res;
        }

		/// <summary>
		/// [atomic]
		/// 
		/// Clears all values in the specified row.
		/// </summary>
		/// <param name="src">Input matrix.</param>
		/// <param name="row">Row index.</param>
		/// <returns>Modified matrix.</returns>
		public static float[,] ClearRow(float[,] src, int row)
		{
			int width = src.GetLength(1);
			var res = (float[,])src.Clone();
			for (int j=0; j<width; ++j)
			{
				res[row, j] = 0;
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Clears all values in the specified column.
		/// </summary>
		/// <param name="src">Input matrix.</param>
		/// <param name="col">Column index.</param>
		/// <returns>Modified matrix.</returns>
		public static float[,] ClearColumn(float[,] src, int col)
		{
			int height = src.GetLength(0);
			var res = (float[,])src.Clone();
			for (int i = 0; i < height; ++i)
			{
				res[i, col] = 0;
			}
			return res;
		}
		#endregion

		#region - Comparison. -
		/// <summary>
		/// [atomic]
		/// 
		/// Compares two given matrices.
		/// </summary>
		/// <param name="m1">1-st matrix.</param>
		/// <param name="m2">2-nd matrix.</param>
		/// <returns>[True] if matrices are equal and [False] otherwise.</returns>
		public static bool Equals (float[,] m1, float[,] m2)
		{
			int height = m1.GetLength(0), width = m1.GetLength(1);
			if (m2.GetLength(0) != height || m2.GetLength(1) != width)
			{
				return false;
			}

			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					if (m1[i,j] != m2[i,j]) return false;
				}
			}

			return true;
		}
		#endregion

		#region - Sampling of sets of matrices. -
		/// <summary>
		/// [atomic]
		/// 
		/// Returns profile for the given list of byte matrices at the given point.
		/// </summary>
		/// <param name="frames">List of matrices.</param>
		/// <param name="pt">Point to get profile at.</param>
		/// <returns>Requested profile.</returns>
		public static List<byte> GetProfile(List<List<List<byte>>> frames, Point pt)
		{
			var res = new List<byte>();
			foreach (var frame in frames)
			{
				res.Add(frame[pt.X][pt.Y]);
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns profile for the given range in the list of byte matrices at the given point.
		/// </summary>
		/// <param name="frames">List of matrices.</param>
		/// <param name="pt">Point to get profile at.</param>
		/// <param name="start">Index of the first matrix.</param>
		/// <param name="finish">Index of the last matrix.</param>
		/// <returns>Requested profile.</returns>
		public static List<byte> GetProfile(List<List<List<byte>>> frames, Point pt, int start, int finish)
		{
			if (finish < start)
			{
				throw new Exception("[UtilityTools.GetProfile] exception: Invalid arguments");
			}

			var res = new List<byte>();
			for (int i = start; i <= finish && i < frames.Count; ++i)
			{
				res.Add(frames[i][pt.X][pt.Y]);
			}
			return res;
		} 
		#endregion

		#region - Splitting matrix. -
		/// <summary>
		/// [atomic]
		/// 
		/// Splits input matrix into 4 quads. The quads are enumerated in the folowwing order:
		///  _________
		///  | 0 | 1 |
		///  | 2 | 3 |
		///  ---------
		///  </summary>
		/// <param name="m">Input matrix.</param>
		/// <returns>List of quads.</returns>
		public static List<float[,]> QuadroSplit (float[,] m)
		{
			int height = m.GetLength(0);
			int width = m.GetLength(1);

			int w_2 = width/2, h_2 = height/2;
			int w_2_1 = width - w_2, h_2_1 = height - h_2;

			var res = new List<float[,]>();

			//
			// create the first sub-matrix.
			res.Add(new float[h_2, w_2]);
			for (int i=0; i<h_2; ++i)
			{
				for(int j=0; j<w_2; ++j)
				{
					res[0][i, j] = m[i, j];
				}
			}

			//
			// create the second sub-matrix.
			res.Add(new float[h_2, w_2_1]);
			for (int i = 0; i < h_2; ++i)
			{
				for (int j = 0; j < w_2_1; ++j)
				{
					res[1][i, j] = m[i, j + w_2];
				}
			}

			//
			// create the third sub-matrix.
			res.Add(new float[h_2_1, w_2]);
			for (int i = 0; i < h_2_1; ++i)
			{
				for (int j = 0; j < w_2; ++j)
				{
					res[2][i, j] = m[i + h_2, j];
				}
			}

			//
			// create the forth sub-matrix.
			res.Add(new float[h_2_1, w_2_1]);
			for (int i = 0; i < h_2_1; ++i)
			{
				for (int j = 0; j < w_2_1; ++j)
				{
					res[3][i, j] = m[i + h_2, j + w_2];
				}
			}

			return res;
		}
		#endregion

		#region - Numeric methods. -

		/// <summary>
		/// [molecule]
		/// 
		/// Calculates roots of the characteristic polynomial for the given matrix.
		/// </summary>
		/// <param name="matr"></param>
		/// <returns>Array of roots.</returns>
		public static Complex[] CalculateCharacteristicRoots(float[][] matr)
		{
			var dblM = ConvertToDoubles(matr);
			var m = Matrix.Create(dblM);

			var count = m.EigenValues.Length;
			var res = new Complex[count];
			for (var i = 0; i < count; i++)
			{
				res[i] = m.EigenValues[count - i - 1];
			}

			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Computes the Gram matrix for the given set of vectors.
		/// </summary>
		/// <param name="vs">Input set of vectors.</param>
		/// <returns>Gram matrix.</returns>
		public static float[][] ComputeGramMatrixAsJaggedArray(IList<float[]> vs)
		{
			var size = vs.Count;
			var res = Zeros (size, size);
			for (int i = 0; i < size; ++i)
			{
				var resi = res[i];
				resi[i] = VectorMath.DotProduct(vs[i], vs[i]);
				for (int j = i + 1; j < size; j++)
				{
					resi[j] = res[j][i] = VectorMath.DotProduct(vs[i], vs[j]);
				}
			}

			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Computes the Gram matrix for the given set of vectors.
		/// </summary>
		/// <param name="vs">Input set of vectors.</param>
		/// <returns>Gram matrix.</returns>
		public static float[,] ComputeGramMatrixAs2DArray (IList<float[]> vs)
		{
			var size = vs.Count;
			var res = new float[size,size];
			for (int i=0; i<size; ++i)
			{
				res[i, i] = VectorMath.DotProduct(vs[i], vs[i]);
				for (int j = i+1; j < size; j++)
				{
					res[i, j] = res[j, i] = VectorMath.DotProduct(vs[i], vs[j]);
				}
			}

			return res;
		}
		#endregion

		#region - Misc. -
		/// <summary>
		/// [atomic]
		/// 
		/// Checks whether the given matrix is upper triangle using specified accuracy parameter.
		/// </summary>
		/// <param name="m">Input matrix.</param>
		/// <param name="eps">Accuracy.</param>
		/// <returns>[True] if matrix is upper triangle, and [False] otherwise.</returns>
		public static bool CheckTriU(double[,] m, double eps)
		{
			int size = m.GetLength(0);

			for (int i = 1; i < size; ++i)
			{
				for (int j = 0; j < i; ++j)
				{
					if (Math.Abs(m[i, j]) >= eps)
					{
						return false;
					}
				}
			}
			return true;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Checks whether the given matrix is in upper Hessenberg form using specified accuracy parameter.
		/// </summary>
		/// <param name="m">Input matrix.</param>
		/// <param name="eps">Accuracy.</param>
		/// <returns>[True] if matrix is upper Hessenberg, and [False] otherwise.</returns>
		public static bool CheckHessenbergU(double[,] m, double eps)
		{
			int size = m.GetLength(0);

			for (int i = 2; i < size; ++i)
			{	// starting from the 3rd row.
				for (int j = 0; j < i-1; ++j)
				{
					if (Math.Abs(m[i, j]) >= eps)
					{
						return false;
					}
				}
			}
			return true;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Checks whether matrix contains valid numbers (not NaN, or inf). Returns [true] if the matrix is valid and [false] otherwise.
		/// </summary>
		/// <param name="m"></param>
		/// <returns></returns>
		public static bool CheckValues (float[,] m)
		{
			foreach (var elem in m)
			{
				if (float.IsNaN(elem) || float.IsInfinity(elem)) return false;
			}
			return true;
		}
		#endregion
	}
}