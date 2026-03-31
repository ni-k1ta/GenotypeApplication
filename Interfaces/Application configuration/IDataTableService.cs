using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenotypeApplication.Interfaces
{
    public interface IDataTableService
    {
        DataTable Parse(string filePath);
    }
}
