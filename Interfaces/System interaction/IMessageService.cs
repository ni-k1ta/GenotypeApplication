using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GenotypeApplication.Interfaces
{
    public interface IMessageService
    {
        void ShowError(string message);
        void ShowWarning(string message);
        bool ShowQuetion(string quetion);
        void ShowInformation(string message);
    }
}
