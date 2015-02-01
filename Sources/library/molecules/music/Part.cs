using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalAlchemy.Molecules.Music
{
	public class Part
	{
		/// <summary>
		/// E.g. "P1".
		/// </summary>
		public string Id;
		/// <summary>
		/// E.g. "Guitar".
		/// </summary>
		public string Name;
		/// <summary>
		/// E.g. "Gtr.".
		/// </summary>
		public string NameAbbreviation;
		public List<Measure> Measures;
		public MidiInstrument Instrument;
	}
}
