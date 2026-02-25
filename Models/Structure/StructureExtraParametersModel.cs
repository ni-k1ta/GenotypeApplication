using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenotypeApplication.Models.Structure
{
    class StructureExtraParametersModel
    {
        private bool _noAdmix;
        public bool NOADMIX
        {
            get => _noAdmix; set => _noAdmix = value;
        }

        private bool _linkage;
        public bool LINKAGE
        {
            get => _linkage; set => _linkage = value;
        }

        private bool _usePopInfo;
        public bool USEPOPINFO
        {
            get => _usePopInfo; set => _usePopInfo = value;
        }

        private bool _locPrior;
        public bool LOCPRIOR
        {
            get => _locPrior; set => _locPrior = value;
        }

        private bool _onefst;
        public bool ONEFST
        {
            get => _onefst; set => _onefst = value;
        }

        private bool _inferAlpha;
        public bool INFERALPHA
        {
            get => _inferAlpha; set => _inferAlpha = value;
        }

        private bool _popAlphas;
        public bool POPALPHAS
        {
            get => _popAlphas; set => _popAlphas = value;
        }

        private double _alpha;
        public double ALPHA
        {
            get => _alpha; set => _alpha = value;
        }

        private bool _inferLambda;
        public bool INFERLAMBDA
        {
            get => _inferLambda; set => _inferLambda = value;
        }

        private bool _popSpecificLambda;
        public bool POPSPECIFICLAMBDA
        {
            get => _popSpecificLambda; set => _popSpecificLambda = value;
        }

        private double _lambda;
        public double LAMBDA
        {
            get => _lambda; set => _lambda = value;
        }

        private double _fPriorMean;
        public double FPRIORMEAN
        {
            get => _fPriorMean; set => _fPriorMean = value;
        }

        private double _fPriorSD;
        public double FPRIORSD
        {
            get => _fPriorSD; set => _fPriorSD = value;
        }

        private bool _unifPriorAlpha;
        public bool UNIFPRIORALPHA
        {
            get => _unifPriorAlpha; set => _unifPriorAlpha = value;
        }

        private double _alphaMax;
        public double ALPHAMAX
        {
            get => _alphaMax; set => _alphaMax = value;
        }

        private double _alphaPropSD;
        public double ALPHAPROPSD
        {
            get => _alphaPropSD; set => _alphaPropSD = value;
        }

        private double _alphaPriorB;
        public double ALPHAPRIORB
        {
            get => _alphaPriorB; set => _alphaPriorB = value;
        }

        private double _alphaPriorA;
        public double ALPHAPRIORA
        {
            get => _alphaPriorA; set => _alphaPriorA = value;
        }

        private double _log10rMin;
        public double LOG10RMIN
        {
            get => _log10rMin; set => _log10rMin = value;
        }

        private double _log10propSD;
        public double LOG10PROPSD
        {
            get => _log10propSD; set => _log10propSD = value;
        }

        private double _log10rMax;
        public double LOG10RMAX
        {
            get => _log10rMax; set => _log10rMax = value;
        }

        private double _log10rStart;
        public double LOG10RSTART
        {
            get => _log10rStart; set => _log10rStart = value;
        }

        private int _gensBack;
        public int GENSBACK
        {
            get => _gensBack; set => _gensBack = value;
        }

        private double _migrPrior;
        public double MIGRPRIOR
        {
            get => _migrPrior; set => _migrPrior = value;
        }

        private bool _pFromPopFlagOnly;
        public bool PFROMPOPFLAGONLY
        {
            get => _pFromPopFlagOnly; set => _pFromPopFlagOnly = value;
        }

        private bool _locIsPop;
        public bool LOCISPOP
        {
            get => _locIsPop; set => _locIsPop = value;
        }

        private double _locPriorInit;
        public double LOCPRIORINIT
        {
            get => _locPriorInit; set => _locPriorInit = value;
        }

        private double _maxLocPrior;
        public double MAXLOCPRIOR
        {
            get => _maxLocPrior; set => _maxLocPrior = value;
        }

        private bool _printNet;
        public bool PRINTNET
        {
            get => _printNet; set => _printNet = value;
        }

        private bool _printLambda;
        public bool PRINTLAMBDA
        {
            get => _printLambda; set => _printLambda = value;
        }

        private bool _printQsum;
        public bool PRINTQSUM
        {
            get => _printQsum; set => _printQsum = value;
        }

        private bool _siteBySite;
        public bool SITEBYSITE
        {
            get => _siteBySite; set => _siteBySite = value;
        }

        private int _updateFreq;
        public int UPDATEFREQ
        {
            get => _updateFreq; set => _updateFreq = value;
        }

        private bool _printLikes;
        public bool PRINTLIKES
        {
            get => _printLikes; set => _printLikes = value;
        }

        private bool _intermedSave;
        public bool INTERMEDSAVE
        {
            get => _intermedSave; set => _intermedSave = value;
        }

        private bool _echoData;
        public bool ECHODATA
        {
            get => _echoData; set => _echoData = value;
        }

        private bool _printQhat;
        public bool PRINTQHAT
        {
            get => _printQhat; set => _printQhat = value;
        }

        private bool _ancestDist;
        public bool ANCESTDIST
        {
            get => _ancestDist; set => _ancestDist = value;
        }

        private double _ancestPint;
        public double ANCESTPINT
        {
            get => _ancestPint; set => _ancestPint = value;
        }

        private int _numBoxes;
        public int NUMBOXES
        {
            get => _numBoxes; set => _numBoxes = value;
        }

        private bool _startAtPopInfo;
        public bool STARTATPOPINFO
        {
            get => _startAtPopInfo; set => _startAtPopInfo = value;
        }

        private int _metroFreq;
        public int METROFREQ
        {
            get => _metroFreq; set => _metroFreq = value;
        }

        private bool _freqsCorr;
        public bool FREQSCORR
        {
            get => _freqsCorr; set => _freqsCorr = value;
        }

        private bool _computerProb;
        public bool COMPUTEPROB
        {
            get => _computerProb; set => _computerProb = value;
        }

        private int _admBurnIn;
        public int ADMBURNIN
        {
            get => _admBurnIn; set => _admBurnIn = value;
        }

        private bool _randomize;
        public bool RANDOMIZE
        {
            get => _randomize; set => _randomize = value;
        }

        private int _seed;
        public int SEED
        {
            get => _seed; set => _seed = value;
        }

        private bool _reportThitRate;
        public bool REPORTHITRATE
        {
            get => _reportThitRate; set => _reportThitRate = value;
        }
    }
}
