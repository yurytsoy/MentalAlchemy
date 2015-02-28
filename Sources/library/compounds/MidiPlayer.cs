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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Midi;
using MidiControl = Midi.Control;
using System.Threading;

namespace MentalAlchemy.Compounds
{
	/// <summary>
	/// TODO: Add Pause.
	/// </summary>
	public class MidiPlayer
	{
		BackgroundWorker _bgWorker = new BackgroundWorker();

		public delegate void PlayCompleted();
		public PlayCompleted OnWorkerCompleted { get; set; }

		public bool IsBusy
		{
			get { return _bgWorker.IsBusy; }
		}

		public MidiPlayer()
		{
			_bgWorker.WorkerSupportsCancellation = true;
			_bgWorker.WorkerReportsProgress = true;
			_bgWorker.DoWork += new DoWorkEventHandler(bwWorker_DoWork);
			//_bgWorker.ProgressChanged += new ProgressChangedEventHandler(bwWorker_);
			_bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bwWorker_RunWorkerCompleted);
		}

		public async void Play(Note[] notes)
		{
			if (OutputDevice.InstalledDevices.Count == 0) return;

			// Prompt user to choose an output device (or if there is only one, use that one).
			OutputDevice outputDevice = OutputDevice.InstalledDevices[0];
			if (!outputDevice.IsOpen)
			{
				outputDevice.Open();
			}

			outputDevice.SendProgramChange(Channel.Channel1, Instrument.ElectricPiano1);

			outputDevice.SendControlChange(Channel.Channel1, MidiControl.SustainPedal, 0);
			outputDevice.SendPitchBend(Channel.Channel1, 8192);

			var args = new WorkerArgs() { Notes = notes, Pitches = null, OutDevice = outputDevice };
			_bgWorker.RunWorkerAsync(args);
		}

		public async void Play(Pitch[] pitches)
		{
			if (OutputDevice.InstalledDevices.Count == 0) return;

			// Prompt user to choose an output device (or if there is only one, use that one).
			OutputDevice outputDevice = OutputDevice.InstalledDevices[0];
			if (!outputDevice.IsOpen)
			{
				outputDevice.Open();
			}

			outputDevice.SendProgramChange(Channel.Channel1, Instrument.ElectricPiano1);

			outputDevice.SendControlChange(Channel.Channel1, MidiControl.SustainPedal, 0);
			outputDevice.SendPitchBend(Channel.Channel1, 8192);

			var args = new WorkerArgs() { Notes = null, Pitches = pitches, OutDevice = outputDevice };
			_bgWorker.RunWorkerAsync(args);
		}

		public void Stop()
		{
			if (_bgWorker.IsBusy)
			{
				_bgWorker.CancelAsync();
			}
		}

		private void bwWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			BackgroundWorker worker = sender as BackgroundWorker;

			if ((worker.CancellationPending == true))
			{
				e.Cancel = true;
			}
			else if (e.Argument is WorkerArgs)
			{
				var args = e.Argument as WorkerArgs;

				if (!args.OutDevice.IsOpen)
				{
					args.OutDevice.Open();
				}

				if (args.Notes != null)
				{
					foreach (var note in args.Notes)
					{
						if ((worker.CancellationPending == true))
						{
							e.Cancel = true;
							return;
						}

						args.OutDevice.SendNoteOn(Channel.Channel1, note.PitchInOctave(4), 80);
						Thread.Sleep(300);
						args.OutDevice.SendNoteOff(Channel.Channel1, note.PitchInOctave(4), 80);
					}
				}
				else if (args.Pitches != null)
				{
					foreach (var pitch in args.Pitches)
					{
						if ((worker.CancellationPending == true))
						{
							e.Cancel = true;
							return;
						}

						args.OutDevice.SendNoteOn(Channel.Channel1, pitch, 80);
						Thread.Sleep(300);
						args.OutDevice.SendNoteOff(Channel.Channel1, pitch, 80);
					}
				}
				args.OutDevice.Close();
			}
		}

		private void bwWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if ((e.Cancelled == true))
			{
			}

			else if (!(e.Error == null))
			{
				//this.tbProgress.Text = ("Error: " + e.Error.Message);
			}

			else
			{
				//this.tbProgress.Text = "Done!";
			}

			if (OnWorkerCompleted != null)
			{
				OnWorkerCompleted();
			}
		}
	}

	class WorkerArgs
	{
		public Note[] Notes;
		public Pitch[] Pitches;
		public OutputDevice OutDevice;
	}
}
