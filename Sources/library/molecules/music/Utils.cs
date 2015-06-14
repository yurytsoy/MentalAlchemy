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

using Midi;
using MentalAlchemy.Atoms;

namespace MentalAlchemy.Molecules.Music
{
	public static class MusicUtils
	{
		#region - Conversion. -
		/// <summary>
		/// Converts given string to a collection of pitches.
		/// The string assumes that pitches are separated by either [separator] or space.
		/// Alternatively the pitches can be written as one-character-notes from a set {C, D, ..., B}.
		/// </summary>
		/// <param name="notes"></param>
		/// <param name="defaultOctave"></param>
		/// <param name="separator"></param>
		/// <returns></returns>
		public static Midi.Pitch[] ToPitches(string notes, int defaultOctave = 4, string separator = ",")
		{
			string[] noteArray;
			#region - Split original input into separated pitches. -
			if (notes.Contains(separator))
			{
				noteArray = notes.Split(separator[0]);
			}
			else if (notes.Contains(" "))
			{
				noteArray = notes.Split(' ');
			}
			else
			{
				// else parse the text as a continuous string.
				var tmp = new List<string>();
				for (int i = 0; i < notes.Length; ++i)
				{	// TODO: fix to enable recognition of pitches, like "A4", "D2", etc.
					tmp.Add(notes[i].ToString());
				}
				noteArray = tmp.ToArray();
			} 
			#endregion

			return ToPitches(noteArray, defaultOctave);

			//#region - Standardize pitches for the [Midi.Pitch] data type. -
			//// reference: Midi.Pitch.A0;
			//// reference: Midi.Pitch.ASharp5;
			//for (int i = 0; i < noteArray.Length; ++i )
			//{
			//	var cur = noteArray[i];
			//	cur = cur.Trim();	// remove extra spaces.

			//	// enforce capital first letter.
			//	var first = cur[0] + "";
			//	cur = cur.Remove(0, 1);
			//	cur = cur.Insert(0, first.ToUpper());

			//	// enforce "Sharp".
			//	if (cur.ToLower().Contains ("sharp"))
			//	{
			//		cur = cur.Remove(1, 5);
			//		cur = cur.Insert(1, "Sharp");
			//	}

			//	// TODO: process flats.

			//	// check if a pitch has a number at the end.
			//	var last = cur[cur.Length - 1];
			//	if (!char.IsDigit (last))
			//	{
			//		cur += defaultOctave;
			//	}
			//	noteArray[i] = cur;
			//}
			//#endregion

			//var res = new Midi.Pitch[noteArray.Length];
			//for (int i = 0; i < noteArray.Length; ++i )
			//{
			//	res[i] = (Midi.Pitch)Enum.Parse(typeof(Midi.Pitch), noteArray[i]);
			//}

			//return res;
		}

		/// <summary>
		/// Converts given string to a collection of pitches.
		/// The string assumes that pitches are separated by either [separator] or space.
		/// Alternatively the pitches can be written as one-character-notes from a set {C, D, ..., B}.
		/// </summary>
		/// <param name="notes"></param>
		/// <param name="defaultOctave"></param>
		/// <param name="separator"></param>
		/// <returns></returns>
		public static Midi.Pitch[] ToPitches(string[] notes, int defaultOctave = 4)
		{
			#region - Standardize pitches for the [Midi.Pitch] data type. -
			// reference: Midi.Pitch.A0;
			// reference: Midi.Pitch.ASharp5;
			for (int i = 0; i < notes.Length; ++i)
			{
				var cur = notes[i];
				cur = cur.Trim();	// remove extra spaces.

				// enforce capital first letter.
				var first = cur[0] + "";
				cur = cur.Remove(0, 1);
				cur = cur.Insert(0, first.ToUpper());

				// enforce "Sharp".
				if (cur.ToLower().Contains("sharp"))
				{
					cur = cur.Remove(1, 5);
					cur = cur.Insert(1, "Sharp");
				}

				// TODO: process flats.

				// check if a pitch has a number at the end.
				var last = cur[cur.Length - 1];
				if (!char.IsDigit(last))
				{
					cur += defaultOctave;
				}
				notes[i] = cur;
			}
			#endregion

			var res = new Midi.Pitch[notes.Length];
			for (int i = 0; i < notes.Length; ++i)
			{
				res[i] = (Midi.Pitch)Enum.Parse(typeof(Midi.Pitch), notes[i]);
			}

			return res;
		}
		
		public static Midi.Note[] ToMidi(string[] notes)
		{
			var res = new Midi.Note[notes.Length];
			for (int i = 0; i < notes.Length; ++i)
			{
				res[i] = new Midi.Note(notes[i]);
			}
			return res;
		}

		public static string OffsetToNoteString(int offset)
		{
			switch (offset)
			{
				case 0: return Pitches.C;
				case 1: return Pitches.D;
				case 2: return Pitches.E;
				case 3: return Pitches.F;
				case 4: return Pitches.G;
				case 5: return Pitches.A;
				case 6: return Pitches.B;
			}
			return "";
		}

		public static string ToString(Midi.Pitch[] pitches, string separator = "\t")
		{
			var res = pitches[0].ToString();
			for (int i = 1; i < pitches.Length; ++i)
			{
				res += separator + pitches[i].ToString();
			}
			return res;
		}
		#endregion

		#region - Generation. -
		public static Midi.Note[] RandomFromOffsets(int size)
		{
			var noteStr = new string[size];
			for (int i = 0; i < size; ++i)
			{
				noteStr[i] = OffsetToNoteString(ContextRandom.Next(7));
			}
			var res = ToMidi(noteStr);
			return res;
		}

		/// <summary>
		/// Generates random collection, composed from elements from 128 MIDI defined pitches.
		/// </summary>
		/// <param name="size"></param>
		/// <returns></returns>
		public static Midi.Pitch[] RandomFromPitches(int size)
		{
			var pitchNames = Enum.GetNames(typeof(Midi.Pitch));
			var pitches = new Midi.Pitch[size];
			for (int i = 0; i < size; ++i)
			{
				var id = ContextRandom.Next(pitchNames.Length);
				pitches[i] = (Midi.Pitch)Enum.Parse(typeof(Midi.Pitch), pitchNames[id]);
			}
			return pitches;
		}

		/// <summary>
		/// Generates random collection, composed from elements from the one line octave without alterations.
		/// </summary>
		/// <param name="size"></param>
		/// <returns></returns>
		public static Midi.Pitch[] RandomPitchesOneLineOctaveSimple(int size)
		{
			var pitches = GetPitchesOneLineOctaveSimple();
			return VectorMath.RandomSample(pitches, size);
		}

		/// <summary>
		/// Generates random collection, composed from elements from the one line octave.
		/// </summary>
		/// <param name="size"></param>
		/// <returns></returns>
		public static Midi.Pitch[] RandomPitchesOneLineOctave(int size)
		{
			var pitches = GetPitchesOneLineOctave();
			return VectorMath.RandomSample(pitches, size);
		}

		/// <summary>
		/// Generates random collection, composed from elements correspondent to the box from fret 0 to fret 4 for the guitar with standard tuning.
		/// </summary>
		/// <param name="size"></param>
		/// <returns></returns>
		public static Midi.Pitch[] RandomPitchesBox04Simple(int size)
		{
			var pitches = GetPitchesBox04Simple();
			return VectorMath.RandomSample(pitches, size);
		}

		/// <summary>
		/// Generates random collection, composed from elements correspondent to the box from fret 5 to fret 9 for the guitar with standard tuning.
		/// </summary>
		/// <param name="size"></param>
		/// <returns></returns>
		public static Midi.Pitch[] RandomPitchesBox59Simple(int size)
		{
			var pitches = GetPitchesBox59Simple();
			return VectorMath.RandomSample(pitches, size);
		}

		/// <summary>
		/// Generates random collection, composed from elements correspondent to the box from fret 7 to fret 12 for the guitar with standard tuning.
		/// </summary>
		/// <param name="size"></param>
		/// <returns></returns>
		public static Midi.Pitch[] RandomPitchesBox712Simple(int size)
		{
			var pitches = GetPitchesBox712Simple();
			return VectorMath.RandomSample(pitches, size);
		}

		/// <summary>
		/// Generates random collection, composed from elements from the pitches corresponding to the 1st string of a guitar with std tuning.
		/// </summary>
		/// <param name="size"></param>
		/// <returns></returns>
		public static Midi.Pitch[] RandomPitchesString1(int size)
		{
			var pitches = GetPitchesString1();
			return VectorMath.RandomSample(pitches, size);
		}

		/// <summary>
		/// Generates random collection, composed from elements from the pitches corresponding to the 2nd string of a guitar with std tuning.
		/// </summary>
		/// <param name="size"></param>
		/// <returns></returns>
		public static Midi.Pitch[] RandomPitchesString2(int size)
		{
			var pitches = GetPitchesString2();
			return VectorMath.RandomSample(pitches, size);
		}

		/// <summary>
		/// Generates random collection, composed from elements from the pitches corresponding to the 3rd string of a guitar with std tuning.
		/// </summary>
		/// <param name="size"></param>
		/// <returns></returns>
		public static Midi.Pitch[] RandomPitchesString3(int size)
		{
			var pitches = GetPitchesString3();
			return VectorMath.RandomSample(pitches, size);
		}

		/// <summary>
		/// Generates random collection, composed from elements from the pitches corresponding to the 4th string of a guitar with std tuning.
		/// </summary>
		/// <param name="size"></param>
		/// <returns></returns>
		public static Midi.Pitch[] RandomPitchesString4(int size)
		{
			var pitches = GetPitchesString4();
			return VectorMath.RandomSample(pitches, size);
		}

		/// <summary>
		/// Generates random collection, composed from elements from the pitches corresponding to the 5th string of a guitar with std tuning.
		/// </summary>
		/// <param name="size"></param>
		/// <returns></returns>
		public static Midi.Pitch[] RandomPitchesString5(int size)
		{
			var pitches = GetPitchesString5();
			return VectorMath.RandomSample(pitches, size);
		}

		/// <summary>
		/// Generates random collection, composed from elements from the pitches corresponding to the 6th string of a guitar with std tuning.
		/// </summary>
		/// <param name="size"></param>
		/// <returns></returns>
		public static Midi.Pitch[] RandomPitchesString6(int size)
		{
			var pitches = GetPitchesString6();
			return VectorMath.RandomSample(pitches, size);
		} 
		#endregion

		#region - Pitches reterival. -
		public static Midi.Pitch[] GetPitchesOneLineOctaveSimple()
		{
			var res = new Midi.Pitch[7];
			res[0] = Midi.Pitch.C4;
			res[1] = Midi.Pitch.D4;
			res[2] = Midi.Pitch.E4;
			res[3] = Midi.Pitch.F4;
			res[4] = Midi.Pitch.G4;
			res[5] = Midi.Pitch.A4;
			res[6] = Midi.Pitch.B4;

			return res;
		}

		public static Midi.Pitch[] GetPitchesOneLineOctave()
		{
			var res = new Midi.Pitch[12];
			res[0] = Midi.Pitch.C4;
			res[1] = Midi.Pitch.CSharp4;
			res[2] = Midi.Pitch.D4;
			res[3] = Midi.Pitch.DSharp4;
			res[4] = Midi.Pitch.E4;
			res[5] = Midi.Pitch.F4;
			res[6] = Midi.Pitch.FSharp4;
			res[7] = Midi.Pitch.G4;
			res[8] = Midi.Pitch.GSharp4;
			res[9] = Midi.Pitch.A4;
			res[10] = Midi.Pitch.ASharp4;
			res[11] = Midi.Pitch.B4;

			return res;
		}

		public static Midi.Pitch[] GetPitchesBox04Simple()
		{
			var res = new Midi.Pitch[16];
			res[0] = Midi.Pitch.E2;
			res[1] = Midi.Pitch.F2;
			res[2] = Midi.Pitch.G2;
			res[3] = Midi.Pitch.A2;
			res[4] = Midi.Pitch.B2;
			res[5] = Midi.Pitch.C3;
			res[6] = Midi.Pitch.D3;
			res[7] = Midi.Pitch.E3;
			res[8] = Midi.Pitch.F3;
			res[9] = Midi.Pitch.G3;
			res[10] = Midi.Pitch.A3;
			res[11] = Midi.Pitch.B3;
			res[12] = Midi.Pitch.C4;
			res[13] = Midi.Pitch.D4;
			res[14] = Midi.Pitch.E4;
			res[15] = Midi.Pitch.F4;

			return res;
		}

		public static Midi.Pitch[] GetPitchesBox59Simple()
		{
			var res = new Midi.Pitch[17];
			res[0] = Midi.Pitch.A2;
			res[1] = Midi.Pitch.B2;
			res[2] = Midi.Pitch.C3;
			res[3] = Midi.Pitch.D3;
			res[4] = Midi.Pitch.E3;
			res[5] = Midi.Pitch.F3;
			res[6] = Midi.Pitch.G3;
			res[7] = Midi.Pitch.A3;
			res[8] = Midi.Pitch.B3;
			res[9] = Midi.Pitch.C4;
			res[10] = Midi.Pitch.D4;
			res[11] = Midi.Pitch.E4;
			res[12] = Midi.Pitch.F4;
			res[13] = Midi.Pitch.G4;
			res[14] = Midi.Pitch.A4;
			res[15] = Midi.Pitch.B4;
			res[16] = Midi.Pitch.C5;

			return res;
		}

		public static Midi.Pitch[] GetPitchesBox712Simple()
		{
			var res = new Midi.Pitch[18];
			res[0] = Midi.Pitch.B2;
			res[1] = Midi.Pitch.C3;
			res[2] = Midi.Pitch.D3;
			res[3] = Midi.Pitch.E3;
			res[4] = Midi.Pitch.F3;
			res[5] = Midi.Pitch.G3;
			res[6] = Midi.Pitch.A3;
			res[7] = Midi.Pitch.B3;
			res[8] = Midi.Pitch.C4;
			res[9] = Midi.Pitch.D4;
			res[10] = Midi.Pitch.E4;
			res[11] = Midi.Pitch.F4;
			res[12] = Midi.Pitch.G4;
			res[13] = Midi.Pitch.A4;
			res[14] = Midi.Pitch.B4;
			res[15] = Midi.Pitch.C5;
			res[16] = Midi.Pitch.D5;
			res[17] = Midi.Pitch.E5;

			return res;
		}

		/// <summary>
		/// Returns pitches up to 12th fret (including open string) for the 1st string for the std tuning of guitar.
		/// Sharps are used for the semitones.
		/// </summary>
		/// <returns></returns>
		public static Midi.Pitch[] GetPitchesString1()
		{
			var res = new Midi.Pitch[13];
			res[0] = Midi.Pitch.E4;
			res[1] = Midi.Pitch.F4;
			res[2] = Midi.Pitch.FSharp4;
			res[3] = Midi.Pitch.G4;
			res[4] = Midi.Pitch.GSharp4;
			res[5] = Midi.Pitch.A4;
			res[6] = Midi.Pitch.ASharp4;
			res[7] = Midi.Pitch.B4;
			res[8] = Midi.Pitch.C5;
			res[9] = Midi.Pitch.CSharp5;
			res[10] = Midi.Pitch.D5;
			res[11] = Midi.Pitch.DSharp5;
			res[12] = Midi.Pitch.E5;

			return res;
		}

		/// <summary>
		/// Returns pitches up to 12th fret (including open string) for the 2nd string for the std tuning of guitar.
		/// Sharps are used for the semitones.
		/// </summary>
		/// <returns></returns>
		public static Midi.Pitch[] GetPitchesString2()
		{
			var res = new Midi.Pitch[13];
			res[0] = Midi.Pitch.B3;
			res[1] = Midi.Pitch.C4;
			res[2] = Midi.Pitch.CSharp4;
			res[3] = Midi.Pitch.D4;
			res[4] = Midi.Pitch.DSharp4;
			res[5] = Midi.Pitch.E4;
			res[6] = Midi.Pitch.F4;
			res[7] = Midi.Pitch.FSharp4;
			res[8] = Midi.Pitch.G4;
			res[9] = Midi.Pitch.GSharp4;
			res[10] = Midi.Pitch.A4;
			res[11] = Midi.Pitch.ASharp4;
			res[12] = Midi.Pitch.B4;

			return res;
		}

		/// <summary>
		/// Returns pitches up to 12th fret (including open string) for the 3rd string for the std tuning of guitar.
		/// Sharps are used for the semitones.
		/// </summary>
		/// <returns></returns>
		public static Midi.Pitch[] GetPitchesString3()
		{
			var res = new Midi.Pitch[13];
			res[0] = Midi.Pitch.G3;
			res[1] = Midi.Pitch.GSharp3;
			res[2] = Midi.Pitch.A3;
			res[3] = Midi.Pitch.ASharp3;
			res[4] = Midi.Pitch.B3;
			res[5] = Midi.Pitch.C4;
			res[6] = Midi.Pitch.CSharp4;
			res[7] = Midi.Pitch.D4;
			res[8] = Midi.Pitch.DSharp4;
			res[9] = Midi.Pitch.E4;
			res[10] = Midi.Pitch.F4;
			res[11] = Midi.Pitch.FSharp4;
			res[12] = Midi.Pitch.G4;

			return res;
		}

		/// <summary>
		/// Returns pitches up to 12th fret (including open string) for the 4th string for the std tuning of guitar.
		/// Sharps are used for the semitones.
		/// </summary>
		/// <returns></returns>
		public static Midi.Pitch[] GetPitchesString4()
		{
			var res = new Midi.Pitch[13];
			res[0] = Midi.Pitch.D3;
			res[1] = Midi.Pitch.DSharp3;
			res[2] = Midi.Pitch.E3;
			res[3] = Midi.Pitch.F3;
			res[4] = Midi.Pitch.FSharp3;
			res[5] = Midi.Pitch.G3;
			res[6] = Midi.Pitch.GSharp3;
			res[7] = Midi.Pitch.A3;
			res[8] = Midi.Pitch.ASharp3;
			res[9] = Midi.Pitch.B3;
			res[10] = Midi.Pitch.C4;
			res[11] = Midi.Pitch.CSharp4;
			res[12] = Midi.Pitch.D4;

			return res;
		}

		/// <summary>
		/// Returns pitches up to 12th fret (including open string) for the 5th string for the std tuning of guitar.
		/// Sharps are used for the semitones.
		/// </summary>
		/// <returns></returns>
		public static Midi.Pitch[] GetPitchesString5()
		{
			var res = new Midi.Pitch[13];
			res[0] = Midi.Pitch.A2;
			res[1] = Midi.Pitch.ASharp2;
			res[2] = Midi.Pitch.B2;
			res[3] = Midi.Pitch.C3;
			res[4] = Midi.Pitch.CSharp3;
			res[5] = Midi.Pitch.D3;
			res[6] = Midi.Pitch.DSharp3;
			res[7] = Midi.Pitch.E3;
			res[8] = Midi.Pitch.F3;
			res[9] = Midi.Pitch.FSharp3;
			res[10] = Midi.Pitch.G3;
			res[11] = Midi.Pitch.GSharp3;
			res[12] = Midi.Pitch.A3;

			return res;
		}

		/// <summary>
		/// Returns pitches up to 12th fret (including open string) for the 5th string for the std tuning of guitar.
		/// Sharps are used for the semitones.
		/// </summary>
		/// <returns></returns>
		public static Midi.Pitch[] GetPitchesString6()
		{
			var res = new Midi.Pitch[13];
			res[0] = Midi.Pitch.E2;
			res[1] = Midi.Pitch.F2;
			res[2] = Midi.Pitch.FSharp2;
			res[3] = Midi.Pitch.G2;
			res[4] = Midi.Pitch.GSharp2;
			res[5] = Midi.Pitch.A2;
			res[6] = Midi.Pitch.ASharp2;
			res[7] = Midi.Pitch.B2;
			res[8] = Midi.Pitch.C3;
			res[9] = Midi.Pitch.CSharp3;
			res[10] = Midi.Pitch.D3;
			res[11] = Midi.Pitch.DSharp3;
			res[12] = Midi.Pitch.E3;

			return res;
		} 
		#endregion
	}
}

