using GenotypeApplication.Models.CLUMPP;
using GenotypeApplication.MVVM.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.IO.Compression;

namespace GenotypeApplication.Models
{
    public class DistructConfigurationModel : ViewModelBase //IEquatable<DistructConfigurationModel>
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

        private const string _infile_popq = "K.popq";
        [DefineParameterModel("INFILE_POPQ")]
        public string INFILE_POPQ => _infile_popq;

        private const string _infile_indivq = "K.indvq";
        [DefineParameterModel("INFILE_INDIVQ")]
        public string INFILE_INDIVQ => _infile_indivq;

        private string _infile_label_atop = string.Empty;
        [DefineParameterModel("INFILE_LABEL_ATOP")]
        public string INFILE_LABEL_ATOP
        {
            get => _infile_label_atop;
            set => _infile_label_atop = value;
        }

        private string _infile_label_below = string.Empty;
        [DefineParameterModel("INFILE_LABEL_BELOW")]
        public string INFILE_LABEL_BELOW
        {
            get => _infile_label_below;
            set => _infile_label_below = value;
        }

        private string _infile_clust_perm = string.Empty;

        //private string _infile_clust_perm = "clust.perm";
        [DefineParameterModel("INFILE_CLUST_PERM")]
        public string INFILE_CLUST_PERM => _infile_clust_perm;

        private const string _outfile = "K.ps";
        [DefineParameterModel("OUTFILE")]
        public string OUTFILE => _outfile;

        private const int _k = 1;
        [DefineParameterModel("K")]
        public int K => _k;

        private int _numpops;
        [DefineParameterModel("NUMPOPS")]
        public int NUMPOPS
        {
            get => _numpops;
            set => _numpops = value;
        }

        private int _numinds;
        [DefineParameterModel("NUMINDS")]
        public int NUMINDS
        {
            get => _numinds;
            set => _numinds = value;
        }

        private bool _print_indivs = true;
        [DefineParameterModel("PRINT_INDIVS")]
        public bool PRINT_INDIVS
        {
            get => _print_indivs;
            set => _print_indivs = value;
        }

        private bool _print_label_atop = false;
        [DefineParameterModel("PRINT_LABEL_ATOP")]
        public bool PRINT_LABEL_ATOP
        {
            get => _print_label_atop;
            set => _print_label_atop = value;
        }

        private bool _print_label_below = false;
        [DefineParameterModel("PRINT_LABEL_BELOW")]
        public bool PRINT_LABEL_BELOW
        {
            get => _print_label_below;
            set => _print_label_below = value;
        }

        private bool _print_sep = true;
        [DefineParameterModel("PRINT_SEP")]
        public bool PRINT_SEP
        {
            get => _print_sep;
            set => _print_sep = value;
        }

        private double _fontheight = 6.0;
        [DefineParameterModel("FONTHEIGHT")]
        public double FONTHEIGHT
        {
            get => _fontheight;
            set => _fontheight = value;
        }

        private double _dist_above = 5.0;
        [DefineParameterModel("DIST_ABOVE")]
        public double DIST_ABOVE
        {
            get => _dist_above;
            set => _dist_above = value;
        }

        private double _dist_below = -7.0;
        [DefineParameterModel("DIST_BELOW")]
        public double DIST_BELOW
        {
            get => _dist_below;
            set => _dist_below = value;
        }

        private double _boxheight = 36.0;
        [DefineParameterModel("BOXHEIGHT")]
        public double BOXHEIGHT
        {
            get => _boxheight;
            set => _boxheight = value;
        }

        private double _indivwidth = 1.5;
        [DefineParameterModel("INDIVWIDTH")]
        public double INDIVWIDTH
        {
            get => _indivwidth;
            set => _indivwidth = value;
        }

        private int _orientation = 0;
        [DefineParameterModel("ORIENTATION")]
        public int ORIENTATION
        {
            get => _orientation;
            set => _orientation = value;
        }

        private double _xorigin = 72.0;
        [DefineParameterModel("XORIGIN")]
        public double XORIGIN
        {
            get => _xorigin;
            set => _xorigin = value;
        }

        private double _yorigin = 28.0;
        [DefineParameterModel("YORIGIN")]
        public double YORIGIN
        {
            get => _yorigin;
            set => _yorigin = value;
        }

        private double _xscale = 1.0;
        [DefineParameterModel("XSCALE")]
        public double XSCALE
        {
            get => _xscale;
            set => _xscale = value;
        }

        private double _yscale = 1.0;
        [DefineParameterModel("YSCALE")]
        public double YSCALE
        {
            get => _yscale;
            set => _yscale = value;
        }

        private double _angle_label_atop = 60.0;
        [DefineParameterModel("ANGLE_LABEL_ATOP")]
        public double ANGLE_LABEL_ATOP
        {
            get => _angle_label_atop;
            set => _angle_label_atop = value;
        }

        private double _angle_label_below = 60.0;
        [DefineParameterModel("ANGLE_LABEL_BELOW")]
        public double ANGLE_LABEL_BELOW
        {
            get => _angle_label_below;
            set => _angle_label_below = value;
        }

        private double _linewidth_rim = 3.0;
        [DefineParameterModel("LINEWIDTH_RIM")]
        public double LINEWIDTH_RIM
        {
            get => _linewidth_rim;
            set => _linewidth_rim = value;
        }

        private double _linewidth_sep = 0.3;
        [DefineParameterModel("LINEWIDTH_SEP")]
        public double LINEWIDTH_SEP
        {
            get => _linewidth_sep;
            set => _linewidth_sep = value;
        }

        private double _linewidth_ind = 0.3;
        [DefineParameterModel("LINEWIDTH_IND")]
        public double LINEWIDTH_IND
        {
            get => _linewidth_ind;
            set => _linewidth_ind = value;
        }

        private bool _grayscale;
        [DefineParameterModel("GRAYSCALE")]
        public bool GRAYSCALE
        {
            get => _grayscale;
            set => _grayscale = value;
        }

        private bool _echo_data = true;
        [DefineParameterModel("ECHO_DATA")]
        public bool ECHO_DATA
        {
            get => _echo_data;
            set => _echo_data = value;
        }

        private bool _reprint_data = true;
        [DefineParameterModel("REPRINT_DATA")]
        public bool REPRINT_DATA
        {
            get => _reprint_data;
            set => _reprint_data = value;
        }

        private bool _print_infile_name;
        [DefineParameterModel("PRINT_INFILE_NAME")]
        public bool PRINT_INFILE_NAME
        {
            get => _print_infile_name;
            set => _print_infile_name = value;
        }

        private bool _print_color_brewer = false;
        [DefineParameterModel("PRINT_COLOR_BREWER")]
        public bool PRINT_COLOR_BREWER
        {
            get => _print_color_brewer;
            set => _print_color_brewer = value;
        }
    }
}
