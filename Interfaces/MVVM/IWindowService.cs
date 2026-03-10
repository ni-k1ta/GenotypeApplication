using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GenotypeApplication.Interfaces.MVVM
{
    public interface IWindowService
    {
        Window ShowWindow<TWindow, TViewModel>(TViewModel viewModel) where TWindow : Window, new();
        void CloseWindow(Window window);
        bool? ShowDialogWindow<TWindow, TViewModel>(TViewModel viewModel) where TWindow : Window, new();
        void CloseDialogWindow(Window window, bool dialogResult = false);
    }
}
