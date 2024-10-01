using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using YukoClientBase.Args;

namespace YukoClientBase.ViewModels
{
    public abstract class OperationProgressViewModel : BindableBase, IProgress<ProgressReportArgs>
    {
        #region Fields
        protected readonly CancellationTokenSource CancellationTokenSource;
        protected SynchronizationContext SynchronizationContext;

        private bool _isIndeterminate;
        private double _minimum;
        private double _maximum;
        private double _value;
        private string _text;
        #endregion

        #region Properties
        public string Title { get; protected set; }

        public bool IsCancellable { get; }

        public bool IsIndeterminate
        {
            get => _isIndeterminate;
            private set
            {
                _isIndeterminate = value;
                RaisePropertyChanged();
            }
        }

        public double Minimum
        {
            get => _minimum;
            private set
            {
                _minimum = value;
                RaisePropertyChanged();
            }
        }

        public double Maximum
        {
            get => _maximum;
            private set
            {
                _maximum = value;
                RaisePropertyChanged();
            }
        }

        public double Value
        {
            get => _value;
            private set
            {
                _value = value;
                RaisePropertyChanged();
            }
        }

        public string Text
        {
            get => _text;
            private set
            {
                _text = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        public DelegateCommand CancelCommand { get; }

        protected OperationProgressViewModel(bool isCancellable = true)
        {
            IsCancellable = isCancellable;

            CancellationTokenSource = new CancellationTokenSource();
            SynchronizationContext = SynchronizationContext.Current;

            CancelCommand = new DelegateCommand(() =>
            {
                if (!CancellationTokenSource.IsCancellationRequested)
                    CancellationTokenSource.Cancel();
            });
        }

        public virtual void Report(ProgressReportArgs value)
        {
            SynchronizationContext.Send(
                state =>
                {
                    ProgressReportArgs args = (ProgressReportArgs)state;

                    if (args.IsIndeterminate != null)
                        IsIndeterminate = args.IsIndeterminate.Value;

                    if (args.Text != null)
                        Text = args.Text;

                    if (args.Maximum != null)
                        Maximum = args.Maximum.Value;

                    if (args.Minimum != null)
                        Minimum = args.Minimum.Value;

                    if (args.Value != null)
                        Value = args.Value.Value;
                },
                value);
        }

        public abstract Task Operation();
        public abstract MessageBoxResult WindowClosingConfirmation();
    }
}