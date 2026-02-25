using GenotypeApplication.Interfaces.MVVM;
using System.Windows;

namespace GenotypeApplication.Services.MVVM
{
    public class WindowService : IWindowService
    {
        public void CloseWindow(Window window)
        {
            window?.Close();
        }   

        public Window ShowWindow<TWindow, TViewModel>(TViewModel viewModel) where TWindow : Window, new()
        {
            var window = new TWindow
            {
                DataContext = viewModel
            };
            window.Show();

            return window;
        }

        public Window ShowDialogWindow<TWindow, TViewModel>(TViewModel viewModel) where TWindow : Window, new()
        {
            var window = new TWindow
            {
                DataContext = viewModel
            };
            window.ShowDialog();

            return window;
        }
    }
}
