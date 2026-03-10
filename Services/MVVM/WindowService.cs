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

        public void CloseDialogWindow(Window window, bool dialogResult = false)
        {
            if (window != null && window.IsActive)
            {
                window.DialogResult = dialogResult;
                //window.Close();
            }
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

        public bool? ShowDialogWindow<TWindow, TViewModel>(TViewModel viewModel) where TWindow : Window, new()
        {
            var window = new TWindow
            {
                DataContext = viewModel
            };

            if (viewModel is IWindowAware aware) aware.SetCurrentWindow(window);

            return window.ShowDialog();
        }
    }
}
