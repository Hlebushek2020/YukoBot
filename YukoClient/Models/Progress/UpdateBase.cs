using System.Windows.Threading;

namespace YukoClient.Models.Progress
{
    public class UpdateBase : Base
    {
        public override void Run(Dispatcher dispatcher)
        {
            dispatcher.Invoke(() => State = "Сохранение изменений");
            Storage.Current.Save();
        }
    }
}
