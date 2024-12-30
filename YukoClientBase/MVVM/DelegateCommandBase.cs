using System;
using System.Threading;
using System.Windows.Input;

namespace YukoClientBase.MVVM
{
    public abstract class DelegateCommandBase : ICommand
    {
        private readonly SynchronizationContext _synchronizationContext;

        public event EventHandler CanExecuteChanged;

        protected DelegateCommandBase() { _synchronizationContext = SynchronizationContext.Current; }

        protected virtual void OnCanExecuteChanged()
        {
            EventHandler handler = CanExecuteChanged;

            if (handler == null)
                return;

            if (_synchronizationContext != null && _synchronizationContext != SynchronizationContext.Current)
                _synchronizationContext.Post((o) => handler.Invoke(this, EventArgs.Empty), null);
            else
                handler.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises <see cref="CanExecuteChanged"/> so every command invoker can requery to check if the command
        /// can execute.
        /// </summary>
        /// <remarks>
        /// Note that this will trigger the execution of <see cref="CanExecuteChanged"/> once for each invoker.
        /// </remarks>
        public void RaiseCanExecuteChanged() { OnCanExecuteChanged(); }

        public virtual bool CanExecute(object parameter) { throw new NotImplementedException(); }

        public virtual void Execute(object parameter) { throw new NotImplementedException(); }
    }
}