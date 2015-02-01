using alphatab.importer;
using alphatab.model;
using Library.Compounds;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

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

		private readonly RelayCommand _showScoreInfoCommand;

		/// <summary>
		/// A command which raises a file opening
		/// </summary>
		public ICommand OpenFileCommand { get; private set; }

		/// <summary>
		/// Gets or sets the currently opened score. 
		/// If a new score is selected, the first track gets loaded.
		/// </summary>
		private Score _score;
		public Score Score
		{
			get { return _score; }
			set
			{
				_score = value;
				OnPropertyChanged("ScoreTitle");
				//_showScoreInfoCommand.RaiseCanExecuteChanged();

				// select the first track
				CurrentTrackIndex = 0;
			}
		}


		/// <summary>
		/// Gets or sets the index of the track which should be currently displayed.
		/// </summary>
		private int _currentTrackIndex;
		public int CurrentTrackIndex
		{
			get { return _currentTrackIndex; }
			set
			{
				_currentTrackIndex = value;

				// update the visual track selection if a new track is selected
				// UpdateSelectedViewModel();

				// notify the ui
				OnPropertyChanged("CurrentTrack");
			}
		}

		/// <summary>
		/// Gets the currently selected track. 
		/// </summary>
		public Track CurrentTrack
		{
			get
			{
				if (Score == null || CurrentTrackIndex < 0 || CurrentTrackIndex >= _score.tracks.length) return null;
				return (Track)_score.tracks[_currentTrackIndex];
			}
		}

		public MainWindowViewModel()
		{
 			OpenFileCommand = new RelayCommand(OpenFile);
			//_showScoreInfoCommand = new RelayCommand(ShowScoreInfo, () => _score != null);
		}

		public void OpenFile()
		{
			var dlg = new OpenFileDialog();
			var res = dlg.ShowDialog();
			if (!res.Value) return;

			var file = dlg.FileName;

			try
			{
				// load the score from the filesystem
				Score = ScoreLoader.loadScore(file);

				//var tmpTrack = new Track();
				//var bar = new Bar();
				//bar.clef = Clef.G2;
				//var tmpScore = new Score ();
				

				//// build the track info objects for the ui
				//TrackViewModel[] trackInfos = new TrackViewModel[Score.tracks.length];
				//for (int i = 0; i < trackInfos.Length; i++)
				//{
				//	trackInfos[i] = new TrackViewModel((Track)Score.tracks[i]);
				//}
				//TrackInfos = trackInfos;
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
				//_errorService.OpenFailed(e);
			}
		}
	}
}
