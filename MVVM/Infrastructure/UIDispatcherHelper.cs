using System.Windows;
using System.Windows.Threading;

namespace GenotypeApplication.MVVM.Infrastructure
{
    public static class UIDispatcherHelper
    {
        public static void RunOnUI(Action action)
        {
            if (Application.Current.Dispatcher.CheckAccess())
                action();
            else
                Application.Current.Dispatcher.BeginInvoke(action, DispatcherPriority.Render);
        }
    }
}
