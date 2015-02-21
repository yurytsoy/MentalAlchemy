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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MentalAlchemy.Molecules.Music;

namespace EarTrainer
{
	/// <summary>
	/// Class to store global settings and parameters.
	/// </summary>
	public static class Global
	{
		public const string DEFAULT_LOG_FILE = "user.log";
		public static Midi.Pitch[] Notes { get; private set; }
		public static string UserNotesText {get; private set; }
		public static UserAnswersLogger Logger = new UserAnswersLogger(DEFAULT_LOG_FILE);

		public static void SetNotes(Midi.Pitch[] notes)
		{
			Notes = notes;
			Logger.AddNewTask(notes);
		}

		public static void SetUserNotesText (string userText, bool addAnswer = false)
		{
			UserNotesText = userText;
			if (addAnswer) 
			{
				// convert to the notes.
				var pitches = MusicUtils.ToPitches(userText);
				Logger.AddUserAnswersToTheCurrentTask(pitches);
			}
		}
	}
}
