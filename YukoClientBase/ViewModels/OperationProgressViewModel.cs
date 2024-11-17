using System.Threading.Tasks;
using System.Windows;
using YukoClientBase.Models.Operations;
using MessageBox = YukoClientBase.Dialogs.MessageBox;

namespace YukoClientBase.ViewModels
{
    public class OperationProgressViewModel : OperationProgressViewModelBase
    {
        private readonly IOperation _operation;

        public OperationProgressViewModel(string title, IOperation operation, bool isCancellable = true)
            : base(isCancellable)
        {
            Title = title;
            _operation = operation;
        }

        public override Task Operation() => _operation.Run(this, CancellationTokenSource.Token);

        public override MessageBoxResult WindowClosingConfirmation() =>
            IsCancellable
                ? MessageBox.Show(Title, "Вы действительно хотите отменить операцию?", MessageBoxButton.YesNo,
                    MessageBoxImage.Question)
                : MessageBoxResult.No;
    }
}