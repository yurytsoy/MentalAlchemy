using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalAlchemy.Molecules.Music
{
	/// <summary>
	/// The class structure is based upon the MusicXML (http://www.musicxml.com/) definition.
	/// Currently it is not so versatile and includes only the most necessary parts.
	/// </summary>
	public class Score
	{
		public List<Part> Parts;
		public string WorkNumber;
		public string WorkTitle;
		public string Composer;
		public string Poet;
		/// <summary>
		/// For a copyright;
		/// </summary>
		public string Rights;
	}
}
