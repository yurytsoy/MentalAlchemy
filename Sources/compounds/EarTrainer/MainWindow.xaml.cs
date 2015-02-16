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
		public static RoutedCommand GenerateCommand = new RoutedCommand();
		MainWindowViewModel _vm;

		public MainWindow()
		{
			InitializeComponent();

			_vm = new MainWindowViewModel();
			DataContext = _vm;

			RoutedCommand generateCmdBinding = new RoutedCommand();
			generateCmdBinding.InputGestures.Add(new KeyGesture(Key.G, ModifierKeys.Control));
			CommandBindings.Add(new CommandBinding(generateCmdBinding, Generate));

			RoutedCommand compCmdBinding = new RoutedCommand();
			compCmdBinding.InputGestures.Add(new KeyGesture(Key.M, ModifierKeys.Control));
			CommandBindings.Add(new CommandBinding(compCmdBinding, Compare));
			
			RoutedCommand playCmdBinding = new RoutedCommand();
			playCmdBinding.InputGestures.Add(new KeyGesture(Key.P, ModifierKeys.Control));
			playCmdBinding.InputGestures.Add(new KeyGesture(Key.Space, ModifierKeys.Control));
			CommandBindings.Add(new CommandBinding(playCmdBinding, Play));

			RoutedCommand showCmdBinding = new RoutedCommand();
			showCmdBinding.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
			CommandBindings.Add(new CommandBinding(showCmdBinding, Show));
		}

		private void Generate( object sender, ExecutedRoutedEventArgs e ) 
		{
			_vm.GenerateNotes();
		}

		private void Compare(object sender, ExecutedRoutedEventArgs e)
		{
			_vm.CompareNotes();
		}

		private void Play(object sender, ExecutedRoutedEventArgs e)
		{
			_vm.PlayNotes();
		}

		private void Show(object sender, ExecutedRoutedEventArgs e)
		{
			_vm.ShowGeneratedNotes = !_vm.ShowGeneratedNotes;
		}

		private void OnRenderFinished(object sender, RoutedEventArgs e)
		{
		}
	}
}
