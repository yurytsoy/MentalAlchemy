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

using Midi;
using MentalAlchemy.Atoms;

namespace MentalAlchemy.Molecules.Music
{
	public static class MusicUtils
	{
		public static Midi.Note[] ToMidi (string[] notes)
		{
			var res = new Midi.Note[notes.Length];
			for (int i=0; i<notes.Length; ++i)
			{
				res[i] = new Midi.Note(notes[i]);
			}
			return res;
		}
		
		public static string OffsetToNoteString (int offset)
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

		public static Midi.Note[] RandomFromOffsets(int size)
		{ 
			var noteStr = new string [size];
			for (int i = 0; i < size; ++i )
			{
				noteStr[i] = OffsetToNoteString( ContextRandom.Next(7));
			}
			var res = ToMidi(noteStr);
			return res;
		}

		/// <summary>
		/// Generates random collection, composed from elements from 128 MIDI defined pitches.
		/// </summary>
		/// <param name="size"></param>
		/// <returns></returns>
		public static Pitch[] RandomFromPitches(int size)
		{
			var pitchNames = Enum.GetNames(typeof (Pitch));
			var pitches = new Pitch[size];
			for (int i = 0; i < size; ++i)
			{
				var id = ContextRandom.Next(128);
				pitches[i] = (Pitch)Enum.Parse(typeof(Pitch), pitchNames[id]);
			}
			return pitches;
		}
	}
}

