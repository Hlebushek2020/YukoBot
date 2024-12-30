using System;
using System.Collections.Generic;
using YukoClient.Models;
using YukoClientBase.MVVM;

namespace YukoClient.ViewModels
{
    public class ExecutionErrorsViewModel : BindableBase
    {
        public string Title => App.Name;
        public IList<string> Errors { get; }

        public DelegateCommand CloseWindowCommand { get; }

        public ExecutionErrorsViewModel(Action close, Script script)
        {
            Errors = script.Errors;
            CloseWindowCommand = new DelegateCommand(close);
        }
    }
}