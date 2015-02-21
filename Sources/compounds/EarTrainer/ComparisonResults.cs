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

	public class Task
	{
		public DateTime Start;
		public Midi.Pitch[] Notes;
		public List<UserAnswer> UserInputs = new List<UserAnswer>();

		public string[] ToStrings()
		{
			var res = new List<string>();
			res.Add("Task start:\t" + Start.ToShortDateString ());
			res.Add("Generated sequence:\t" + MusicUtils.ToString(Notes));
			foreach (var answer in UserInputs)
			{
				res.Add(answer.ToString ());
			}
			return res.ToArray();
		}
	}

	public class UserAnswer
	{
		public Midi.Pitch[] Notes;
		public DateTime Time;

		public override string ToString()
		{
			var res = Time.ToShortDateString () + "\t" + MusicUtils.ToString(Notes);
			return res;
		}
	}

	public class UserAnswersLogger
	{
		List<Task> _tasks = new List<Task> ();
		string _filename;

		public UserAnswersLogger(string filename) 
		{
			_filename = filename;
		}

		public void AddNewTask(Midi.Pitch[] notes)
		{
			var newTask = new Task() { Notes = notes, Start = DateTime.Now };
			_tasks.Add(newTask);
		}

		//public void AddUserAnswersToTheCurrentTask(string answer)
		//{
		//	var curTask = _tasks.Last();
		//	curTask.UserInputs.Add(answer);
		//}

		public void AddUserAnswersToTheCurrentTask(Midi.Pitch[] notes)
		{
			var curSession = _tasks.Last();
			var answer = new UserAnswer();
			answer.Notes = notes;
			answer.Time = DateTime.Now;
			curSession.UserInputs.Add(answer);
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
