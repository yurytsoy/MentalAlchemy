using MentalAlchemy.Molecules.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace EarTrainer
{
	public class HistoryTreeViewItem : TreeViewItem
	{
		/// <summary>
		/// Task reference.
		/// </summary>
		Task _task;
		UserAnswer _userAnswer;

		public string TaskSequence
		{
			get { return MusicUtils.ToString(_task.Notes, ","); }
		}

		public string StartTime
		{
			get { return _task.Start.ToShortTimeString(); }
		}

		public string AnswerTime
		{
			get { return _userAnswer.Time.ToShortTimeString(); }
		}

		public string UserAnswer
		{
			get { return MusicUtils.ToString(_userAnswer.Notes, ","); }
		}

		public string Test
		{
			get { return "taradfafd"; }
		}

		public HistoryTreeViewItem(UserAnswer uAnswer, TreeViewItem parent)
		{
			_userAnswer = uAnswer;
			Header = AnswerTime + ": " + UserAnswer;
			parent.Items.Add (this);
		}

		public HistoryTreeViewItem(Task task)
		{
			_task = task;
			Header = StartTime + ": " + TaskSequence;
		}
	}
}
