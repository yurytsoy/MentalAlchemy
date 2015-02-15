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

using MentalAlchemy.Molecules.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EarTrainer
{
	public class ComparisonResults
	{
		public Midi.Pitch PrevNote;
		public Midi.Pitch CurNote;
		public Midi.Pitch NextNote;
		public Midi.Pitch UserNote;
	}

	public class Session
	{
		public DateTime Start;
		public Midi.Pitch[] Notes;
		public List<string> UserInputs = new List<string> ();

		public string[] ToStrings()
		{
			var res = new List<string>();
			res.Add("Session start:\t" + Start.ToShortDateString ());
			res.Add("Generated sequence:\t" + MusicUtils.ToString(Notes));
			foreach (var str in UserInputs)
			{
				res.Add(str);
			}
			return res.ToArray();
		}
	}

	public class UserAnswersLogger
	{
		List<Session> _sessions = new List<Session> ();
		string _filename;

		public UserAnswersLogger(string filename) 
		{
			_filename = filename;
		}

		public void AddNewSession(Midi.Pitch[] notes)
		{
			var newSession = new Session() { Notes = notes, Start = DateTime.Now };
			_sessions.Add(newSession);
		}

		public void AddUserAnswersToTheCurrentSession(string answers)
		{
			var curSession = _sessions.Last();
			curSession.UserInputs.Add(answers);
		}

		/// <summary>
		/// Write the sessions to the file.
		/// </summary>
		public void DumpSessionsInfo()
		{
			var lines = new List<string>();
		}
	}
}
