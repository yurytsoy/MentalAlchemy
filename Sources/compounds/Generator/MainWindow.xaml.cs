/*************************************************************************
The MIT License (MIT)

Copyright (c) 2014 Yury Tsoy

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
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

using MusUtils = MentalAlchemy.Molecules.Music.MusicUtils;
using Library.Compounds;

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
				_composer.CreateComposingNetwork(connProb: 0.7f);
			}
			else
			{	//  train from a midi file.
			}

			ShowAnn(_composer);
		}

		private void PlayOriginal_Click(object sender, RoutedEventArgs e)
		{
			var rand = MusicUtils.RandomFromOffsets(32);
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

			var notes = MusUtils.ToMidi(NotesBox.Text.Split(','));
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
