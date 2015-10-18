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

using Microsoft.Win32;
using System;
using System.Collections.Generic;
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

//using Composer;
using MentalAlchemy.Atoms;
using MentalAlchemy.Molecules.Music;
using MentalAlchemy.Compounds;

using MusUtils = MentalAlchemy.Molecules.Music.MusicUtils;
using MentalAlchemy.Molecules;

namespace Generator
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		OpenFileDialog _openDlg = new OpenFileDialog();
		NeuralComposer _composer = new NeuralComposer();
		MidiPlayer _player = new MidiPlayer();
		string midiFile;

		public MainWindow()
		{
			InitializeComponent();

			_openDlg.Filter = MentalAlchemy.Molecules.Music.Const.MIDI_FILE_FILTER;
		}

		#region - Event handlers. -
		private void TrainBtn_Click(object sender, RoutedEventArgs e)
		{
			// check whether midi file is available.
			if (string.IsNullOrEmpty(midiFile))
			{	// make random composer.
				_composer.CreateComposingNetwork(connProb: 0.7f, actFunc: ActivationFunctions.SigmoidProbBinary);
			}
			else
			{	//  train from a midi file.
			}

			ShowAnn(_composer);
		}

		private void PlayOriginal_Click(object sender, RoutedEventArgs e)
		{
			var rand = MusicUtils.RandomFromOffsets( int.Parse (CompLengthBox.Text));
			_player.Play(rand);
		}

		private void ComposeBtn_Click(object sender, RoutedEventArgs e)
		{
			int length;
			if (int.TryParse(CompLengthBox.Text, out length))
			{
				_composer.ResetOutputs();
				var notes = _composer.ComposeMonotone(length);
				if (notes == null) return;

				var strNotes = StringUtils.Concat(notes, ",");
				NotesBox.Text = strNotes;
			};
		}

		private void PlayAnnBtn_Click(object sender, RoutedEventArgs e)
		{
			if (_player.IsBusy) return;

			var notes = MusUtils.ToNote(NotesBox.Text.Split(','));
			_player.Play(notes);
		} 
		#endregion

		#region - Utils. -
		protected void ShowAnn(NeuralComposer composer)
		{
			AnnListBox.ItemsSource = composer.ToDotFormat();
			//QuickGraph.Graphviz.
		}
		#endregion
	}
}
