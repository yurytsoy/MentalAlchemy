/*************************************************************************
The MIT License (MIT)

Copyright (c) 2014 Yury Tsoy

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
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

