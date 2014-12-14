using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MentalAlchemy.Atoms
{
	public class StringUtils
	{
		#region - Operations. -
		/// <summary>
		/// [molecule]
		/// 
		/// Concatenates lines from the given array into a single line in their appearance order.
		/// </summary>
		/// <param name="lines">Lines to concatenate.</param>
		/// <returns>Concatenation of input lines.</returns>
		public static string Concat(string[] lines)
		{
			var res = "";
			foreach (var line in lines)
			{
				res += line;
			}
			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Concatenates lines from the given array into a single line in their appearance order.
		/// Separating sequence is inserted between each pair of concatenated strings.
		/// </summary>
		/// <param name="lines">Lines to concatenate.</param>
		/// <param name="sep">Separating sequence.</param>
		/// <returns>Concatenation of input lines.</returns>
		public static string Concat(string[] lines, string sep)
		{
			var res = lines[0];
			for (int i = 1; i < lines.Length; i++)
			{
				res += sep + lines[i];
			}
			return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Splitts given string leaving separators in the result.
		/// </summary>
		/// <param name="line">Input line.</param>
		/// <param name="seps">array of separator chars.</param>
		/// <returns>True if [lines] contain [str], and False otherwise.</returns>
		public static string[] SplitWithSeparators(string line, char[] seps)
		{
			var res = new List<string>();

			var count = 0;
			do
			{
				var idx = line.IndexOfAny(seps, count);
				if (idx >= 0)
				{
					if (idx != count)
					{
						res.Add(line.Substring(count, idx - count));
					}
					res.Add(line[idx].ToString());	// add separator as well.
					count = idx + 1;
				}
				else
				{
					res.Add(line.Substring(count));
					break;
				}
			} while (count < line.Length);
			return res.ToArray();
		} 

		/// <summary>
		/// [atomic]
		/// 
		/// Filters out empty strings from the given array and returns filtered array.
		/// </summary>
		/// <param name="strs"></param>
		/// <returns></returns>
		public static string[] RemoveEmptyElements (string[] strs)
		{
			var res = new List<string>();

			foreach (var s in strs)
			{
				if (s.Length != 0)
				{
					res.Add(s);
				}
			}

			return res.ToArray();
		}
		#endregion

		#region - Checking. -
		/// <summary>
		/// [atomic]
		/// 
		/// Defines whether given array of lines contains specified string.
		/// </summary>
		/// <param name="lines">Input array of lines.</param>
		/// <param name="str">Requested string.</param>
		/// <param name="ignoreCase">Defines whether comparison is case sensitive or not.</param>
		/// <returns>True if [lines] contain [str], and False otherwise.</returns>
		public static bool Contains(string[] lines, string str, bool ignoreCase)
		{
			foreach (var line in lines)
			{
				if (string.Compare(line, str, ignoreCase) == 0) { return true; }
			}
			return false;
		} 
		#endregion

		#region - Search. -
		/// <summary>
		/// [atomic]
		/// 
		/// Returns indices of the substring [trg] entries, which are met inside the source string.
		/// If no entry is met, then null is returned.
		/// </summary>
		/// <param name="src"></param>
		/// <param name="trg"></param>
		/// <returns></returns>
		public static int[] GetIndices (string src, string trg)
		{
			// create RegEx for search.
			var re = new Regex(trg);
			//var ms = re.Matches(src);

			var idx = new List<int>();
			int start = 0, maxStart = src.Length - trg.Length;
			do
			{
				var m = re.Match(src, start, trg.Length);
				if (m.Success) {idx.Add(m.Index);}
				start++;
			} while (start <= maxStart);

			//if (ms.Count == 0) return null;
			if (idx.Count == 0) return null;
			return idx.ToArray();

			//var res = new int[idx.Count];
			//for (int i = 0; i < res.Length; i++)
			//{
			//    res[i] = ms[i].Index;
			//}

			//return res;
		}

		/// <summary>
		/// [atomic]
		/// 
		/// Looks for repetitive substrings of the fixed length inside the given string.
		/// 
		/// </summary>
		/// <param name="str"></param>
		/// <param name="size"></param>
		/// <returns></returns>
		public static Dictionary<string, int[]> FindRepSubstrings (string str, int size)
		{
			var tempRes = new Dictionary<string, int[]>();
			var counts = new Dictionary<string, int>();
			var hits = new bool[str.Length];
			for (int i = 0; i < str.Length - size; i++)
			{
				if (hits[i] == true) continue;	// if the string has already been met.

				var substr = str.Substring(i, size);
				var inds = GetIndices(str, substr);

				if (inds == null) continue;

				// mark indices as hits.
				foreach (var idx in inds)
				{
					hits[idx] = true;
				}

				// save temporal results and counts
				tempRes.Add(substr, inds);
				counts = StructMath.InsertDescendingValue(counts, substr, inds.Length);
			}

			// sort resulting entries by their usage.
			var res = new Dictionary<string, int[]>();
			foreach (var pair in counts)
			{
				res.Add(pair.Key, tempRes[pair.Key]);
			}

			return res;
		}
		#endregion
	}
}
