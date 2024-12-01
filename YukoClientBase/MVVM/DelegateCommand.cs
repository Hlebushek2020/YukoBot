using System;
using System.Windows.Input;

namespace YukoClientBase.MVVM
{
    public class DelegateCommand : DelegateCommandBase
    {
        private readonly Action _executeMethod;
        private readonly Func<bool> _canExecuteMethod;

        /// <summary>
        /// Creates a new instance of <see cref="DelegateCommand"/> with the <see cref="Action"/> to invoke on execution.
        /// </summary>
        /// <param name="executeMethod">
        /// The <see cref="Action"/> to invoke when <see cref="ICommand.Execute(object)"/> is called.
        /// </param>
        public DelegateCommand(Action executeMethod) : this(executeMethod, () => true) { }

        /// <summary>
        /// Creates a new instance of <see cref="DelegateCommand"/> with the <see cref="Action"/> to invoke on execution
        /// and a <see langword="Func" /> to query for determining if the command can execute.
        /// </summary>
        /// <param name="executeMethod">
        /// The <see cref="Action"/> to invoke when <see cref="ICommand.Execute"/> is called.
        /// </param>
        /// <param name="canExecuteMethod">
        /// The <see cref="Func{TResult}"/> to invoke when <see cref="ICommand.CanExecute"/> is called
        /// </param>
        public DelegateCommand(Action executeMethod, Func<bool> canExecuteMethod)
        {
            _executeMethod = executeMethod ?? throw new ArgumentNullException(nameof(executeMethod));
            _canExecuteMethod = canExecuteMethod ?? throw new ArgumentNullException(nameof(canExecuteMethod));
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        public void Execute() { _executeMethod(); }

        /// <summary>
        /// Determines if the command can be executed.
        /// </summary>
        /// <returns>Returns <see langword="true"/> if the command can execute,otherwise returns <see langword="false"/>.</returns>
        public bool CanExecute() { return _canExecuteMethod(); }

        /// <summary>
        /// Handle the internal invocation of <see cref="ICommand.Execute(object)"/>
        /// </summary>
        /// <param name="parameter">Command Parameter</param>
        public override void Execute(object parameter) { Execute(); }

        /// <summary>
        /// Handle the internal invocation of <see cref="ICommand.CanExecute(object)"/>
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns><see langword="true"/> if the Command Can Execute, otherwise <see langword="false" /></returns>
        public override bool CanExecute(object parameter) { return CanExecute(); }
    }
}