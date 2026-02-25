using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenotypeApplication.Interfaces
{
    public interface IDialogService
    {
        string SelectFolder(string initialDirectory);
        string SelectFile(string initialDirectory);
    }
}
