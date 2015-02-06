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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using Midi;

using MentalAlchemy.Atoms;
using MentalAlchemy.Molecules.Music;
using MidiControl = Midi.Control;
using MusUtils = MentalAlchemy.Molecules.Music.MusicUtils;
using System.ComponentModel;
using Library.Compounds;
//using Composer;

namespace Generator.Controls
{
	/// <summary>
	/// Interaction logic for ComposerControl.xaml
	/// </summary>
	public partial class ComposerControl : UserControl
	{
		const string PLAY = "Play";
		const string STOP = "Stop";

		MidiPlayer _midiPlayer = new MidiPlayer();
		//BackgroundWorker _bgWorker = new BackgroundWorker();

		public ComposerControl()
		{
			InitializeComponent();

			// init composers box.
			ComposerCombo.ItemsSource = Utils.GetComposerNames();
			ComposerCombo.SelectedIndex = 0;

			//_bgWorker.WorkerSupportsCancellation = true;
			//_bgWorker.WorkerReportsProgress = true;
			//_bgWorker.DoWork += new DoWorkEventHandler(bwWorker_DoWork);
			////_bgWorker.ProgressChanged += new ProgressChangedEventHandler(bwWorker_);
			//_bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bwWorker_RunWorkerCompleted);

			_midiPlayer.OnWorkerCompleted += WorkerCompletedHandler;

			PlayBtn.Content = PLAY;
		}

		public void WorkerCompletedHandler()
		{
			PlayBtn.Content = PLAY;
		}

		private void PlayBtn_Click(object sender, RoutedEventArgs e)
		{
			var notes = MusUtils.ToMidi(NotesBox.Text.Split(','));
			if (PlayBtn.Content == PLAY)
			{
				PlayBtn.Content = STOP;
				if (!_midiPlayer.IsBusy)
				{
					_midiPlayer.Play(notes);
					//Play(notes);
				}
			}
			else
			{	// cancel playing.
				PlayBtn.Content = PLAY;
				_midiPlayer.Stop();
				//_bgWorker.CancelAsync();
			}
			//Play(notes);
		}

		private void RunBtn_Click(object sender, RoutedEventArgs e)
		{
			//
			var composerName = (string)ComposerCombo.SelectedItem;
			var composer = Utils.CreateComposer(composerName);
			var notes = composer.ComposeMonotone(16);

			var strNotes = StringUtils.Concat(notes, ','.ToString ());
			NotesBox.Text = strNotes;
			//NotesBox.
		}

		//private async void Play(Note[] notes)
		//{
		//	if (OutputDevice.InstalledDevices.Count == 0) return;

		//	// Prompt user to choose an output device (or if there is only one, use that one).
		//	OutputDevice outputDevice = OutputDevice.InstalledDevices[0];
		//	if (!outputDevice.IsOpen)
		//	{
		//		outputDevice.Open();
		//	}

		//	outputDevice.SendProgramChange(Channel.Channel1, Instrument.ElectricPiano1);

		//	outputDevice.SendControlChange(Channel.Channel1, MidiControl.SustainPedal, 0);
		//	outputDevice.SendPitchBend(Channel.Channel1, 8192);

		//	//await PlayAsync(notes, outputDevice);
		//	var args = new WorkerArgs() { Notes = notes, OutDevice = outputDevice};
		//	_bgWorker.RunWorkerAsync(args);

		//	// Close the output device.
		//	//outputDevice.Close();
		//}

		//private void bwWorker_DoWork(object sender, DoWorkEventArgs e)
		//{
		//	BackgroundWorker worker = sender as BackgroundWorker;

		//	if ((worker.CancellationPending == true))
		//	{
		//		e.Cancel = true;
		//	}
		//	else if (e.Argument is WorkerArgs)
		//	{
		//		var args = e.Argument as WorkerArgs;

		//		if (!args.OutDevice.IsOpen)
		//		{
		//			args.OutDevice.Open();
		//		}
		//		foreach (var note in args.Notes)
		//		{
		//			if ((worker.CancellationPending == true))
		//			{
		//				e.Cancel = true;
		//				return;
		//			}

		//			args.OutDevice.SendNoteOn(Channel.Channel1, note.PitchInOctave(4), 80);
		//			Thread.Sleep(300);
		//			args.OutDevice.SendNoteOff(Channel.Channel1, note.PitchInOctave(4), 80);
		//		}
		//		args.OutDevice.Close();
		//	}
		//}

		//private void bwWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		//{
		//	if ((e.Cancelled == true))
		//	{
		//	}

		//	else if (!(e.Error == null))
		//	{
		//		//this.tbProgress.Text = ("Error: " + e.Error.Message);
		//	}

		//	else
		//	{
		//		//this.tbProgress.Text = "Done!";
		//	}

		//	PlayBtn.Content = PLAY;
		//}

	}



	public class Utils
	{
		public const string NEURAL_COMPOSER = "Neural Composer";

		public static string[] GetComposerNames ()
		{ 
			return new [] {NEURAL_COMPOSER};
		}

		public static BaseComposer CreateComposer(string name)
		{
			if (name == NEURAL_COMPOSER)
			{
				var comp = new NeuralComposer();
				comp.CreateComposingNetwork(0.8f);
				return comp;
			}
			return null;
		}
	}
}
