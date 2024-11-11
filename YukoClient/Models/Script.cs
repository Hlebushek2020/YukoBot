using Prism.Mvvm;
using System.Collections.Generic;

namespace YukoClient.Models
{
    public class Script : BindableBase
    {
        private bool? _completedWithErrors;

        #region Propirties
        public Channel Channel { get; set; }
        public DisplayScriptMode Mode { get; set; }
        public ulong MessageId { get; set; }
        public int Count { get; set; }
        public IList<string> Errors { get; set; } = new List<string>();

        public bool? CompletedWithErrors
        {
            get => _completedWithErrors;
            set
            {
                _completedWithErrors = value;
                RaisePropertyChanged();
            }
        }
        #endregion
    }
}