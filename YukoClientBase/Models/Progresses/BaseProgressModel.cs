using System.Threading;
using System.Windows.Threading;
using Prism.Mvvm;

namespace YukoClientBase.Models.Progresses
{
    public class BaseProgressModel : BindableBase
    {
        #region Field
        private string _state = string.Empty;
        private int _value = 0;
        private int _maxValue = 1;
        private bool _isIndeterminate = true;
        #endregion

        #region Propirties
        public string State
        {
            get { return _state; }
            set
            {
                _state = value;
                RaisePropertyChanged();
            }
        }
        public int Value
        {
            get { return _value; }
            set
            {
                this._value = value;
                RaisePropertyChanged();
            }
        }
        public int MaxValue
        {
            get { return _maxValue; }
            set
            {
                _maxValue = value;
                IsIndeterminate = _maxValue < 1;
                if (!_isIndeterminate)
                {
                    RaisePropertyChanged();
                }
            }
        }
        public bool IsIndeterminate
        {
            get { return _isIndeterminate; }
            set
            {
                _isIndeterminate = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        public virtual void Run(Dispatcher dispatcher, CancellationToken cancellationToken)
        {
        }
    }
}