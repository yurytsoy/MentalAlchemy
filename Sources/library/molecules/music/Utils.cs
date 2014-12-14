using System;

using Midi;
using MentalAlchemy.Atoms;

namespace MentalAlchemy.Molecules.Music
{
	public static class MusicUtils
	{
		public static Note[] ToMidi (string[] notes)
		{
			var res = new Note[notes.Length];
			for (int i=0; i<notes.Length; ++i)
			{
				res [i] = new Note ( notes [i] );
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

		public static Note[] Random(int size)
		{ 
			var noteStr = new string [size];
			for (int i = 0; i < size; ++i )
			{
				noteStr[i] = OffsetToNoteString( ContextRandom.Next(7));
			}
			var res = ToMidi(noteStr);
			return res;
		}
	}
}

