using Prism.Mvvm;
using System.Windows.Threading;

namespace YukoClient.Models.Progress
{
    public class Base : BindableBase
    {
        #region Field
        private string state = string.Empty;
        private int value = 0;
        private int maxValue = 1;
        private bool isIndeterminate = true;
        #endregion

        #region Propirties
        public string State
        {
            get { return state; }
            set
            {
                state = value;
                RaisePropertyChanged();
            }
        }
        public int Value
        {
            get { return value; }
            set
            {
                this.value = value;
                RaisePropertyChanged();
            }
        }
        public int MaxValue
        {
            get { return maxValue; }
            set
            {
                maxValue = value;
                IsIndeterminate = maxValue < 1;
                if (!isIndeterminate)
                {
                    RaisePropertyChanged();
                }
            }
        }
        public bool IsIndeterminate
        {
            get { return isIndeterminate; }
            set
            {
                isIndeterminate = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        public virtual void Run(Dispatcher dispatcher) { }
    }
}