using System.Threading;
using System.Windows.Threading;
using Prism.Mvvm;
using YukoClientBase.Interfaces;
using YukoClientBase.Models.Progresses;

namespace YukoClientBase.ViewModels
{
    public class ProgressViewModel : BindableBase
    {
        private readonly BaseProgressModel _model;

        private bool _cancelButtonIsEnabled;

        #region Propirties
        public string Title { get; }
        public bool IsCancellable { get; }

        public bool CancelButtonIsEnabled
        {
            get => _cancelButtonIsEnabled;
            private set
            {
                _cancelButtonIsEnabled = value;
                RaisePropertyChanged();
            }
        }

        public string State => _model.State;
        public int Value => _model.Value;
        public int MaxValue => _model.MaxValue;
        public bool IsIndeterminate => _model.IsIndeterminate;
        #endregion

        public ProgressViewModel(string title, BaseProgressModel model, bool isCancellable)
        {
            Title = title;
            IsCancellable = isCancellable;

            _model = model;
            _model.PropertyChanged += (s, e) => RaisePropertyChanged(e.PropertyName);
        }

        public void Run(Dispatcher dispatcher, CancellationToken cancellationToken) =>
            _model.Run(dispatcher, cancellationToken);

        public void CancellationRequested() => CancelButtonIsEnabled = true;
    }
}