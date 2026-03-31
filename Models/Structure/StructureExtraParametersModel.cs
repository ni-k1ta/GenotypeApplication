namespace GenotypeApplication.Models.Structure
{
    public class StructureExtraParametersModel : IEquatable<StructureExtraParametersModel>
    {
        private bool _noAdmix;
        [DefineParameterModel("NOADMIX")]
        public bool NOADMIX
        {
            get => _noAdmix; set => _noAdmix = value;
        }

        private bool _linkage;
        [DefineParameterModel("LINKAGE")]
        public bool LINKAGE
        {
            get => _linkage; set => _linkage = value;
        }

        private bool _usePopInfo;
        [DefineParameterModel("USEPOPINFO")]
        public bool USEPOPINFO
        {
            get => _usePopInfo; set => _usePopInfo = value;
        }

        private bool _locPrior;
        [DefineParameterModel("LOCPRIOR")]
        public bool LOCPRIOR
        {
            get => _locPrior; set => _locPrior = value;
        }

        private bool _onefst;
        [DefineParameterModel("ONEFST")]
        public bool ONEFST
        {
            get => _onefst; set => _onefst = value;
        }

        private bool _inferAlpha = false;
        [DefineParameterModel("INFERALPHA")]
        public bool INFERALPHA
        {
            get => _inferAlpha; set => _inferAlpha = value;
        }

        private bool _popAlphas;
        [DefineParameterModel("POPALPHAS")]
        public bool POPALPHAS
        {
            get => _popAlphas; set => _popAlphas = value;
        }

        private double _alpha = 1;
        [DefineParameterModel("ALPHA")]
        public double ALPHA
        {
            get => _alpha; set => _alpha = value;
        }

        private bool _inferLambda = false;
        [DefineParameterModel("INFERLAMBDA")]
        public bool INFERLAMBDA
        {
            get => _inferLambda; set => _inferLambda = value;
        }

        private bool _popSpecificLambda;
        [DefineParameterModel("POPSPECIFICLAMBDA")]
        public bool POPSPECIFICLAMBDA
        {
            get => _popSpecificLambda; set => _popSpecificLambda = value;
        }

        private double _lambda = 1;
        [DefineParameterModel("LAMBDA")]
        public double LAMBDA
        {
            get => _lambda; set => _lambda = value;
        }

        private double _fPriorMean = 0.01;
        [DefineParameterModel("FPRIORMEAN")]
        public double FPRIORMEAN
        {
            get => _fPriorMean; set => _fPriorMean = value;
        }

        private double _fPriorSD = 0.05;
        [DefineParameterModel("FPRIORSD")]
        public double FPRIORSD
        {
            get => _fPriorSD; set => _fPriorSD = value;
        }

        private bool _unifPriorAlpha = true;
        [DefineParameterModel("UNIFPRIORALPHA")]
        public bool UNIFPRIORALPHA
        {
            get => _unifPriorAlpha; set => _unifPriorAlpha = value;
        }

        private double _alphaMax = 10.0;
        [DefineParameterModel("ALPHAMAX")]
        public double ALPHAMAX
        {
            get => _alphaMax; set => _alphaMax = value;
        }

        private double _alphaPropSD = 0.025;
        [DefineParameterModel("ALPHAPROPSD")]
        public double ALPHAPROPSD
        {
            get => _alphaPropSD; set => _alphaPropSD = value;
        }

        private double _alphaPriorB = 0.001;
        [DefineParameterModel("ALPHAPRIORB")]
        public double ALPHAPRIORB
        {
            get => _alphaPriorB; set => _alphaPriorB = value;
        }

        private double _alphaPriorA = 0.05;
        [DefineParameterModel("ALPHAPRIORA")]
        public double ALPHAPRIORA
        {
            get => _alphaPriorA; set => _alphaPriorA = value;
        }

        private double _log10rMin = -4.0;
        [DefineParameterModel("LOG10RMIN")]
        public double LOG10RMIN
        {
            get => _log10rMin; set => _log10rMin = value;
        }

        private double _log10rpropSD = 0.1;
        [DefineParameterModel("LOG10RPROPSD")]
        public double LOG10RPROPSD
        {
            get => _log10rpropSD; set => _log10rpropSD = value;
        }

        private double _log10rMax = 1.0;
        [DefineParameterModel("LOG10RMAX")]
        public double LOG10RMAX
        {
            get => _log10rMax; set => _log10rMax = value;
        }

        private double _log10rStart = -2.0;
        [DefineParameterModel("LOG10RSTART")]
        public double LOG10RSTART
        {
            get => _log10rStart; set => _log10rStart = value;
        }

        private int _gensBack = 2;
        [DefineParameterModel("GENSBACK")]
        public int GENSBACK
        {
            get => _gensBack; set => _gensBack = value;
        }

        private double _migrPrior = 0.01;
        [DefineParameterModel("MIGRPRIOR")]
        public double MIGRPRIOR
        {
            get => _migrPrior; set => _migrPrior = value;
        }

        private bool _pFromPopFlagOnly;
        [DefineParameterModel("PFROMPOPFLAGONLY")]
        public bool PFROMPOPFLAGONLY
        {
            get => _pFromPopFlagOnly; set => _pFromPopFlagOnly = value;
        }

        private bool _locIsPop;
        [DefineParameterModel("LOCISPOP")]
        public bool LOCISPOP
        {
            get => _locIsPop; set => _locIsPop = value;
        }

        private double _locPriorInit = 1.0;
        [DefineParameterModel("LOCPRIORINIT")]
        public double LOCPRIORINIT
        {
            get => _locPriorInit; set => _locPriorInit = value;
        }

        private double _maxLocPrior = 20.0;
        [DefineParameterModel("MAXLOCPRIOR")]
        public double MAXLOCPRIOR
        {
            get => _maxLocPrior; set => _maxLocPrior = value;
        }

        private bool _printNet = true;
        [DefineParameterModel("PRINTNET")]
        public bool PRINTNET
        {
            get => _printNet; set => _printNet = value;
        }

        private bool _printLambda = true;
        [DefineParameterModel("PRINTLAMBDA")]
        public bool PRINTLAMBDA
        {
            get => _printLambda; set => _printLambda = value;
        }

        private bool _printQsum = true;
        [DefineParameterModel("PRINTQSUM")]
        public bool PRINTQSUM
        {
            get => _printQsum; set => _printQsum = value;
        }

        private bool _siteBySite;
        [DefineParameterModel("SITEBYSITE")]
        public bool SITEBYSITE
        {
            get => _siteBySite; set => _siteBySite = value;
        }

        private int _updateFreq;
        [DefineParameterModel("UPDATEFREQ")]
        public int UPDATEFREQ
        {
            get => _updateFreq; set => _updateFreq = value;
        }

        private bool _printLikes;
        [DefineParameterModel("PRINTLIKES")]
        public bool PRINTLIKES
        {
            get => _printLikes; set => _printLikes = value;
        }

        private bool _intermedSave;
        [DefineParameterModel("INTERMEDSAVE")]
        public bool INTERMEDSAVE
        {
            get => _intermedSave; set => _intermedSave = value;
        }

        private bool _echoData;
        [DefineParameterModel("ECHODATA")]
        public bool ECHODATA
        {
            get => _echoData; set => _echoData = value;
        }

        private bool _printQhat;
        [DefineParameterModel("PRINTQHAT")]
        public bool PRINTQHAT
        {
            get => _printQhat; set => _printQhat = value;
        }

        private bool _ancestDist;
        [DefineParameterModel("ANCESTDIST")]
        public bool ANCESTDIST
        {
            get => _ancestDist; set => _ancestDist = value;
        }

        private double _ancestPint = 0.9;
        [DefineParameterModel("ANCESTPINT")]
        public double ANCESTPINT
        {
            get => _ancestPint; set => _ancestPint = value;
        }

        private int _numBoxes = 1_000;
        [DefineParameterModel("NUMBOXES")]
        public int NUMBOXES
        {
            get => _numBoxes; set => _numBoxes = value;
        }

        private bool _startAtPopInfo;
        [DefineParameterModel("STARTATPOPINFO")]
        public bool STARTATPOPINFO
        {
            get => _startAtPopInfo; set => _startAtPopInfo = value;
        }

        private int _metroFreq = 10;
        [DefineParameterModel("METROFREQ")]
        public int METROFREQ
        {
            get => _metroFreq; set => _metroFreq = value;
        }

        private bool _freqsCorr = true;
        [DefineParameterModel("FREQSCORR")]
        public bool FREQSCORR
        {
            get => _freqsCorr; set => _freqsCorr = value;
        }

        private bool _computeProb = true;
        [DefineParameterModel("COMPUTEPROB")]
        public bool COMPUTEPROB
        {
            get => _computeProb; set => _computeProb = value;
        }

        private int _admBurnIn = 500;
        [DefineParameterModel("ADMBURNIN")]
        public int ADMBURNIN
        {
            get => _admBurnIn; set => _admBurnIn = value;
        }

        private bool _randomize = true;
        [DefineParameterModel("RANDOMIZE")]
        public bool RANDOMIZE
        {
            get => _randomize; set => _randomize = value;
        }

        private int _seed;
        [DefineParameterModel("SEED")]
        public int SEED
        {
            get => _seed; set => _seed = value;
        }

        private bool _reportThitRate;
        [DefineParameterModel("REPORTHITRATE")]
        public bool REPORTHITRATE
        {
            get => _reportThitRate; set => _reportThitRate = value;
        }

        public bool Equals(StructureExtraParametersModel? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return NOADMIX == other.NOADMIX &&
                LINKAGE == other.LINKAGE &&
                USEPOPINFO == other.USEPOPINFO &&
                LOCPRIOR == other.LOCPRIOR &&
                ONEFST == other.ONEFST &&
                INFERALPHA == other.INFERALPHA &&
                POPALPHAS == other.POPALPHAS &&
                ALPHA == other.ALPHA &&
                INFERLAMBDA == other.INFERLAMBDA &&
                POPSPECIFICLAMBDA == other.POPSPECIFICLAMBDA &&
                LAMBDA == other.LAMBDA &&
                FPRIORMEAN == other.FPRIORMEAN &&
                FPRIORSD == other.FPRIORSD &&
                UNIFPRIORALPHA == other.UNIFPRIORALPHA &&
                ALPHAMAX == other.ALPHAMAX &&
                ALPHAPROPSD == other.ALPHAPROPSD &&
                ALPHAPRIORB == other.ALPHAPRIORB &&
                ALPHAPRIORA == other.ALPHAPRIORA &&
                LOG10RMIN == other.LOG10RMIN &&
                LOG10RPROPSD == other.LOG10RPROPSD &&
                LOG10RMAX == other.LOG10RMAX &&
                LOG10RSTART == other.LOG10RSTART &&
                GENSBACK == other.GENSBACK &&
                MIGRPRIOR == other.MIGRPRIOR &&
                PFROMPOPFLAGONLY == other.PFROMPOPFLAGONLY &&
                LOCISPOP == other.LOCISPOP &&
                LOCPRIORINIT == other.LOCPRIORINIT &&
                MAXLOCPRIOR == other.MAXLOCPRIOR &&
                PRINTNET == other.PRINTNET &&
                PRINTLAMBDA == other.PRINTLAMBDA &&
                PRINTQSUM == other.PRINTQSUM &&
                SITEBYSITE == other.SITEBYSITE &&
                UPDATEFREQ == other.UPDATEFREQ &&
                PRINTLIKES == other.PRINTLIKES &&
                INTERMEDSAVE == other.INTERMEDSAVE &&
                ECHODATA == other.ECHODATA &&
                PRINTQHAT == other.PRINTQHAT &&
                ANCESTDIST == other.ANCESTDIST &&
                ANCESTPINT == other.ANCESTPINT &&
                NUMBOXES == other.NUMBOXES &&
                STARTATPOPINFO == other.STARTATPOPINFO &&
                METROFREQ == other.METROFREQ &&
                FREQSCORR == other.FREQSCORR &&
                COMPUTEPROB == other.COMPUTEPROB &&
                ADMBURNIN == other.ADMBURNIN &&
                RANDOMIZE == other.RANDOMIZE &&
                SEED == other.SEED &&
                REPORTHITRATE == other.REPORTHITRATE;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as StructureExtraParametersModel);
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(NOADMIX);
            hash.Add(LINKAGE);
            hash.Add(USEPOPINFO);
            hash.Add(LOCPRIOR);
            hash.Add(ONEFST);
            hash.Add(INFERALPHA);
            hash.Add(POPALPHAS);
            hash.Add(ALPHA);
            hash.Add(INFERLAMBDA);
            hash.Add(POPSPECIFICLAMBDA);
            hash.Add(LAMBDA);
            hash.Add(FPRIORMEAN);
            hash.Add(FPRIORSD);
            hash.Add(UNIFPRIORALPHA);
            hash.Add(ALPHAMAX);
            hash.Add(ALPHAPROPSD);
            hash.Add(ALPHAPRIORB);
            hash.Add(ALPHAPRIORA);
            hash.Add(LOG10RMIN);
            hash.Add(LOG10RPROPSD);
            hash.Add(LOG10RMAX);
            hash.Add(LOG10RSTART);
            hash.Add(GENSBACK);
            hash.Add(MIGRPRIOR);
            hash.Add(PFROMPOPFLAGONLY);
            hash.Add(LOCISPOP);
            hash.Add(LOCPRIORINIT);
            hash.Add(MAXLOCPRIOR);
            hash.Add(PRINTNET);
            hash.Add(PRINTLAMBDA);
            hash.Add(PRINTQSUM);
            hash.Add(SITEBYSITE);
            hash.Add(UPDATEFREQ);
            hash.Add(PRINTLIKES);
            hash.Add(INTERMEDSAVE);
            hash.Add(ECHODATA);
            hash.Add(PRINTQHAT);
            hash.Add(ANCESTDIST);
            hash.Add(ANCESTPINT);
            hash.Add(NUMBOXES);
            hash.Add(STARTATPOPINFO);
            hash.Add(METROFREQ);
            hash.Add(FREQSCORR);
            hash.Add(COMPUTEPROB);
            hash.Add(ADMBURNIN);
            hash.Add(RANDOMIZE);
            hash.Add(SEED);
            hash.Add(REPORTHITRATE);

            return hash.ToHashCode();
        }

        public static bool operator ==(StructureExtraParametersModel? left, StructureExtraParametersModel? right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        public static bool operator !=(StructureExtraParametersModel? left, StructureExtraParametersModel? right)
        {
            return !(left == right);
        }
    }
}
