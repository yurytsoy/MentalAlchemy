﻿using alphatab;
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

namespace EarTrainer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			DataContext = new MainWindowViewModel();

		}

		private void OnRenderFinished(object sender, RoutedEventArgs e)
		{
			// TODO
			//var sets = TabCanvas.Settings;
			////sets.
			//var st = TabCanvas.Settings.staves;
			//var tab = (StaveSettings)st.__a[18];
			//MessageBox.Show(sets.engine);
			//tab.]

			//global::haxe.lang.Runtime.toString();
			//var st2 = tab;
		}
	}
}
