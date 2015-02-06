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

