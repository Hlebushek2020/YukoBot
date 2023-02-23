using Prism.Mvvm;
using YukoClient.Models.Progress;
using YukoClientBase.Interfaces;
using YukoClientBase.Models.Progresses;

namespace YukoClient.ViewModels
{
    public class ProgressViewModel : BindableBase, IViewTitle
    {
        #region Fields
        private BaseProgressModel model;
        #endregion

        #region Propirties
        public string Title { get => App.Name; }
        public string State { get => model.State; }
        public int Value { get => model.Value; }
        public int MaxValue { get => model.MaxValue; }
        public bool IsIndeterminate { get => model.IsIndeterminate; }
        #endregion

        public ProgressViewModel(BaseProgressModel model)
        {
            this.model = model;
            this.model.PropertyChanged += (s, e) => RaisePropertyChanged(e.PropertyName);
        }
    }
}
