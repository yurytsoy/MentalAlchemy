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
		int _musicLength = 100;
		public int CompositionLength
		{
			get { return _musicLength; }
			set
			{
				_musicLength = value;
				NotifyPropertyChanged("MusicLength");
			}
		}

		public MainWindow()
		{
			InitializeComponent();

			this.DataContext = this;
		}

		private void PlayBtn_Click(object sender, RoutedEventArgs e)
		{
			// TODO:
		}

		private void GenerateBtn_Click(object sender, RoutedEventArgs e)
		{
			//
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public void NotifyPropertyChanged(string propName)
		{
			if (this.PropertyChanged != null)
				this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
		}
	}
}
