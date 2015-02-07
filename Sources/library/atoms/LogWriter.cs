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
using System.IO;

namespace MentalAlchemy.Atoms
{
	/// <summary>
	/// [atomic]
	/// 
	/// A singleton class for logging. Use Instance method to create/gain access to the LogWriter object.
	/// </summary>
	public class LogWriter
	{
		#region - Variables. -
		private const string DEFAULT_FILENAME = "program.log";
		private static LogWriter logWriter = null;
		private static StreamWriter writer = null;
		#endregion

		#region - Properties. -
		/// <summary>
		/// Indicates whether log writer writes information ([true]) or not ([false])
		/// </summary>
		public bool Enabled { get; set; } 
		#endregion

		#region - Creation & Destruction. -
		/// <summary>
		/// Default constructor.
		/// </summary>
		protected LogWriter() { }
		/// <summary>
		/// Creates log writer and opens the provided file for writing.
		/// </summary>
		/// <param name="filename">Filename.</param>
		protected LogWriter(string filename)
		{
			writer = new StreamWriter(filename);
			Enabled = true;
		}
		/// <summary>
		/// Releases unmanaged resources and performs other cleanup operations before the
		/// <see cref="MentalAlchemy.Atoms.LogWriter"/> is reclaimed by garbage collection.
		/// </summary>
		~LogWriter(){}

		/// <summary>
		/// Creates LogWriter object if it's not yet created or returns existing instance.
		/// </summary>
		/// <returns>Active LogWriter object.</returns>
		public static LogWriter Instance()
		{
			if (logWriter == null)
			{
				logWriter = new LogWriter(DEFAULT_FILENAME);
			}
			return logWriter;
		}
		#endregion

		#region - Public methods. -
		/// <summary>
		/// Writes single line into the log-file. Places carriage return at the end of line.
		/// Writes current data and time at the beginning of the line.
		/// </summary>
		/// <param name="line">Line to write.</param>
		public void WriteLine(string line)
		{
			if (!Enabled) return;
			writer.WriteLine(string.Format("[{0}]\t{1}", DateTime.Now, line));
			writer.Flush();
		}

		/// <summary>
		/// Writes a string into the log-file. Doesn't place carriage return.
		/// </summary>
		/// <param name="str">String to write.</param>
		public void Write(string str)
		{
			if (!Enabled) return;
			writer.Write(str);
		} 
		#endregion
	}
}
