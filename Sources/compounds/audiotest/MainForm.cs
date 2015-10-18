using Accord.Audio;
using Accord.DirectSound;
using MentalAlchemy.Atoms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace audiotest
{
	public partial class MainForm : Form
	{
		IAudioSource source;
		AudioDeviceInfo primaryDevice;
		int frameCount = 0;
		float[] current;

		public MainForm()
		{
			InitializeComponent();

			// init device.
			AudioDeviceCollection audioDevices = new AudioDeviceCollection(AudioDeviceCategory.Capture);
			primaryDevice = (audioDevices.Count() > 0) ? audioDevices.ElementAt(0) : null;
		}

		/// <summary>
		/// Based on the code from:
		/// - http://accord-framework.net/docs/html/T_Accord_DirectSound_AudioCaptureDevice.htm
		/// - FFT sample from the Accord.NET library.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void RecordBtn_Click(object sender, EventArgs e)
		{
			if (primaryDevice == null) return;

			// Create capture device
			source = new AudioCaptureDevice(primaryDevice)
			{
				// Listen on 22050 Hz
				DesiredFrameSize = 1024,	// affects the window size for the signal analysis.
				SampleRate = 22050,

				// We will be reading 16-bit PCM
				//Format = SampleFormat.Format16Bit
			};

			// Wire up some events
			source.NewFrame += source_NewFrame;
			//source.AudioSourceError += source_AudioSourceError;

			// Create buffer for wavechart control
			current = new float[source.DesiredFrameSize];

			//// Create stream to store file
			//stream = new MemoryStream();
			//encoder = new WaveEncoder(stream);

			// Start
			source.Start();

			frameCount = 0;
		}

		/// <summary>
		///   This method will be called whenever there is a new audio
		///   frame to be processed.
		/// </summary>
		/// 
		void source_NewFrame(object sender, NewFrameEventArgs eventArgs)
		{
			// We can start by converting the audio frame to a complex signal
			ComplexSignal signal = ComplexSignal.FromSignal(eventArgs.Signal);

			// If its needed,
			//if (window != null)
			//{
			//	// Apply the chosen audio window
			//	signal = window.Apply(signal, 0);
			//}

			// Transform to the complex domain
			signal.ForwardFourierTransform();

			// Now we can get the power spectrum output and its
			// related frequency vector to plot our spectrometer.

			Complex[] channel = signal.GetChannel(0);
			double[] power = Accord.Audio.Tools.GetPowerSpectrum(channel);
			double[] freqv = Accord.Audio.Tools.GetFrequencyVector(signal.Length, signal.SampleRate);

			power[0] = 0; // zero DC.
			float[] g = new float[power.Length];
			var lines = new List<string>();
			var maxPowerIdx = VectorMath.IndexOfMax(power);
			lines.Add(DateTime.Now.ToLongTimeString() + ": " + freqv[maxPowerIdx]);
			for (int i = 0; i < power.Length; i++)
			{
				g[i] = (float)power[i];
			}

			BeginInvoke(new Action(() =>
			{
				//OutputList.Items.AddRange(lines.ToArray());

				var strNotes = Utils.GetNote(power, freqv);
				OutputList.Items.Add(DateTime.Now.ToLongTimeString() + ": " + strNotes);
			}));
			//if (InvokeRequired)
			//{
			//	BeginInvoke(new Action( () =>
			//	{
			//		OutputList.Items.AddRange(lines.ToArray ());
			//	}));
			//}
			//else
			//{
			//	OutputList.Items.AddRange(lines.ToArray());
			//}
		}

		private void StopBtn_Click(object sender, EventArgs e)
		{
			if (source != null ) { source.SignalToStop(); }
		}
	}
}
