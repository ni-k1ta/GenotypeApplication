using System.Windows;

namespace GenotypeApplication.Interfaces.MVVM
{
    public interface IWindowAware
    {
        void SetCurrentWindow(Window window);
    }
}
