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

namespace MentalAlchemy.Molecules.Music
{
	/// <summary>
	/// Helper class for pitches.
	/// </summary>
	public class Pitches
	{
		#region - Available pitches. -
		public const string Cflat = "Cb";
		public const string C = "C";
		public const string Csharp = "C#";
		public const string Dflat = "Db";
		public const string D = "D";
		public const string Dsharp = "D#";
		public const string Eflat = "Eb";
		public const string E = "E";
		public const string Esharp = "E#";
		public const string Fflat = "Fb";
		public const string F = "F";
		public const string Fsharp = "F#";
		public const string Gflat = "Gb";
		public const string G = "G";
		public const string Gsharp = "G#";
		public const string Aflat = "Ab";
		public const string A = "A";
		public const string Asharp = "A#";
		public const string Bflat = "Bb";
		public const string B = "B";
		public const string Bsharp = "B#";
		#endregion
	}

	/// <summary>
	/// Constants to denote keys.
	/// </summary>
	public class Keys
	{
		#region - Symbolic names for keys. -

		public const string Cmaj = "Cmaj";
		public const string Dmaj = "Dmaj";
		public const string Emaj = "Emaj";
		public const string Fmaj = "Fmaj";
		public const string Gmaj = "Gmaj";

		#endregion

		/// <summary>
		/// Returns notes, correspondent to the keys.
		/// </summary>
		/// <returns>The key notes.</returns>
		/// <param name="key">Key.</param>
		public static string[] GetKeyNotes (string key)
		{
			if (key == Cmaj)
			{
				return new [] { "C", "D", "E", "F", "G", "A", "B",};
			}
			return null;
		}
	}

	public class Const
	{
		public const string MIDI_FILE_FILTER = "Midi files (*.mid)|*.mid|All files (*.*)|*.*";
		public const string SAVE_MIDI_FILE_FILTER = "*.mid|*.mid";
	}
}

