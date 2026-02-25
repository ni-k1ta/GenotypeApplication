using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenotypeApplication.Models.Structure
{
    public class StructureMainParametersModel
    {
        private int _burnIn;
        public int BURNIN
        {
            get => _burnIn; set => _burnIn = value;
        }

        private int _numReps;
        public int NumReps
        {
            get => _numReps; set => _numReps = value;
        }

        private bool _markovPhase;
        public bool MARKOVPHASE
        {
            get => _markovPhase; set => _markovPhase = value;
        }

        private bool _phased;
        public bool PHASED
        {
            get => _phased; set => _phased = value;
        }
    }
}
