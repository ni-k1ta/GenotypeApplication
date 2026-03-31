using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenotypeApplication.Models.Structure_Harvester
{
    public class EvannoParametersModel
    {
        public int K { get; set; }
        //public int Reps { get; set; }
        public double MeanLnPK { get; set; }
        public double StdevLnPK { get; set; }
        public double? LnPrimeK { get; set; }
        public double? LnDoublePrimeK { get; set; }
        public double? DeltaK { get; set; }
    }
}
