using System;

using MentalAlchemy.Atoms;
using MentalAlchemy.Molecules;
using MentalAlchemy.Molecules.Music;

using MusUtils = MentalAlchemy.Molecules.Music.MusicUtils;

namespace MentalAlchemy.Tests
{
	public class TestMusic
	{
		public static void TestTransitionTable ()
		{
			var comp = new TransitionTableComposer ();
			var notes = comp.Compose ();
			var midi = MusUtils.ToMidi (notes);


			var notesStr = VectorMath.ConvertToString (notes, ',');
			Console.WriteLine ("Resulting notes:");
			Console.WriteLine (notesStr);
		}
	}
}

