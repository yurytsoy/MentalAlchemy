using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Library.Compounds
{
	/// <summary>
	/// This command implementation delegates the execution to a referenced method. 
	/// </summary>
	public class RelayCommand : ICommand
	{
		private readonly Action _action;
		private readonly Func<bool> _canExecute;

		public RelayCommand(Action action, Func<bool> canExecute = null)
		{
			_action = action;
			_canExecute = canExecute;
		}

		public bool CanExecute(object parameter)
		{
			return _canExecute == null || _canExecute();
		}

		public void Execute(object parameter)
		{
			_action();
		}

		public event EventHandler CanExecuteChanged;
		public virtual void RaiseCanExecuteChanged()
		{
			EventHandler handler = CanExecuteChanged;
			if (handler != null) handler(this, EventArgs.Empty);
		}
	}
}
