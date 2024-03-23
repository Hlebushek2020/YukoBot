using System;
using System.Collections.Generic;
using Prism.Commands;
using Prism.Mvvm;
using YukoClient.Models;

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