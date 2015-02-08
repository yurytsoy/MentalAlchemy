﻿/*************************************************************************
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


using Library.Compounds;
using MentalAlchemy.Atoms;
using MentalAlchemy.Molecules.Music;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MusUtils = MentalAlchemy.Molecules.Music.MusicUtils;

namespace EarTrainer
{
	/// <summary>
	/// Based upon the example from AlphaTab.
	/// </summary>
	public class MainWindowViewModel : INotifyPropertyChanged
	{
		#region - INotifyPropertyChanged implementation. -
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		} 
		#endregion

		//#region - Support for the AlphaTab. -
		//private readonly RelayCommand _showScoreInfoCommand;

		///// <summary>
		///// A command which raises a file opening
		///// </summary>
		//public ICommand OpenFileCommand { get; private set; }

		///// <summary>
		///// Gets or sets the currently opened score. 
		///// If a new score is selected, the first track gets loaded.
		///// </summary>
		//private Score _score;
		//public Score Score
		//{
		//	get { return _score; }
		//	set
		//	{
		//		_score = value;
		//		OnPropertyChanged("ScoreTitle");
		//		//_showScoreInfoCommand.RaiseCanExecuteChanged();

		//		// select the first track
		//		CurrentTrackIndex = 0;
		//	}
		//}


		///// <summary>
		///// Gets or sets the index of the track which should be currently displayed.
		///// </summary>
		//private int _currentTrackIndex;
		//public int CurrentTrackIndex
		//{
		//	get { return _currentTrackIndex; }
		//	set
		//	{
		//		_currentTrackIndex = value;

		//		// update the visual track selection if a new track is selected
		//		// UpdateSelectedViewModel();

		//		// notify the ui
		//		OnPropertyChanged("CurrentTrack");
		//	}
		//}

		///// <summary>
		///// Gets the currently selected track. 
		///// </summary>
		//public Track CurrentTrack
		//{
		//	get
		//	{
		//		if (Score == null || CurrentTrackIndex < 0 || CurrentTrackIndex >= _score.tracks.length) return null;
		//		return (Track)_score.tracks[_currentTrackIndex];
		//	}
		//} 
		//#endregion

		MidiPlayer _player = new MidiPlayer();
		Midi.Pitch[] _notes;
		string _separator = ",";

		int _notesLength = 5;
		public int NotesLength
		{
			get { return _notesLength; }
			set 
			{
				_notesLength = value;
				OnPropertyChanged("NotesLength");
			}
		}

		bool _showNotes;
		public bool ShowGeneratedNotes
		{
			get { return _showNotes; }
			set 
			{
				_showNotes = value;
				OnPropertyChanged("ShowGeneratedNotes");

				NotesText = _notesText;	// yes this looks ugly.
			}
		}

		string _notesText = "";
		public string NotesText
		{
			get { return ShowGeneratedNotes? _notesText : ""; }
			set 
			{
				_notesText = value;
				OnPropertyChanged("NotesText");
			}
		}

		string _userNotesText = "";
		public string UserNotesText
		{
			get { return _userNotesText; }
			set 
			{
				_userNotesText = value;
				OnPropertyChanged("UserNotesText");
			}
		}

		string _statusText = "";
		public string StatusText
		{
			get { return _statusText; }
			set 
			{
				_statusText = value;
				OnPropertyChanged("StatusText");
			}
		}

		ObservableCollection<string> _taskModes;
		public ObservableCollection<string> TaskModes
		{
			get { return _taskModes; }
		}

		int _curTaskModeIndex;
		public int CurTaskModeIndex
		{
			get { return _curTaskModeIndex; }
			set 
			{
				_curTaskModeIndex = value;
				OnPropertyChanged("CurTaskModeIndex");
			}
		}

		public ICommand Generate { get; private set; }
		public ICommand Play { get; private set; }
		public ICommand Compare { get; private set; }
		public ICommand About { get; private set; }

		public MainWindowViewModel()
		{
			_taskModes = new ObservableCollection<string>();
			_taskModes.Add(Tools.MODE_OCTAVE4_SIMPLE);
			_taskModes.Add(Tools.MODE_STRING1);
			_taskModes.Add(Tools.MODE_STRING2);
			_taskModes.Add(Tools.MODE_STRING3);
			_taskModes.Add(Tools.MODE_STRING4);
			_taskModes.Add(Tools.MODE_STRING5);
			_taskModes.Add(Tools.MODE_STRING6);
			//_taskModes.Add(Tools.MODE_POSITION1);
			//_taskModes.Add(Tools.MODE_POSITION2);
			//_taskModes.Add(Tools.MODE_POSITION3);
			//_taskModes.Add(Tools.MODE_POSITION4);
			_curTaskModeIndex = 0;

			Generate = new RelayCommand(GenerateNotes);
			Play = new RelayCommand(PlayNotes);
			Compare = new RelayCommand(CompareNotes);
			About = new RelayCommand(AboutClick);
		}

		public void GenerateNotes ()
		{
			int length = _notesLength;
			string mode = _taskModes[_curTaskModeIndex];

			_notes = GenerateNotes (mode, length);

			//_notesText = _notes[0].Letter.ToString ();
			_notesText = _notes[0].ToString();
			for (int i = 1; i < _notes.Length; ++i)
			{
				_notesText += _separator + _notes[i].ToString();
			}
			NotesText = _notesText;
		}

		public void PlayNotes()
		{
			if (_notes == null) return;

			if (!_player.IsBusy)
			{
				_player.Play(_notes);
			}
			else
			{
				_player.Stop();
			}
		}

		public void CompareNotes()
		{
			if (_userNotesText.Length == 0 || _notesText.Length == 0) return;

			var userNotesStr = getUserNotes(_userNotesText);
			int errors = 0;
			for (int i = 0; i < System.Math.Min (_notesLength, userNotesStr.Length); ++i )
			{
				var notestr = _notes[i].ToString();
				var note = notestr.Substring(0, notestr.Length - 1).ToLower();
				if (note != userNotesStr[i].ToLower())
				{
					errors++;
				}
			}
			errors += System.Math.Abs(_notesLength - userNotesStr.Length);
			StatusText = "Errors count: " + errors;
		}

		public void AboutClick()
		{
			AboutWindow wnd = new AboutWindow();
			wnd.ShowDialog();
		}

		protected Midi.Pitch[] GenerateNotes(string mode, int length)
		{
			if (mode == Tools.MODE_OCTAVE4_SIMPLE)
			{
				return MusicUtils.RandomPitchesOneLineOctaveSimple(length);
			}
			if (mode == Tools.MODE_STRING1)
			{
				return MusicUtils.RandomPitchesString1(length);
			}
			if (mode == Tools.MODE_STRING2)
			{
				return MusicUtils.RandomPitchesString2(length);
			}
			if (mode == Tools.MODE_STRING3)
			{
				return MusicUtils.RandomPitchesString3(length);
			}
			if (mode == Tools.MODE_STRING4)
			{
				return MusicUtils.RandomPitchesString4(length);
			}
			if (mode == Tools.MODE_STRING5)
			{
				return MusicUtils.RandomPitchesString5(length);
			}
			if (mode == Tools.MODE_STRING6)
			{
				return MusicUtils.RandomPitchesString6(length);
			}
			throw new NotImplementedException();
		}

		protected string[] getUserNotes(string userText)
		{
			if (userText.Contains(_separator))
			{
				return userText.Split(_separator[0]);
			}
			else if (userText.Contains (' '))
			{
				return userText.Split(' ');
			}

			// else parse the text as a continuous string.
			var res = new List<string>();
			for (int i = 0; i < userText.Length; ++i )
			{	// TODO: fix to enable recognition of pitches, like "A4", "D2", etc.
				res.Add(userText[i].ToString ());
			}
			return res.ToArray();
		}
	}
}
