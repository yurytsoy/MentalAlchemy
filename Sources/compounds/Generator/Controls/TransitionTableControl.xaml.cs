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
