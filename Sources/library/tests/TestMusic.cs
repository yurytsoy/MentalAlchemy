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

