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
using System.Drawing;

namespace MentalAlchemy.Atoms
{
	public class StructMath
	{
		#region - Methods for [PointF]. -
		/// <summary>
		/// [atomic]
		/// 
		/// Calculates Euclidian distance between two points.
		/// </summary>
		/// <param name="pt1">1st point.</param>
		/// <param name="pt2">2nd point.</param>
		/// <returns>Euclidian distance value.</returns>
		public static float Distance(PointF pt1, PointF pt2)
		{
			return (float)Math.Sqrt(DistanceSqr(pt1, pt2));
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates squared Euclidian distance between two points.
		/// </summary>
		/// <param name="pt1">1st point.</param>
		/// <param name="pt2">2nd point.</param>
		/// <returns>Squared euclidian distance value.</returns>
		public static float DistanceSqr(PointF pt1, PointF pt2)
		{
			float diffY = pt1.Y - pt2.Y;
			float diffX = pt1.X - pt2.X;
			float res = diffY * diffY + diffX * diffX;

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Defines whether given line contains given point.
		/// </summary>
		/// <param name="lineBeg">Line 1st points.</param>
		/// <param name="lineEnd">Line 2nd points.</param>
		/// <param name="point">Point to consider.</param>
		/// <param name="eps">Checking accuracy.</param>
		/// <returns>[True] if the points belongs to the line with respect to [eps] and [False] otherwise.</returns>
		public static bool CheckLinePoint(PointF lineBeg, PointF lineEnd, PointF point, float eps)
		{
			var res = false;

			double distA = Distance(lineBeg, lineEnd),
					distB = Distance(lineBeg, point),
					distC = Distance(point, lineEnd);

			if ((distB + distC - distA) < eps)
			{
				res = true;
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Defines whether given point belongs to the ellipse border with horizontal orientation.
		/// See http://ru.wikipedia.org/wiki/Эллипс for details.
		/// </summary>
		/// <param name="rect">Bounding rectangle.</param>
		/// <param name="point">Point to consider.</param>
		/// <param name="eps">Accuracy.</param>
		/// <returns>[True] if the points belongs to the ellipse and [False] otherwise.</returns>
		public static bool CheckEllipseBorderHorizontal(RectangleF rect, PointF point, float eps)
		{
			// calculate a and b with offset.
			double a = rect.Width / 2, b = rect.Height / 2;

			// calculate additional parameters.
			double e = Math.Sqrt(1.0 - b * b / (a * a));	// eccentricity.
			double rp = a * (1 - e);						// perifocal distance.

			// calculate focal points
			PointF f1 = new PointF((float)(rect.Left + rp), (float)(rect.Top + b)),
					f2 = new PointF((float)(rect.Right - rp), (float)(rect.Top + b));

			double dist1 = Distance(f1, point),
					dist2 = Distance(f2, point);
			double diff = Math.Abs(dist1 + dist2 - 2 * a);

			//double eps = 5.0;
			if (diff < eps) { return true; }

			return false;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Defines whether given point belongs to the ellipse border with vertical orientation.
		/// See http://ru.wikipedia.org/wiki/Эллипс for details.
		/// </summary>
		/// <param name="rect">Bounding rectangle.</param>
		/// <param name="point">Point to consider.</param>
		/// <param name="eps">Accuracy.</param>
		/// <returns>[True] if the points belongs to the ellipse and [False] otherwise.</returns>
		public static bool CheckEllipseBorderVertical(RectangleF rect, PointF point, float eps)
		{
			// calculate a and b with offset.
			float b = rect.Width / 2, a = rect.Height / 2;

			// calculate additional parameters.
			var e = (float)Math.Sqrt(1.0 - b * b / (a * a));	// eccentricity.
			var rp = a * (1 - e);						// perifocal distance.

			// calculate focal points
			var f1 = new PointF((rect.Left + b), (rect.Top + rp));
			var f2 = new PointF((rect.Left + b), (rect.Bottom - rp));

			var dist1 = Distance(f1, point);
			var dist2 = Distance(f2, point);
			var diff = Math.Abs(dist1 + dist2 - 2 * a);

			return diff < eps;
		}
		#endregion

		#region - Methods for sets of points. -
		/// <summary>
		/// [atomic]
		/// 
		/// Calculates bounding rectangle for the given set of points.
		/// </summary>
		/// <param name="points">List of points.</param>
		/// <returns>Bounding rectangle parameters.</returns>
		public static RectangleF GetBoundingRectangle (List<PointF> points)
		{
			float left = float.MaxValue, right = 0, top = float.MaxValue, bottom = 0;
			foreach(PointF point in points)
			{
				if (left > point.X) { left = point.X; }
				if (right < point.X) { right = point.X; }

				if (top > point.Y) { top = point.Y; }
				if (bottom < point.Y) { bottom = point.Y; }
			}
			return new RectangleF(left, top, (right - left), (bottom - top));
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Flips given array of points horizontally.
		/// </summary>
		/// <param name="points">List of points.</param>
		/// <returns>List of horizontally flipped points.</returns>
		public static List<PointF> FlipHorizontally (List<PointF> points)
		{
			var bRect = GetBoundingRectangle(points);
			var res = new List<PointF>();

			float deltaX = 2.0f*bRect.X + bRect.Width;
			foreach (var point in points)
			{
				var temp = new PointF(- point.X + deltaX, point.Y);
				res.Add(temp);
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Flips given array of points vertically.
		/// </summary>
		/// <param name="points">List of points.</param>
		/// <returns>List of vertically flipped points.</returns>
		public static List<PointF> FlipVertically(List<PointF> points)
		{
			var bRect = GetBoundingRectangle(points);
			var res = new List<PointF>();

			float deltaY = 2.0f * bRect.Y + bRect.Height;
			foreach (var point in points)
			{
				var temp = new PointF(point.X , -point.Y + deltaY);
				res.Add(temp);
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns list of random points lying within the specified size.
		/// </summary>
		/// <param name="size">Points sampling constraints.</param>
		/// <param name="number">Number of points to sample.</param>
		/// <param name="rand">Random numbers generator.</param>
		/// <returns>Set of random points.</returns>
		public static List<Point> SampleRandomPoints(Size size, int number, Random rand)
		{
			var res = new List<Point>();
			for (int i = 0; i < number; i++)
			{
				Point tempPoint;
				do
				{
					int y = rand.Next(0, size.Height);
					int x = rand.Next(0, size.Width);
					tempPoint = new Point(x, y);
				} while (res.Contains(tempPoint));

				res.Add(tempPoint);
			}

			return res;
		}

		#region - Search methods. -
		/// <summary>
		/// [atomic]
		/// 
		/// Returns closest point from the given array of points to the given point using
		///		predefined accuracy [eps].
		/// </summary>
		/// <param name="pts">Array of points.</param>
		/// <param name="point">Point to search.</param>
		/// <param name="eps">Search accuracy.</param>
		/// <returns>Closes point if the search is successfull. Otherwise [PointF.Empty] is returned.</returns>
		public static PointF GetClosestPoint(IList<PointF> pts, PointF point, float eps)
		{
			int index = GetClosestPointIndex(pts, point, eps);
			return (index >= 0) ? pts[index] : PointF.Empty;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Returns index of the closest point from the given array of points to the given point using
		///		predefined accuracy [eps].
		/// </summary>
		/// <param name="pts">Array of points.</param>
		/// <param name="point">Point to search.</param>
		/// <param name="eps">Search accuracy.</param>
		/// <returns>Index of the closes point if the search is successfull. Otherwise -1 is returned.</returns>
		public static int GetClosestPointIndex(IList<PointF> pts, PointF point, float eps)
		{
			PointF tempPt = PointF.Empty;
			int idx = 0;
			foreach (PointF pt in pts)
			{
				float dist = Distance(pt, point);
				if (dist < eps)
				{
					tempPt = pt;
					break;
				}
				++idx;
			}
			return (!tempPt.IsEmpty) ? idx : -1;
		}
		#endregion
		#endregion

		#region - Methods for sparse histograms, represented by dictionaries. -
		/// <summary>
		/// [atomic]
		/// 
		/// Calculates sum of counts in the given dictionary, representing sparse histogram for bytes.
		/// </summary>
		/// <param name="dict">Sparse histogram of bytes.</param>
		/// <returns>Sum of histogram counts.</returns>
		public static int SumCounts<T> (Dictionary<T, int> dict)
		{
			int res = 0;
			foreach (var count in dict.Values) { res += count; }
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates mean of key values from the given sparse histogram of bytes.
		/// </summary>
		/// <param name="dict">Sparse histogram of bytes.</param>
		/// <returns>Mean key value.</returns>
		public static float Mean(Dictionary<byte, int> dict)	
		{
			int size = SumCounts(dict);	

			float mean = 0.0f;
			foreach (var pair in dict)
			{
				float temp = pair.Key;
				mean += temp * pair.Value;
			}
			return mean / size;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates variance of key values from the given sparse histogram of bytes.
		/// </summary>
		/// <param name="dict">Sparse histogram of bytes.</param>
		/// <returns>Variance of key value.</returns>
		public static float Variance(Dictionary<byte, int> dict)
		{
			float mean = Mean(dict);
			return Variance(dict, mean);
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates variance of key values using given mean key value from the given sparse histogram of bytes.
		/// </summary>
		/// <param name="dict">Sparse histogram of bytes.</param>
		/// <param name="mean">Mean key value.</param>
		/// <returns>Variance of key value.</returns>
		public static float Variance(Dictionary<byte, int> dict, float mean)
		{
			int size = SumCounts(dict);

			float var = 0.0f;
			foreach (var pair in dict)
			{
				float temp = pair.Key;
				var += temp * temp * pair.Value;
			}
			var = var / size - mean * mean;
			return var;
		}
		#endregion

		#region - Methods for dictionaries. -
		/// <summary>
		/// [atomic]
		/// 
		/// Converts given dictionary into a list of string arrays, each entries of which consists of two elements -- key and value from dictionary pair.
		/// </summary>
		/// <typeparam name="T1">Dictionary key type.</typeparam>
		/// <typeparam name="T2">Dictionary value type.</typeparam>
		/// <param name="dict">Input dictionary.</param>
		/// <returns>Conversion result.</returns>
		public static List<string[]> ConvertToStringsList<T1,T2> (Dictionary<T1, T2> dict)
		{
			var res = new List<string[]>();
			foreach (var pair in dict)
			{
				var tempList = new List<string>();
                tempList.Add(pair.Key.ToString());
				tempList.Add(pair.Value.ToString());
				res.Add(tempList.ToArray());
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Converts given dictionary into a list of string arrays, each entries of which consists of two elements -- key and value from dictionary pair.
		/// </summary>
		/// <typeparam name="T1">Dictionary key type.</typeparam>
		/// <typeparam name="T2">Dictionary value type.</typeparam>
		/// <param name="dict">Input dictionary.</param>
		/// <returns>Conversion result.</returns>
		public static List<string[]> ConvertToStringsList<T1, T2>(Dictionary<T1, List<T2>> dict)
		{
			var res = new List<string[]>();
			foreach (var pair in dict)
			{
				var tempList = new List<string>();
				tempList.Add(pair.Key.ToString());
				tempList.Add(pair.Value.ToString());
				res.Add(tempList.ToArray());
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Converts given dictionary into an array of strings, separated by the specified character.
		/// </summary>
		/// <typeparam name="T1">Dictionary key type.</typeparam>
		/// <typeparam name="T2">Dictionary value type.</typeparam>
		/// <param name="dict">Input dictionary.</param>
		/// <param name="sep">Separating character.</param>
		/// <returns>Conversion result.</returns>
		public static string[] ConvertToStringsArray<T1, T2>(Dictionary<T1, List<T2>> dict, char sep)
		{
			var res = new List<string>();
			foreach (var pair in dict)
			{
				var str = pair.Key.ToString();
				str += sep + VectorMath.ConvertToString(pair.Value.ToArray(), sep);
				res.Add(str);
			}
			return res.ToArray();
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Converts given list of stats into a list of string arrays.
		/// </summary>
		/// <param name="stats">Input list of stats.</param>
		/// <param name="useHeader">Indicates whether stats header should be returned as well.</param>
		/// <returns>Conversion result.</returns>
		public static List<string> ConvertToStringsList(List<Stats> stats, bool useHeader)
		{
			var res = new List<string>();

			if (useHeader) res.Add(stats[0].GetStatsHeader());
			foreach (var stat in stats)
			{
				res.Add(stat.GetStatsString());
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Computes *dense* ranks for [dict] keys based upon [dict] values.
		/// The less the value the higher the rank.
		/// 
		/// Dense ranking means that the following situations is possible [1 2 2 3].
		/// 
		/// Returns dictionary with assigned ranks for each key from the input [dict].
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dict"></param>
		/// <returns></returns>
		public static Dictionary<T, int> ComputeRanks<T> (Dictionary<T, float> dict)
		{
			var sDict = SortByValue(dict);

			int size = dict.Count;
			var vals = new float[size];
			sDict.Values.CopyTo(vals, 0);

			// fill array of ranks.
			var ranks = new int[size];
			int rank = 1;
			for (int i = 0; i < size-1; i++)
			{
				ranks[i] = rank;
				if (vals[i] != vals[i + 1])
				{	// the next value is different from the current.
					rank++;
				}
			}
			ranks[size - 1] = rank;	// fill-in rank value for the last element.

			var res = new Dictionary<T, int>();
			var count = 0;
			foreach (var key in sDict.Keys)
			{
				res.Add(key, ranks[count]);
				count++;
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Multiplies values in the given dictionary by the value specified.
		/// </summary>
		/// <typeparam name="T">Dictionary key type.</typeparam>
		/// <param name="dict">Input dictionary.</param>
		/// <param name="mul">Multiplier.</param>
		/// <returns>Resulting dictionary.</returns>
		public static Dictionary<T, int> MulValues<T> (Dictionary<T, int> dict, float mul)
		{
			var values = new int[dict.Values.Count];
			dict.Values.CopyTo(values, 0);

			for (var i = 0; i < values.Length; i++)
			{
				values[i] = (int)(values[i]*mul + 0.5f);
			}

			var res = new Dictionary<T, int>();
			var count = 0;
			foreach (var pair in dict)
			{
				res.Add(pair.Key, values[count]);
				++count;
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Multiplies values in the given dictionary by the value specified.
		/// </summary>
		/// <typeparam name="T">Dictionary key type.</typeparam>
		/// <param name="dict">Input dictionary.</param>
		/// <param name="mul">Multiplier.</param>
		/// <returns>Resulting dictionary.</returns>
		public static Dictionary<T, float> MulValues<T>(Dictionary<T, float> dict, float mul)
		{
			var values = new float[dict.Values.Count];
			dict.Values.CopyTo(values, 0);

			for (var i = 0; i < values.Length; i++)
			{
				values[i] = values[i] * mul;
			}

			var res = new Dictionary<T, float>();
			var count = 0;
			foreach (var pair in dict)
			{
				res.Add(pair.Key, values[count]);
				++count;
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Sums given dictionaries by values using keys as indices. The result is written into input dictionary.
		/// </summary>
		/// <param name="v">Input dictionary.</param>
		/// <param name="arg">Summand dictionary.</param>
		public static void Accumulate<T>(ref Dictionary<T, int> v, Dictionary<T, int> arg)
		{
			foreach (var pair in arg)
			{
				if (v.ContainsKey(pair.Key))
				{
					v[pair.Key] += pair.Value;
				}
				else
				{
					v.Add(pair.Key, pair.Value);
				}
			}
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Substracts [arg] dictionary from [v] by values using keys as indices. The result is written into input dictionary.
		/// </summary>
		/// <param name="v">Input dictionary.</param>
		/// <param name="arg">Substracted dictionary.</param>
		public static void Deaccumulate<T>(ref Dictionary<T, int> v, Dictionary<T, int> arg)
		{
			foreach (var pair in arg)
			{
				if (v.ContainsKey(pair.Key))
				{
					v[pair.Key] -= pair.Value;
				}
			}
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Finds min values in arrays in entries and returns dictionary with mins.
		/// </summary>
		/// <param name="v">Dictionary for which the entries are processed.</param>
		public static Dictionary<T, int> Min<T>(Dictionary<T, List<int>> v)
		{
			var res = new Dictionary<T, int>();
			foreach (var pair in v)
			{
				res.Add(pair.Key, VectorMath.Min(pair.Value.ToArray()));
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Finds min values in arrays in entries and returns dictionary with mins.
		/// </summary>
		/// <param name="v">Dictionary for which the entries are processed.</param>
		public static Dictionary<T, float> Min<T>(Dictionary<T, List<float>> v)
		{
			var res = new Dictionary<T, float>();
			foreach (var pair in v)
			{
				res.Add(pair.Key, VectorMath.Min(pair.Value.ToArray()));
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Finds max values in arrays in entries and returns dictionary with maxs.
		/// </summary>
		/// <param name="v">Dictionary for which the entries are processed.</param>
		public static Dictionary<T, int> Max<T>(Dictionary<T, List<int>> v)
		{
			var res = new Dictionary<T, int>();
			foreach (var pair in v)
			{
				res.Add(pair.Key, VectorMath.Max(pair.Value.ToArray()));
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Finds max values in arrays in entries and returns dictionary with maxs.
		/// </summary>
		/// <param name="v">Dictionary for which the entries are processed.</param>
		public static Dictionary<T, float> Max<T>(Dictionary<T, List<float>> v)
		{
			var res = new Dictionary<T, float>();
			foreach (var pair in v)
			{
				res.Add(pair.Key, VectorMath.Max(pair.Value.ToArray()));
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Finds mean values in arrays in entries and returns dictionary with means.
		/// </summary>
		/// <param name="v">Dictionary for which the entries are processed.</param>
		public static Dictionary<T, float> Mean<T>(Dictionary<T, List<int>> v)
		{
			var res = new Dictionary<T, float>();
			foreach (var pair in v)
			{
				res.Add(pair.Key, VectorMath.Mean(pair.Value.ToArray()));
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Finds mean values in arrays in entries and returns dictionary with means.
		/// </summary>
		/// <param name="v">Dictionary for which the entries are processed.</param>
		public static Dictionary<T, float> Mean<T>(Dictionary<T, List<float>> v)
		{
			var res = new Dictionary<T, float>();
			foreach (var pair in v)
			{
				res.Add(pair.Key, VectorMath.Mean(pair.Value.ToArray()));
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Finds mean values in arrays in entries and returns dictionary with means.
		/// </summary>
		/// <param name="v">Dictionary for which the entries are processed.</param>
		public static Dictionary<T, float> Mean<T>(Dictionary<T, int[]> v)
		{
			var res = new Dictionary<T, float>();
			foreach (var pair in v)
			{
				res.Add(pair.Key, VectorMath.Mean(pair.Value));
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Finds max values in arrays in entries and returns dictionary with maxs.
		/// </summary>
		/// <param name="v">Dictionary for which the entries are processed.</param>
		public static Dictionary<T, float> StdDev<T>(Dictionary<T, List<int>> v)
		{
			var res = new Dictionary<T, float>();
			foreach (var pair in v)
			{
				var var = VectorMath.Variance(pair.Value.ToArray());
				res.Add(pair.Key, (float)Math.Sqrt(var));
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Finds max values in arrays in entries and returns dictionary with maxs.
		/// </summary>
		/// <param name="v">Dictionary for which the entries are processed.</param>
		public static Dictionary<T, float> StdDev<T>(Dictionary<T, List<float>> v)
		{
			var res = new Dictionary<T, float>();
			foreach (var pair in v)
			{
				var var = VectorMath.Variance(pair.Value.ToArray());
				res.Add(pair.Key, (float)Math.Sqrt(var));
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Sums entries in arrays and returns dictionary with sums.
		/// </summary>
		/// <param name="v">Dictionary for which the entries are summed.</param>
		public static Dictionary<T, int> Sum<T>(Dictionary<T, List<int>> v)
		{
			var res = new Dictionary<T, int>();
			foreach (var pair in v)
			{
				res.Add(pair.Key, VectorMath.Sum(pair.Value));
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Appends given dictionary to the existing values using entries' keys.
		/// </summary>
		/// <param name="v">Dictionary to append to.</param>
		/// <param name="arg">Appended dictionary.</param>
		public static void Append<T, T1>(ref Dictionary<T, List<T1>> v, Dictionary<T, T1> arg)
		{
			foreach (var pair in arg)
			{
				if (!v.ContainsKey(pair.Key))
				{
					v.Add(pair.Key, new List<T1>());
				}
				v[pair.Key].Add(pair.Value);
			}
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Sums given dictionaries by values using keys as indices. The result is written into input dictionary.
		/// </summary>
		/// <param name="v">Input dictionary.</param>
		/// <param name="arg">Summand dictionary.</param>
		public static void Accumulate<T>(ref Dictionary<T, float> v, Dictionary<T, float> arg)
		{
			foreach (var pair in arg)
			{
				if (v.ContainsKey(pair.Key))
				{
					v[pair.Key] += pair.Value;
				}
				else
				{
					v.Add(pair.Key, pair.Value);
				}
			}
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Substracts [arg] dictionary from [v] by values using keys as indices. The result is written into input dictionary.
		/// </summary>
		/// <param name="v">Input dictionary.</param>
		/// <param name="arg">Substracted dictionary.</param>
		public static void Deaccumulate<T>(ref Dictionary<T, float> v, Dictionary<T, float> arg)
		{
			foreach (var pair in arg)
			{
				if (v.ContainsKey(pair.Key))
				{
					v[pair.Key] -= pair.Value;
				}
			}
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Converts given dictionary of counters into the dictionary of probabilities.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dict"></param>
		/// <returns></returns>
		public static Dictionary<T, float> ConvertToProbabilities<T>(Dictionary<T, int> dict)
		{
			var sum = SumCounts(dict);
			var sum_1 = 1.0f/sum;
			var res = new Dictionary<T, float>();
			foreach (var pair in dict)
			{
				res.Add(pair.Key, pair.Value * sum_1);
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates inverse dictionary for the given one. The result contains values: 1 / dict[key]
		///  </summary>
		/// <typeparam name="T">Key type.</typeparam>
		/// <param name="dict">Input dictionary.</param>
		/// <returns>Resulting dictionary with inverse values.</returns>
		public static Dictionary<T, float> Inverse<T>(Dictionary<T, int> dict)
		{
			var res = new Dictionary<T, float> ();
			foreach(var pair in dict)
			{
				if (pair.Value != 0) res.Add(pair.Key, 1.0f / pair.Value);
				else res.Add(pair.Key, 0f);
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Sorts given dictionary by values.
		/// </summary>
		/// <typeparam name="T">Key type.</typeparam>
		/// <param name="dict">Input dictionary.</param>
		/// <returns>Sorted dictionary.</returns>
		public static Dictionary<T, int> SortByValue<T>(Dictionary<T, int> dict)
		{
			return InsertionSortByValue(dict);
			//return MergeSortByValue(dict, 0, dict.Count-1);
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Performs insertion sorting in descending order by value over the given dictionary. The implementation is based upon Cormen's book.
		/// </summary>
		/// <typeparam name="T">Key type.</typeparam>
		/// <param name="dict">Input dictionary.</param>
		/// <returns>Sorted dictionary.</returns>
		public static Dictionary<T, int> InsertionSortByValue<T>(Dictionary<T, int> dict)
		{
			//var inVals = new float[dict.Count];
			//var inKeys = new T[dict.Count];
			//dict.Values.CopyTo(inVals, 0);
			//dict.Keys.CopyTo(inKeys, 0);
			var vals = new List<int>();
			var keys = new List<T>();
			foreach (var pair in dict)
			{
				var idx = VectorMath.FirstGreaterIndex(vals, pair.Value);
				if (idx >= 0)
				{
					vals.Insert(idx, pair.Value);
					keys.Insert(idx, pair.Key);
				}
				else
				{
					vals.Add(pair.Value);
					keys.Add(pair.Key);
				}
			}

			var res = new Dictionary<T, int>();
			for (int i = 0; i < dict.Count; i++)
			{
				res.Add(keys[i], vals[i]);
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Sorts given dictionary by values.
		/// </summary>
		/// <typeparam name="T">Key type.</typeparam>
		/// <param name="dict">Input dictionary.</param>
		/// <returns>Sorted dictionary.</returns>
		public static Dictionary<T, float> SortByValue<T> (Dictionary<T, float> dict)
		{
			return InsertionSortByValue(dict);
			//return MergeSortByValue(dict, 0, dict.Count-1);
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Reverses order in which elements are arranged inside the given dictionary.
		/// </summary>
		/// <typeparam name="T1">Key type.</typeparam>
		/// <typeparam name="T2">Value type.</typeparam>
		/// <param name="dict">Input dictionary.</param>
		/// <returns>Sorted dictionary.</returns>
		public static Dictionary<T1, T2> Reverse<T1, T2>(Dictionary<T1, T2> dict)
		{
			var res = new Dictionary<T1, T2>();

			var rkeys = new T1[dict.Count];
			dict.Keys.CopyTo(rkeys, 0);
			rkeys = VectorMath.Reverse(rkeys);

			foreach (var rkey in rkeys)
			{
				res.Add(rkey, dict[rkey]);
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Performs insertion sorting in descending order by value over the given dictionary. The implementation is based upon Cormen's book.
		/// </summary>
		/// <typeparam name="T">Key type.</typeparam>
		/// <param name="dict">Input dictionary.</param>
		/// <returns>Sorted dictionary.</returns>
		public static Dictionary<T, float> InsertionSortByValue<T>(Dictionary<T, float> dict)
		{
			//var inVals = new float[dict.Count];
			//var inKeys = new T[dict.Count];
			//dict.Values.CopyTo(inVals, 0);
			//dict.Keys.CopyTo(inKeys, 0);
			var vals = new List<float>();
			var keys = new List<T>();
			foreach (var pair in dict)
			{
				var idx = VectorMath.FirstGreaterIndex(vals, pair.Value);
				if (idx >= 0)
				{
					vals.Insert(idx, pair.Value);
					keys.Insert(idx, pair.Key);
				}
				else
				{
					vals.Add(pair.Value);
					keys.Add(pair.Key);
				}
			}

			var res = new Dictionary<T, float>();
			for (int i = 0; i < dict.Count; i++)
			{
				res.Add(keys[i], vals[i]);
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Performs merge sorting in descending order by value over the given dictionary. The implementation is based upon Cormen's book.
		/// </summary>
		/// <typeparam name="T">Key type.</typeparam>
		/// <param name="dict">Input dictionary.</param>
		/// <param name="p">Sorting interval start.</param>
		/// <param name="r">Sorting interval end.</param>
		/// <returns>Sorted dictionary.</returns>
		public static Dictionary<T, float> MergeSortByValue<T> (Dictionary<T, float> dict, int p, int r)
		{
			var res = new Dictionary<T, float>();

			//
			// copy input dictionary.
			foreach (var pair in dict)
			{
				res.Add(pair.Key, pair.Value);
			}
			
			//
			// perform sorting.
			if (p < r)
			{
				var q = (p + r) / 2;
				MergeSortByValue(res, p, q);
				MergeSortByValue(res, q + 1, r);
				MergeByValue(res, p, q, r);
			}

			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Merges partially sorted continuous intervals in the given dictionary in a descending orders.
		/// First interval locates at indices [p; q] and the second at [q+1; r].
		/// The result is written into the input dictionary.
		/// </summary>
		/// <typeparam name="T">Key type.</typeparam>
		/// <param name="dict">Input dictionary.</param>
		/// <param name="p">Sorting interval start.</param>
		/// <param name="q">Sorting interval middle.</param>
		/// <param name="r">Sorting interval end.</param>
		public static void MergeByValue<T>(Dictionary<T, float> dict, int p, int q, int r)
		{
			//
			// 'split' input dictionary.
			var keys = new T[dict.Count];
			var values = new float[dict.Count];
			dict.Keys.CopyTo(keys, 0);
			dict.Values.CopyTo(values, 0);

			var n1 = q - p + 1;
			var n2 = r - q;

			//
			// prepare temporal arrays [left] and [right].
			var left = new float[n1 + 1];
			var leftKeys = new T[n1 + 1];
			for (var i = 0; i < n1; ++i) 
			{
				left[i] = values [p+i];
				leftKeys[i] = keys[p + i];
			}
			left[n1] = float.PositiveInfinity;	// a small trick.

			var right = new float[n2+1];
			var rightKeys = new T[n2+1];
			for (var i = 0; i < n2 && (q + i + 1) < values.Length; ++i)
			{
				right[i] = values[q + i + 1];
				rightKeys[i] = keys[q + i + 1];
			}
			right[n2] = float.PositiveInfinity;	// just another small trick.

			//
			// make merging.
			//	for (int i = p, il = 0, ir = 0; i <= r; ++i)
			for (int i = p, il = 0, ir = 0; i <= r && i < keys.Length; ++i)
			{
				if (left[il] < right[ir])
				{
					keys[i] = leftKeys[il];
					values[i] = left[il];
					++il;
				}
				else
				{
					keys[i] = rightKeys[ir];
					values[i] = right[ir];
					++ir;
				}
			}

			//
			// write updated [keys] and [values].
			dict.Clear();
			for (int i = 0; i < keys.Length; i++)
			{
				dict.Add(keys[i], values[i]);
			}

			//
			// dispose memory.
			Array.Clear(left, 0, left.Length);
			Array.Clear(right, 0, right.Length);
			Array.Clear(leftKeys, 0, leftKeys.Length);
			Array.Clear(rightKeys, 0, rightKeys.Length);
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Inserts given object in the dictionary in descending order for values.
		/// </summary>
		/// <typeparam name="T">Key type.</typeparam>
		/// <param name="dict">Input dictionary.</param>
		/// <param name="key">Kay.</param>
		/// <param name="value">Value.</param>
		/// <returns>Modified dictionary.</returns>
		public static Dictionary<T, float> InsertDescendingValue<T> (Dictionary<T, float> dict, T key, float value)
		{
			var res = new Dictionary<T, float>();
			var objAdded = false;
			foreach (var pair in dict)
			{
				if (!objAdded && pair.Value < value)
				{
					res.Add(key, value);
					objAdded = true;
				}
				res.Add(pair.Key, pair.Value);
			}
			if (!objAdded)
			{
				res.Add(key, value);
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Inserts given object in the dictionary in descending order for values.
		/// </summary>
		/// <typeparam name="T">Key type.</typeparam>
		/// <param name="dict">Input dictionary.</param>
		/// <param name="key">Kay.</param>
		/// <param name="value">Value.</param>
		/// <returns>Modified dictionary.</returns>
		public static Dictionary<T, int> InsertDescendingValue<T>(Dictionary<T, int> dict, T key, int value)
		{
			var res = new Dictionary<T, int>();
			var objAdded = false;
			foreach (var pair in dict)
			{
				if (!objAdded && pair.Value < value)
				{
					res.Add(key, value);
					objAdded = true;
				}
				res.Add(pair.Key, pair.Value);
			}
			if (!objAdded)
			{
				res.Add(key, value);
			}
			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Copies entries from the [src] dictionary into the [dest] using specified array of keys.
		/// </summary>
		/// <typeparam name="T1">Key type.</typeparam>
		/// <typeparam name="T2">Value type.</typeparam>
		/// <param name="src">Source dictionary.</param>
		/// <param name="dest">Destination dictionary.</param>
		/// <param name="keys">List of keys for the elements to copy.</param>
		public static void PartialCopy<T1, T2>(Dictionary<T1, T2> src, ref Dictionary<T1, T2> dest, List<T1> keys)
		{
			foreach (var key in keys)
			{
				T2 srcValue;
				if (src.TryGetValue(key, out srcValue))
				{
					if (dest.ContainsKey(key))
					{
						dest[key] = srcValue;
					}
					else
					{
						dest.Add(key, srcValue);
					}
				}
			}
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Calculates Euclidian distance between given dictionaries' entries.
		/// 
		/// Note: If dictionaries has different set of keys then the distance is infinite.
		/// </summary>
		/// <typeparam name="T1">Key type.</typeparam>
		/// <param name="dict1">1st dictionary.</param>
		/// <param name="dict2">2nd dictionary.</param>
		/// <returns>Euclidian distance value.</returns>
		public static float DistanceEuclidian<T1>(Dictionary<T1, float> dict1, Dictionary<T1, float> dict2)
		{
			if (dict1.Count != dict2.Count) return float.PositiveInfinity;

			var res = 0f;
			foreach (var pair in dict1)
			{
				float val2;
				if (dict2.TryGetValue(pair.Key, out val2))
				{
					res += (pair.Value - val2)*(pair.Value - val2);
				}
				else
				{
					return float.PositiveInfinity;
				}
			}

			return (float)Math.Sqrt(res);
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Calculates Euclidian distance between given dictionaries' entries even if they have different set of keys
		/// so that only the distance between entries with the same keys counts.
		/// </summary>
		/// <typeparam name="T1">Key type.</typeparam>
		/// <param name="dict1">1st dictionary.</param>
		/// <param name="dict2">2nd dictionary.</param>
		/// <returns>Euclidian distance value.</returns>
		public static float DistanceEuclidianNoKeyMatch<T1>(Dictionary<T1, float> dict1, Dictionary<T1, float> dict2)
		{
			var res = 0f;
			foreach (var pair in dict1)
			{
				float val2;
				if (dict2.TryGetValue(pair.Key, out val2))
				{
					res += (pair.Value - val2) * (pair.Value - val2);
				}
			}

			return (float)Math.Sqrt(res);
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Looks for the key of the first minimal value from dictionary.
		/// </summary>
		/// <typeparam name="T">Key type.</typeparam>
		/// <param name="dict">Input dictionary.</param>
		/// <returns>Key of the maximal element.</returns>
		public static T FindMinValueKey<T>(Dictionary<T, int> dict)
		{
			T key = dict.Keys.GetEnumerator().Current;
			var min = int.MaxValue;
			foreach (var pair in dict)
			{
				if (pair.Value < min)
				{
					min = pair.Value;
					key = pair.Key;
				}
			}
			return key;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Looks for the key of the first minimal value from dictionary.
		/// </summary>
		/// <typeparam name="T">Key type.</typeparam>
		/// <param name="dict">Input dictionary.</param>
		/// <returns>Key of the maximal element.</returns>
		public static T FindMinValueKey<T>(Dictionary<T, float> dict)
		{
			T key = dict.Keys.GetEnumerator().Current;
			var min = float.MaxValue;
			foreach (var pair in dict)
			{
				if (pair.Value < min)
				{
					min = pair.Value;
					key = pair.Key;
				}
			}
			return key;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Looks for the key of the first minimal value from dictionary.
		/// </summary>
		/// <typeparam name="T">Key type.</typeparam>
		/// <param name="dict">Input dictionary.</param>
		/// <param name="lBound">Lower bound, above which the requested value should be.</param>
		/// <returns>Key of the maximal element.</returns>
		public static T FindMinValueKey<T>(Dictionary<T, float> dict, float lBound)
		{
			T key = dict.Keys.GetEnumerator().Current;
			var min = float.MaxValue;
			foreach (var pair in dict)
			{
				if (pair.Value < min && pair.Value > lBound)
				{
					min = pair.Value;
					key = pair.Key;
				}
			}
			return key;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Looks for the key of the first maximal value from dictionary.
		/// </summary>
		/// <typeparam name="T">Key type.</typeparam>
		/// <param name="dict">Input dictionary.</param>
		/// <returns>Key of the maximal element.</returns>
		public static T FindMaxValueKey<T>(Dictionary<T, float> dict)
		{
			T key = dict.Keys.GetEnumerator().Current;
			var max = float.MinValue;
			foreach (var pair in dict)
			{
				if (pair.Value > max)
				{
					max = pair.Value;
					key = pair.Key;
				}
			}
			return key;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Looks for the key of the first maximal value from dictionary.
		/// </summary>
		/// <typeparam name="T">Key type.</typeparam>
		/// <param name="dict">Input dictionary.</param>
		/// <returns>Key of the maximal element.</returns>
		public static T FindMaxValueKey<T>(Dictionary<T, int> dict)
		{
			T key = dict.Keys.GetEnumerator().Current;
			var max = int.MinValue;
			foreach (var pair in dict)
			{
				if (pair.Value > max)
				{
					max = pair.Value;
					key = pair.Key;
				}
			}
			return key;
		}
		#endregion

		#region - Methods for [Node2D]. -
		/// <summary>
		/// [atomic]
		/// 
		/// Method to calculate distances between all nodes from the given 2D array and a
		/// given node. The distances are exponentially scaled using expression:
		/// 
		/// 	distance = \exp{-factor*distance}
		/// 
		/// where [factor] is a constant argument.
		/// </summary>
		/// <param name="nodes">2D array of nodes.</param>
		/// <param name="argNode">Node from which the distance is to be calculated.</param>
		/// <param name="factor">Factor for exponential scaling.</param>
		/// <returns>2D array of distances.</returns>
		public static float[,] CalculateNeighborhoodDistancesExp(List<List<Node2D>> nodes, Node2D argNode, float factor)
		{
			int height = nodes.Count, width = nodes[0].Count;
			var dists = new float[height,width];
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					if (nodes[i][j] == argNode){continue;}

					float dist = DistanceSqr(nodes[i][j].Coord, argNode.Coord);
					dists[i,j] = (float)Math.Exp(-dist * factor);
				}
			}
			return dists;
		} 

		/// <summary>
		/// [atomic]
		/// 
		/// Calculates 2D array of distances each corresponding to the distance
		///		between node with the same coordinates from the given 2D array and the given vector.
		/// </summary>
		/// <param name="nodes">2D array of nodes.</param>
		/// <param name="vector">Vector to calculate distances from.</param>
		/// <returns>2D array of distances.</returns>
        public static float[,] CalculateDistances (List<List<Node2D>> nodes, float[] vector)
        {
            int height = nodes.Count, width = nodes[0].Count;
            var dist = new float[height, width];

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    dist[i,j] = VectorMath.EuclidianDistance(nodes[i][j].W.ToArray(), vector);
                }
            }

            return dist;
        }
	    #endregion

		#region - Methods for [Size]. -
		/// <summary>
		/// [atomic]
		/// 
		/// Returns new size, by reducing the given one so that the maximal
		/// dimension doesn't exceed [maxDim].
		/// If both input dimensions are not large than [maxDim] then no change is made.
		/// </summary>
		/// <param name="src">Input size.</param>
		/// <param name="maxDim">Max dimension.</param>
		/// <returns>New size.</returns>
		public static Size ResizeReduceOnly (Size src, int maxDim)
		{
			Size res;
			if (src.Width > maxDim || src.Height > maxDim)
			{	// reduce image size.
				float factor = src.Width > src.Height
								? src.Width / (float)maxDim
								: src.Height / (float)maxDim;
				var width = (int)(src.Width / factor);
				var height = (int)(src.Height / factor);
				res = new Size(width, height);
			}
			else
			{	// do not do anything.
				res = src;
			}

			return res;
		}
		#endregion

		#region - Methods for [Stats]. -
		/// <summary>
		/// [atomic]
		/// 
		/// Adds additional [data] as column the current list of [Stats] so that single value from data[i] is appended to the stats[i].
		/// </summary>
		/// <param name="stats">List of stats.</param>
		/// <param name="data">Data column to add.</param>
		/// <param name="name">Added data name.</param>
		public static void AppendStats (ref List<Stats> stats, List<float> data, string name)
		{
			int size = stats.Count;
			for (int i = 0; i < size; i++)
			{
				stats[i].AppendData(data[i], name);
			}
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Creates [Stats] object using given data and data header.
		/// Note that the function is hardcoded so changing the standard data order or header is dangerous.
		/// The entries should be in the following order:
		///		min, max, mean, variance, median, total, additional data
		/// </summary>
		/// <param name="data">Source data.</param>
		/// <param name="header">Header.</param>
		/// <returns>Stats.</returns>
		public static Stats CreateStats (float[] data, string header)
		{
			int count = 0;
			var res = new Stats();
			res.Header = header;
			res.Min = data[count];	// warning: hardcode!
			res.Max = data[++count];
			res.Mean = data[++count];
			res.Variance = data[++count];
			res.Median = data[++count];
			res.Total = data[++count];

			for (int i = count + 1; i < data.Length; i++)
			{
				res.Data.Add(data[i]);
			}
			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Averages given list of stats collections.
		/// Note that all [Stats] objects should contain the same number of entries which should be ordered in the same way.
		/// </summary>
		/// <param name="stats">List of stats collections.</param>
		/// <returns>Averaged stats.</returns>
		public static List<Stats> Average (List<List<Stats>> stats)
		{
			if (stats.Count == 0) return new List<Stats>();

			var header = stats[0][0].Header;
			var res = new List<Stats>();
			for (int i = 0; i < stats[0].Count; i++)
			{
				var m = new List<float[]>();
				for (int j = 0; j < stats.Count; j++)
				{
					var tempV = VectorMath.CreateFromStats(stats[j][i]);
					m.Add(tempV);
					//if (VectorMath.ContainsNonZeros(tempV))
					//{
					//    m.Add(tempV);
					//}
				}
				var meanV = VectorMath.MeanVector(m);

				var temp = CreateStats (meanV, header);
				res.Add(temp);
			}

			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Averages given list of stats collections.
		/// This is a more universal immplementation which can deal with entries of [Stats] of different sizes.
		/// </summary>
		/// <param name="stats">List of stats collections.</param>
		/// <returns>Averaged stats.</returns>
		public static List<Stats> AverageUneq(List<List<Stats>> stats)
		{
			if (stats.Count == 0) return new List<Stats>();

			// find max length for [Stats] entries.
			var maxLen = stats[0].Count;
			for (int i = 1; i < stats.Count; i++)
			{
				if (maxLen < stats[i].Count) {maxLen = stats[i].Count;}
			}

			var header = stats[0][0].Header;
			var res = new List<Stats>();
			for (int i = 0; i < maxLen; i++)
			{
				var m = new List<float[]>();
				for (int j = 0; j < stats.Count; j++)
				{
					if (stats[j].Count <= i) continue;
					
					var tempV = VectorMath.CreateFromStats(stats[j][i]);
					m.Add(tempV);
				}
				var meanV = VectorMath.MeanVector(m);

				var temp = CreateStats(meanV, header);
				res.Add(temp);
			}

			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Fills gaps in Data sections with zeroes. New values are appended to Data elements.
		/// </summary>
		/// <param name="stats"></param>
		public static void FillGapsZero (List<Stats> stats)
		{
			// find max number of extra data entries.
			var maxDataCount = 0;
			foreach (var stat in stats)
			{
				if (maxDataCount < stat.Data.Count)
				{
					maxDataCount = stat.Data.Count;
				}
			}

			// fill gaps by appending zeroes.
			foreach (var stat in stats)
			{
				while (stat.Data.Count < maxDataCount)
				{
					stat.Data.Add(0);
				}
			}
		}
		#endregion
	}
}