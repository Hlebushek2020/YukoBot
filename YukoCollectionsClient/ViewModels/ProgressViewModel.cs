using Prism.Mvvm;
using YukoClientBase.Interfaces;
using YukoCollectionsClient.Models.Progress;

namespace YukoCollectionsClient.ViewModels
{
    public class ProgressViewModel : BindableBase, IViewTitle
    {
        #region Fields
        private Base model;
        #endregion

        #region Propirties
        public string Title { get => App.Name; }
        public string State { get => model.State; }
        public int Value { get => model.Value; }
        public int MaxValue { get => model.MaxValue; }
        public bool IsIndeterminate { get => model.IsIndeterminate; }
        #endregion

        public ProgressViewModel(Base model)
        {
            this.model = model;
            this.model.PropertyChanged += (s, e) => RaisePropertyChanged(e.PropertyName);
        }
    }
}
