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

using MentalAlchemy.Compounds;
using MentalAlchemy.Molecules.Music;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MusicMethods
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		enum State { Normal, PlayMusic }

		const string RANDOM_TAB_HEADER = "Random";
		const string TRANSTABLE_TAB_HEADER = "Transition table";

		State _curState = State.Normal;
		MidiPlayer _player = new MidiPlayer();
		TransitionTableComposer _transTable = new TransitionTableComposer();
		Midi.Pitch[] _notes;
		string[] _playBtnText = { "Play", "Pause" };

		int _compLength = 10;
		public int CompositionLength
		{
			get { return _compLength; }
			set
			{
				_compLength = value;
				NotifyPropertyChanged("CompositionLength");
			}
		}

		public MainWindow()
		{
			InitializeComponent();
			_transTable.InitTransitionTable();

			this.DataContext = this;
		}

		private void PlayBtn_Click(object sender, RoutedEventArgs e)
		{
			if (_player.IsBusy)
			{
				PlayBtn.Content = _playBtnText[0];
				_curState = State.Normal;
				_player.Stop();
			}
			else if (_notes != null && _notes.Length != 0)
			{
				PlayBtn.Content = _playBtnText[1];
				_curState = State.PlayMusic;
				_player.Play(_notes);
			}
		}

		private void GenerateBtn_Click(object sender, RoutedEventArgs e)
		{
			var selTab = (TabItem)MainTabControl.SelectedItem;
			var tabHeader = (string)selTab.Header;
			if (tabHeader == RANDOM_TAB_HEADER) { _notes = generateRandom(_compLength); }
			else if (tabHeader == "Transition table") { _notes = generateTransitionTable(_compLength); }
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public void NotifyPropertyChanged(string propName)
		{
			if (this.PropertyChanged != null)
				this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
		}

		#region - Generators. -
		protected Midi.Pitch[] generateRandom(int length)
		{
			var notes = MusicUtils.RandomPitchesOneLineOctaveSimple(_compLength);
			var noteStr = MusicUtils.ToString(notes, ", ");
			RandomNotesBox.Text = noteStr;
			return notes;
		}

		protected Midi.Pitch[] generateTransitionTable(int length)
		{
			var str = _transTable.Compose(noteCount: length);
			var notes = MusicUtils.ToPitches(str);
			TransTableBox.Text = MusicUtils.ToString(notes, ", ");
			return notes;
		}
		#endregion

		private void MainTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var notesStr = getCurrentNotesString();
			if (notesStr == null || notesStr.Length == 0) _notes = new Midi.Pitch[0];

			_notes = MusicUtils.ToPitches(notesStr);
		}

		protected string getCurrentNotesString()
		{ 
			var selTab = (TabItem)MainTabControl.SelectedItem;
			var tabHeader = (string)selTab.Header;
			if (tabHeader == RANDOM_TAB_HEADER) { return RandomNotesBox.Text; }
			else if (tabHeader == TRANSTABLE_TAB_HEADER) {return TransTableBox.Text; }
			return "";
		}
	}
}
