using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalAlchemy.Molecules.Music
{
	public class Note
	{
		public Pitch Pitch;
		public int Duration;
		public NoteType NoteType;
	}

	public class Pitch
	{
		public NoteStep Step;
		public int Octave;
	}

	public enum NoteStep { C, D, E, F, G, H, A, B };
	public enum NoteType { Whole, Half, Quarter, Eighth, Sixteenth, ThirtySecondth, FortySixth };
}
