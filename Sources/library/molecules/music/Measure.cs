using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalAlchemy.Molecules.Music
{
	public class Measure
	{
		public Clef Clef;
		public int Number;
		public List<Note> Notes;
	}

	public enum Clef { Gclef, Fclef, Tab };
}
