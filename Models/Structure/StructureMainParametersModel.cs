using GenotypeApplication.Constants;

namespace GenotypeApplication.Models.Structure
{
    public class StructureMainParametersModel : IEquatable<StructureMainParametersModel>
    {
        private string _infile = string.Empty;
        [DefineParameterModel("INFILE")]
        public string INFILE
        {
            get => _infile; set => _infile = value;
        }

        private const string _outfile = StructureConstants.STRUCTURE_OUTPUT_FILE_DEFAULT_NAME; //const "outfile_"
        [DefineParameterModel("OUTFILE")]
        public string OUTFILE => _outfile;

        private int _burnIn = 10_000;
        [DefineParameterModel("BURNIN")]
        public int BURNIN
        {
            get => _burnIn; set => _burnIn = value;
        }

        private int _numReps = 10_000;
        [DefineParameterModel("NUMREPS")]
        public int NumReps
        {
            get => _numReps; set => _numReps = value;
        }

        private bool _markovPhase;
        [DefineParameterModel("MARKOVPHASE")]
        public bool MARKOVPHASE
        {
            get => _markovPhase; set => _markovPhase = value;
        }

        private bool _phased;
        [DefineParameterModel("PHASED")]
        public bool PHASED
        {
            get => _phased; set => _phased = value;
        }

        public bool Equals(StructureMainParametersModel? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return INFILE == other.INFILE &&
                   OUTFILE == other.OUTFILE &&
                   BURNIN == other.BURNIN &&
                   NumReps == other.NumReps &&
                   MARKOVPHASE == other.MARKOVPHASE &&
                   PHASED == other.PHASED;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as StructureMainParametersModel);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(INFILE, OUTFILE, BURNIN, NumReps, MARKOVPHASE, PHASED);
        }

        public static bool operator ==(StructureMainParametersModel? left, StructureMainParametersModel? right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        public static bool operator !=(StructureMainParametersModel? left, StructureMainParametersModel? right)
        {
            return !(left == right);
        }
    }
}