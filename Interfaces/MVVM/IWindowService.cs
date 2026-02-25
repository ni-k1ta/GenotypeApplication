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
        Window ShowDialogWindow<TWindow, TViewModel>(TViewModel viewModel) where TWindow : Window, new();
    }
}
