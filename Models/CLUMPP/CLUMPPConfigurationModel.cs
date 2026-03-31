using GenotypeApplication.Models.Structure;
using GenotypeApplication.MVVM.Infrastructure;

namespace GenotypeApplication.Models.CLUMPP
{
    public class CLUMPPConfigurationModel : ViewModelBase/*, IEquatable<CLUMPPConfigurationModel>*/
    {
        private string _parametersName = string.Empty;
        public string ParametersName
        {
            get => _parametersName;
            set
            {
                SetField(ref _parametersName, value);
            }
        }

        private bool _dataType;
        [DefineParameterModel("DATATYPE")]
        public bool DATATYPE
        {
            get => _dataType;
            set => _dataType = value;
        }

        private const string _indFile = "K.indfile";
        [DefineParameterModel("INDFILE")]
        public string INDFILE => _indFile;

        private const string _popFile = "K.popfile";
        [DefineParameterModel("POPFILE")]
        public string POPFILE => _popFile;

        private const string _outFile = "K.indivq";
        [DefineParameterModel("OUTFILE")]
        public string OUTFILE => _outFile;

        private const string _miscFile = "K.miscfile";
        [DefineParameterModel("MISCFILE")]
        public string MISCFILE => _miscFile;

        private const int _k = 1;
        [DefineParameterModel("K")]
        public int K => _k;

        private int _c = 0;
        [DefineParameterModel("C")]
        public int C
        {
            get => _c;
            set => _c = value;
        }

        private int _r = 0;
        [DefineParameterModel("R")]
        public int R
        {
            get => _r;
            set => _r = value;
        }

        private int _m = 0;
        [DefineParameterModel("M")]
        public int M
        {
            get => _m;
            set => _m = value;
        }

        private bool _w;
        [DefineParameterModel("W")]
        public bool W
        {
            get => _w;
            set => _w = value;
        }

        private int _s = 0;
        [DefineParameterModel("S")]
        public int S
        {
            get => _s;
            set => _s = value;
        }

        private int _greedyOption = 0;
        [DefineParameterModel("GREEDY_OPTION")]
        public int GREEDY_OPTION
        {
            get => _greedyOption;
            set => _greedyOption = value;
        }

        private int _repeats = 0;
        [DefineParameterModel("REPEATS")]
        public int REPEATS
        {
            get => _repeats;
            set => _repeats = value;
        }

        private string _permutationFile = string.Empty;
        [DefineParameterModel("PERMUTATIONFILE")]
        public string PERMUTATIONFILE
        {
            get => _permutationFile;
            set => _permutationFile = value;
        }

        private int _printPermutedData = 0;
        [DefineParameterModel("PRINT_PERMUTED_DATA")]
        public int PRINT_PERMUTED_DATA
        {
            get => _printPermutedData;
            set => _printPermutedData = value;
        }

        private const string _permutedDataFile = "perm.perm_datafile";
        [DefineParameterModel("PERMUTED_DATAFILE")]
        public string PERMUTED_DATAFILE => _permutedDataFile;

        private bool _printEveryPerm;
        [DefineParameterModel("PRINT_EVERY_PERM")]
        public bool PRINT_EVERY_PERM
        {
            get => _printEveryPerm;
            set => _printEveryPerm = value;
        }

        private const string _everyPermFile = "everyperm.every_permfile";
        [DefineParameterModel("EVERY_PERMFILE")]
        public string EVERY_PERMFILE => _everyPermFile;

        private bool _printRandomInputorder;
        [DefineParameterModel("PRINT_RANDOM_INPUTORDER")]
        public bool PRINT_RANDOM_INPUTORDER
        {
            get => _printRandomInputorder;
            set => _printRandomInputorder = value;
        }

        private const string _randomInputorderFile = "randomorder.random_inputorderfile";
        [DefineParameterModel("RANDOM_INPUTORDERFILE")]
        public string RANDOM_INPUTORDERFILE => _randomInputorderFile;

        private bool _overrideWarnings;
        [DefineParameterModel("OVERRIDE_WARNINGS")]
        public bool OVERRIDE_WARNINGS
        {
            get => _overrideWarnings;
            set => _overrideWarnings = value;
        }

        private int _orderByRun = 0;
        [DefineParameterModel("ORDER_BY_RUN")]
        public int ORDER_BY_RUN
        {
            get => _orderByRun;
            set => _orderByRun = value;
        }

        //public bool Equals(CLUMPPConfigurationModel? other)
        //{
        //    if (other is null) return false;
        //    if (ReferenceEquals(this, other)) return true;
        //    return DATATYPE == other.DATATYPE &&
        //           INDFILE == other.INDFILE &&
        //           POPFILE == other.POPFILE &&
        //           OUTFILE == other.OUTFILE &&
        //           MISCFILE == other.MISCFILE &&
        //           K == other.K &&
        //           C == other.C &&
        //           R == other.R &&
        //           M == other.M &&
        //           W == other.W &&
        //           S == other.S &&
        //           GREEDY_OPTION == other.GREEDY_OPTION &&
        //           REPEATS == other.REPEATS &&
        //           PERMUTATIONFILE == other.PERMUTATIONFILE &&
        //           PRINT_PERMUTED_DATA == other.PRINT_PERMUTED_DATA &&
        //           PERMUTED_DATAFILE == other.PERMUTED_DATAFILE &&
        //           PRINT_EVERY_PERM == other.PRINT_EVERY_PERM &&
        //           EVERY_PERMFILE == other.EVERY_PERMFILE &&
        //           PRINT_RANDOM_INPUTORDER == other.PRINT_RANDOM_INPUTORDER &&
        //           RANDOM_INPUTORDERFILE == other.RANDOM_INPUTORDERFILE &&
        //           OVERRIDE_WARNINGS == other.OVERRIDE_WARNINGS &&
        //           ORDER_BY_RUN == other.ORDER_BY_RUN;
        //}
        //public override bool Equals(object? obj)
        //{
        //    return Equals(obj as CLUMPPConfigurationModel);
        //}

        //public override int GetHashCode()
        //{
        //    var hash = new HashCode();

        //    hash.Add(DATATYPE);
        //    hash.Add(INDFILE);
        //    hash.Add(POPFILE);
        //    hash.Add(OUTFILE);
        //    hash.Add(MISCFILE);
        //    hash.Add(K);
        //    hash.Add(C);
        //    hash.Add(R);
        //    hash.Add(M);
        //    hash.Add(W);
        //    hash.Add(S);
        //    hash.Add(GREEDY_OPTION);
        //    hash.Add(REPEATS);
        //    hash.Add(PERMUTATIONFILE);
        //    hash.Add(PRINT_PERMUTED_DATA);
        //    hash.Add(PERMUTED_DATAFILE);
        //    hash.Add(PRINT_EVERY_PERM);
        //    hash.Add(EVERY_PERMFILE);
        //    hash.Add(PRINT_RANDOM_INPUTORDER);
        //    hash.Add(RANDOM_INPUTORDERFILE);
        //    hash.Add(OVERRIDE_WARNINGS);
        //    hash.Add(ORDER_BY_RUN);

        //    return hash.ToHashCode();
        //}

        //public static bool operator ==(CLUMPPConfigurationModel? left, CLUMPPConfigurationModel? right)
        //{
        //    if (left is null) return right is null;
        //    return left.Equals(right);
        //}

        //public static bool operator !=(CLUMPPConfigurationModel? left, CLUMPPConfigurationModel? right)
        //{
        //    return !(left == right);
        //}
    }
}
