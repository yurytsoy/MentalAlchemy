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

using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using MentalAlchemy.Atoms;

namespace MentalAlchemy.Molecules
{
	/// <summary>
	/// Utils to work with files.
	/// </summary>
	public class FileIO
	{
		#region - FileIO. -
		/// <summary>
		/// [molecule]
		/// 
		/// Returns all lines read from the given file.
		/// </summary>
		/// <param name="filename">Name of the file.</param>
		/// <returns>Array of all lines from the file.</returns>
		public static string[] ReadAllLines(string filename)
		{
			var lines = new List<string>();
			using (var reader = new StreamReader(filename))
			{
				while (!reader.EndOfStream)
				{
					string tempStr = reader.ReadLine();
					lines.Add(tempStr);
				}
			}
			return lines.ToArray();
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Read text from the given file and splits each line using the prescribed separators.
		/// </summary>
		/// <param name="filename">Filename.</param>
		/// <param name="separators">Array of separtors for splitting file lines. If [null] then default separator '\t' is used.</param>
		/// <returns>List of splitted lines.</returns>
		public static List<string[]> ReadColumns(string filename, char[] separators)
		{
			var lines = ReadAllLines(filename);
			var res = new List<string[]>();

			var seps = separators ?? new[] { '\t' };
			foreach (var line in lines)
			{
				if (line.Trim().Length == 0) { continue; }
				string[] cols = line.Split(seps);
				res.Add(cols);
			}

			return res;
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Writes given lines into the specified file separating each column with [TAB].
		/// Each line contains array of strings, which correspond to cells contents.
		/// </summary>
		/// <param name="filename">Filename.</param>
		/// <param name="lines">List of lines.</param>
		/// <param name="header">Optional header which is written before all lines.</param>
		public static void WriteColumns(string filename, List<string[]> lines, string header)
		{
			using (var writer = new StreamWriter(filename))
			{
				WriteColumns(writer, lines, header);
			}
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Writes given lines into the specified output stream separating each column with [TAB].
		/// Each line contains array of strings, which correspond to cells contents.
		/// </summary>
		/// <param name="writer">Output stream.</param>
		/// <param name="lines">List of lines.</param>
		/// <param name="header">Optional header which is written before all lines.</param>
		public static void WriteColumns(TextWriter writer, List<string[]> lines, string header)
		{
			if (header != null) writer.WriteLine(header);

			var separator = '\t';
			foreach (var line in lines)
			{
				if (line.Length == 0)
				{
					writer.WriteLine();
					continue;
				}

				var tempStr = line[0];
				for (int i = 1; i < line.Length; i++)
				{
					tempStr += separator + line[i];
				}
				writer.WriteLine(tempStr);
			}
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Writes given table into the specified file separating each column with [TAB].
		/// Each line contains array of strings, which correspond to cells contents.
		/// </summary>
		/// <param name="filename">Filename.</param>
		/// <param name="lines">Table to write.</param>
		/// <param name="header">Optional header which is written before all lines.</param>
		public static void WriteColumns<T>(string filename, T[,] lines, string header)
		{
			var separator = '\t';
			using (var writer = new StreamWriter(filename))
			{
				WriteColumns(writer, lines, header, separator);
			}
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Writes given table into the specified file separating each column with [TAB].
		/// Each line contains array of strings, which correspond to cells contents.
		/// </summary>
		/// <param name="filename">Filename.</param>
		/// <param name="lines">Table to write.</param>
		/// <param name="header">Optional header which is written before all lines.</param>
		/// <param name="separator">Columns separator.</param>
		public static void WriteColumns<T>(string filename, T[,] lines, string header, char separator)
		{
			using (var writer = new StreamWriter(filename))
			{
				WriteColumns(writer, lines, header, separator);
			}
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Writes given table into the specified file separating each column with [TAB].
		/// Each line contains array of strings, which correspond to cells contents.
		/// </summary>
		/// <param name="writer">Output stream.</param>
		/// <param name="lines">Table to write.</param>
		/// <param name="header">Optional header which is written before all lines.</param>
		/// <param name="separator">Column-separating symbol.</param>
		public static void WriteColumns<T>(TextWriter writer, T[,] lines, string header, char separator)
		{
			if (header != null) writer.WriteLine(header);

			int height = lines.GetLength(0), width = lines.GetLength(1);
			for (int i = 0; i < height; i++)
			{
				string tempStr = lines[i, 0].ToString();
				for (int j = 1; j < width; j++)
				{
					tempStr += separator + lines[i, j].ToString();
				}
				writer.WriteLine(tempStr);
			}
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Writes given list of [Stats] structures into the specified output stream and numerates each entry. Optionally writes stats header.
		/// </summary>
		/// <param name="writer">Output stream.</param>
		/// <param name="stats">List of stats to write.</param>
		/// <param name="writeHeader">Indicates whether stats header, which names the stats and their order should be written.</param>
		public static void WriteStatsNumerate(TextWriter writer, List<Stats> stats, bool writeHeader)
		{
			// [optiona] write header. During writing add the beginning tab to shift header entries due to line numbers.
			if (writeHeader) writer.WriteLine("\t" + stats[0].GetStatsHeader());	// note: all [stats] entries are considered to have the same header.
			for (int i = 0; i < stats.Count; i++)
			{
				string line = string.Format("{0}:\t{1}", i, stats[i].GetStatsString());
				writer.WriteLine(line);
			}
		} 

		/// <summary>
		/// [molecule]
		/// 
		/// Writers all given lines into the specified file.
		/// </summary>
		/// <param name="filename">Output file.</param>
		/// <param name="lines">Lines to write.</param>
		public static void WriteAllLines (string filename, IList<string> lines)
		{
			using (var writer = new StreamWriter(filename))
			{
				WriteAllLines(writer, lines);
			}
		}

		/// <summary>
		/// [molecule]
		/// 
		/// Writers all given lines into the specified stream.
		/// </summary>
		/// <param name="writer">Output stream.</param>
		/// <param name="lines">Lines to write.</param>
		public static void WriteAllLines(TextWriter writer, IList<string> lines)
		{
			foreach (var line in lines)
			{
				writer.WriteLine(line);
			}
		}
		#endregion

		#region - Files comparison. -
		/// <summary>
		/// [molecule]
		/// 
		/// Defines whether two given files are identical or not.
		/// The comparison is perfomed symbol-wise with respect to symbols' case.
		/// </summary>
		/// <param name="file1">Name of the 1st file.</param>
		/// <param name="file2">Name of the 2nd file.</param>
		/// <returns>[true] if files identical and [false] otherwise.</returns>
		public static bool ValidateFilesIdentity(string file1, string file2)
		{
			var lines1 = ReadAllLines(file1);
			var lines2 = ReadAllLines(file2);

			if (lines1.Length != lines2.Length) return false;

			int size = lines1.Length;
			for (int i = 0; i < size; i++)
			{
				if (string.Compare(lines1[i], lines2[i]) != 0)
				{
					return false;
				}
			}

			return true;
		} 
		#endregion

		#region - Paths processing. -
		/// <summary>
		/// [molecule]
		/// 
		/// Returns only directory and filename from the given path. If the path doesn't contain such information then the source string is returned.
		/// </summary>
		/// <param name="path">Input path.</param>
		/// <returns>Relative path.</returns>
		public static string GetShortRelativePath (string path)
		{
			var last = path.LastIndexOf(Path.DirectorySeparatorChar);
			if (last < 0) return path;

			var last2 = path.LastIndexOf(Path.DirectorySeparatorChar, last-1);
			if (last2 < 0) return path;
			
			return path.Substring(last2+1);
		}
		#endregion

		#region - Serialization. -
		/// <summary>
		/// [molecule]
		/// 
		/// Performs serialization of the given object (should have [Serializable] attribute!).
		/// </summary>
		/// <param name="filename">Name of the resulting file.</param>
		/// <param name="obj">Object to serialize.</param>
		public static void Serialize (string filename, object obj)
		{
			IFormatter formatter = new BinaryFormatter();
			Stream stream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None);
			formatter.Serialize(stream, obj);
			stream.Close();
		}
		#endregion
	}
}
