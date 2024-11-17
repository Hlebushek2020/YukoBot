using System;
using System.Windows.Input;

namespace YukoClientBase.MVVM
{
    public class DelegateCommand<T> : DelegateCommandBase
    {
        private readonly Action<T> _executeMethod;
        private readonly Func<T, bool> _canExecuteMethod;

        /// <summary>
        /// Initializes a new instance of <see cref="Prism.Commands.DelegateCommand{T}"/>.
        /// </summary>
        /// <param name="executeMethod">
        /// Delegate to execute when Execute is called on the command. This can be null to just hook up a CanExecute
        /// delegate
        /// .</param>
        /// <remarks><see cref="CanExecute(T)"/> will always return true.</remarks>
        public DelegateCommand(Action<T> executeMethod) : this(executeMethod, (o) => true) { }

        /// <summary>
        /// Initializes a new instance of <see cref="Prism.Commands.DelegateCommand{T}"/>.
        /// </summary>
        /// <param name="executeMethod">
        /// Delegate to execute when Execute is called on the command. This can be null to just hook up a CanExecute
        /// delegate.
        /// </param>
        /// <param name="canExecuteMethod">
        /// Delegate to execute when CanExecute is called on the command. This can be null.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// When both <paramref name="executeMethod"/> and <paramref name="canExecuteMethod"/> are <see langword="null" />.
        /// </exception>
        public DelegateCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod)
        {
            _executeMethod = executeMethod ?? throw new ArgumentNullException(nameof(executeMethod));
            _canExecuteMethod = canExecuteMethod ?? throw new ArgumentNullException(nameof(canExecuteMethod));
        }

        ///<summary>
        ///Executes the command and invokes the <see cref="Action{T}"/> provided during construction.
        ///</summary>
        ///<param name="parameter">Data used by the command.</param>
        public void Execute(T parameter) { _executeMethod(parameter); }

        ///<summary>
        ///Determines if the command can execute by invoked the <see cref="Func{T,Bool}"/> provided during construction.
        ///</summary>
        ///<param name="parameter">Data used by the command to determine if it can execute.</param>
        ///<returns>
        ///<see langword="true" /> if this command can be executed; otherwise, <see langword="false" />.
        ///</returns>
        public bool CanExecute(T parameter) { return _canExecuteMethod(parameter); }

        /// <summary>
        /// Handle the internal invocation of <see cref="ICommand.Execute(object)"/>
        /// </summary>
        /// <param name="parameter">Command Parameter</param>
        public override void Execute(object parameter) { Execute((T)parameter); }

        /// <summary>
        /// Handle the internal invocation of <see cref="ICommand.CanExecute(object)"/>
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns><see langword="true"/> if the Command Can Execute, otherwise <see langword="false" /></returns>
        public override bool CanExecute(object parameter) { return CanExecute((T)parameter); }
    }
}