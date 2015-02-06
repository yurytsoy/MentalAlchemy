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
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using MentalAlchemy.Molecules.Music;
using MentalAlchemy.Atoms;

using Midi;
using System.Threading;
using MidiControl = Midi.Control;
using MusUtils = MentalAlchemy.Molecules.Music.MusicUtils;

namespace Generator.Controls
{
	/// <summary>
	/// Interaction logic for TransitionTableControl.xaml
	/// </summary>
	public partial class TransitionTableControl : UserControl
	{
		public TransitionTableControl()
		{
			InitializeComponent();
		}

		private void TestBtn_Click(object sender, RoutedEventArgs e)
		{
			var trComp = new TransitionTableComposer();
			var notes = trComp.Compose();
			ResultBox.Text = StringUtils.Concat (notes, ",");
		}

		private void PlayBtn_Click(object sender, RoutedEventArgs e)
		{
			var notes = MusUtils.ToMidi(ResultBox.Text.Split (','));
			var res = "";
			foreach (var note in notes)
			{
				res += note.ToString();
			}
			ResultBox.Text = res;

			Play(notes);
		}

		private void Play(Midi.Note[] notes)
		{
			if (OutputDevice.InstalledDevices.Count == 0) return;

			// Prompt user to choose an output device (or if there is only one, use that one).
			OutputDevice outputDevice = OutputDevice.InstalledDevices[0];
			outputDevice.Open();

			Console.WriteLine("Playing an arpeggiated C chord and then bending it down.");

			outputDevice.SendProgramChange(Channel.Channel1, Instrument.Bagpipe);

			outputDevice.SendControlChange(Channel.Channel1, MidiControl.SustainPedal, 0);
			outputDevice.SendPitchBend(Channel.Channel1, 8192);
			// Play C, E, G in half second intervals.
			foreach (var note in notes)
			{
				outputDevice.SendNoteOn(Channel.Channel1, note.PitchInOctave(4), 80);
				Thread.Sleep(500);
				outputDevice.SendNoteOff(Channel.Channel1, note.PitchInOctave(4), 80);
			}

			// Close the output device.
			outputDevice.Close();
		}
	}
}
