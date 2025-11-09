using System;
using System.Windows.Input;

namespace HerculesSimulation.Cores
{
    public class RelayCommand : ICommand
    {
        private Action<object> _execute;
        private Func<object, bool> _canExecute;
        //Khong nen dung CommandManager de kiem soat thu cong -> tot cho hieu nang
        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            this._execute = execute;
            this._canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        //Ham dung de bat view kiem tra lai trang thai CanExecute
        public void RaiseCanExecuteChanged()
        {
            // Nen chay tren UI thread
            App.Current.Dispatcher.Invoke(() =>
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            });
        }
    }
}
