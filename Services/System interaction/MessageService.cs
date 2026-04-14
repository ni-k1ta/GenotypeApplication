using GenotypeApplication.Interfaces;
using System.Windows;

namespace GenotypeApplication.Services
{
    internal class MessageService : IMessageService
    {
        public void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        public void ShowWarning(string message)
        {
            MessageBox.Show(message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        public bool ShowQuetion(string quetion)
        {
            if (MessageBox.Show(quetion, "Quetion", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                return true;
            }
            return false;
        }
        public void ShowInformation(string message)
        {
            MessageBox.Show(message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        public bool? ShowExpandedQuetion(string quetion)
        {
            var result = MessageBox.Show(quetion, "Quetion", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes) return true;
            else if (result == MessageBoxResult.No) return false;
            else return null;
        }
    }
}
