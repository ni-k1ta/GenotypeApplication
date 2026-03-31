using GenotypeApplication.Models.CLUMPP;
using System.ComponentModel.DataAnnotations;
using System.IO.Compression;

namespace GenotypeApplication.Models
{
    public class DistructConfigurationModel : IEquatable<DistructConfigurationModel>
    {
        private string _parametersName = string.Empty;
        public string ParametersName
        {
            get => _parametersName;
            set => _parametersName = value;
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
        [DefineParameterModel("INFILE_CLUST_PERM")]
        public string INFILE_CLUST_PERM
        {
            get => _infile_clust_perm;
            set => _infile_clust_perm = value;
        }

        private const string _outfile = "K.ps";
        [DefineParameterModel("OUTFILE")]
        public string OUTFILE => _outfile;

        private const int _k = 0;
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

        private bool _print_indivs;
        [DefineParameterModel("PRINT_INDIVS")]
        public bool PRINT_INDIVS
        {
            get => _print_indivs;
            set => _print_indivs = value;
        }

        private bool _print_label_atop;
        [DefineParameterModel("PRINT_LABEL_ATOP")]
        public bool PRINT_LABEL_ATOP
        {
            get => _print_label_atop;
            set => _print_label_atop = value;
        }

        private bool _print_label_below;
        [DefineParameterModel("PRINT_LABEL_BELOW")]
        public bool PRINT_LABEL_BELOW
        {
            get => _print_label_below;
            set => _print_label_below = value;
        }

        private bool _print_sep;
        [DefineParameterModel("PRINT_SEP")]
        public bool PRINT_SEP
        {
            get => _print_sep;
            set => _print_sep = value;
        }

        private double _fontheight;
        [DefineParameterModel("FONTHEIGHT")]
        public double FONTHEIGHT
        {
            get => _fontheight;
            set => _fontheight = value;
        }

        private double _dist_above;
        [DefineParameterModel("DIST_ABOVE")]
        public double DIST_ABOVE
        {
            get => _dist_above;
            set => _dist_above = value;
        }

        private double _dist_below;
        [DefineParameterModel("DIST_BELOW")]
        public double DIST_BELOW
        {
            get => _dist_below;
            set => _dist_below = value;
        }

        private double _boxheight;
        [DefineParameterModel("BOXHEIGHT")]
        public double BOXHEIGHT
        {
            get => _boxheight;
            set => _boxheight = value;
        }

        private double _indivwidth;
        [DefineParameterModel("INDIVWIDTH")]
        public double INDIVWIDTH
        {
            get => _indivwidth;
            set => _indivwidth = value;
        }

        private int _orientation;
        [DefineParameterModel("ORIENTATION")]
        public int ORIENTATION
        {
            get => _orientation;
            set => _orientation = value;
        }

        private double _xorigin;
        [DefineParameterModel("XORIGIN")]
        public double XORIGIN
        {
            get => _xorigin;
            set => _xorigin = value;
        }

        private double _yorigin;
        [DefineParameterModel("YORIGIN")]
        public double YORIGIN
        {
            get => _yorigin;
            set => _yorigin = value;
        }

        private double _xscale;
        [DefineParameterModel("XSCALE")]
        public double XSCALE
        {
            get => _xscale;
            set => _xscale = value;
        }

        private double _yscale;
        [DefineParameterModel("YSCALE")]
        public double YSCALE
        {
            get => _yscale;
            set => _yscale = value;
        }

        private double _angle_label_atop;
        [DefineParameterModel("ANGLE_LABEL_ATOP")]
        public double ANGLE_LABEL_ATOP
        {
            get => _angle_label_atop;
            set => _angle_label_atop = value;
        }

        private double _angle_label_below;
        [DefineParameterModel("ANGLE_LABEL_BELOW")]
        public double ANGLE_LABEL_BELOW
        {
            get => _angle_label_below;
            set => _angle_label_below = value;
        }

        private double _linewidth_rim;
        [DefineParameterModel("LINEWIDTH_RIM")]
        public double LINEWIDTH_RIM
        {
            get => _linewidth_rim;
            set => _linewidth_rim = value;
        }

        private double _linewidth_sep;
        [DefineParameterModel("LINEWIDTH_SEP")]
        public double LINEWIDTH_SEP
        {
            get => _linewidth_sep;
            set => _linewidth_sep = value;
        }

        private double _linewidth_ind;
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

        private bool _echo_data;
        [DefineParameterModel("ECHO_DATA")]
        public bool ECHO_DATA
        {
            get => _echo_data;
            set => _echo_data = value;
        }

        private bool _reprint_data;
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

        private bool _print_color_brewer;
        [DefineParameterModel("PRINT_COLOR_BREWER")]
        public bool PRINT_COLOR_BREWER
        {
            get => _print_color_brewer;
            set => _print_color_brewer = value;
        }

        public bool Equals(DistructConfigurationModel? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return INFILE_POPQ == other.INFILE_POPQ &&              
                   INFILE_INDIVQ == other.INFILE_INDIVQ &&
                   INFILE_LABEL_ATOP == other.INFILE_LABEL_ATOP &&
                   INFILE_LABEL_BELOW == other.INFILE_LABEL_BELOW &&
                   INFILE_CLUST_PERM == other.INFILE_CLUST_PERM &&
                   OUTFILE == other.OUTFILE &&
                   K == other.K &&
                   NUMPOPS == other.NUMPOPS &&
                   NUMINDS == other.NUMINDS &&
                   PRINT_INDIVS == other.PRINT_INDIVS &&
                   PRINT_LABEL_ATOP == other.PRINT_LABEL_ATOP &&
                   PRINT_LABEL_BELOW == other.PRINT_LABEL_BELOW &&
                   PRINT_SEP == other.PRINT_SEP &&
                   FONTHEIGHT == other.FONTHEIGHT &&
                   DIST_ABOVE == other.DIST_ABOVE &&
                   DIST_BELOW == other.DIST_BELOW &&
                   BOXHEIGHT == other.BOXHEIGHT &&
                   INDIVWIDTH == other.INDIVWIDTH &&
                   ORIENTATION == other.ORIENTATION &&
                   XORIGIN == other.XORIGIN &&
                   YORIGIN == other.YORIGIN &&
                   XSCALE == other.XSCALE &&
                   YSCALE == other.YSCALE &&
                   ANGLE_LABEL_ATOP == other.ANGLE_LABEL_ATOP &&
                   ANGLE_LABEL_BELOW == other.ANGLE_LABEL_BELOW &&
                   LINEWIDTH_RIM == other.LINEWIDTH_RIM &&
                   LINEWIDTH_SEP == other.LINEWIDTH_SEP &&
                   LINEWIDTH_IND == other.LINEWIDTH_IND &&
                   GRAYSCALE == other.GRAYSCALE &&
                   ECHO_DATA == other.ECHO_DATA &&
                   REPRINT_DATA == other.REPRINT_DATA &&
                   PRINT_INFILE_NAME == other.PRINT_INFILE_NAME &&
                   PRINT_COLOR_BREWER == other.PRINT_COLOR_BREWER;
        }
        public override bool Equals(object? obj)
        {
            return Equals(obj as DistructConfigurationModel);
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            hash.Add(INFILE_POPQ);
            hash.Add(INFILE_INDIVQ);
            hash.Add(INFILE_LABEL_ATOP);
            hash.Add(INFILE_LABEL_BELOW);
            hash.Add(INFILE_CLUST_PERM);
            hash.Add(OUTFILE);
            hash.Add(K);
            hash.Add(NUMPOPS);
            hash.Add(NUMINDS);
            hash.Add(PRINT_INDIVS);
            hash.Add(PRINT_LABEL_ATOP);
            hash.Add(PRINT_LABEL_BELOW);
            hash.Add(PRINT_SEP);
            hash.Add(FONTHEIGHT);
            hash.Add(DIST_ABOVE);
            hash.Add(DIST_BELOW);
            hash.Add(BOXHEIGHT);
            hash.Add(INDIVWIDTH);
            hash.Add(ORIENTATION);
            hash.Add(XORIGIN);
            hash.Add(YORIGIN);
            hash.Add(XSCALE);
            hash.Add(YSCALE);
            hash.Add(ANGLE_LABEL_ATOP);
            hash.Add(ANGLE_LABEL_BELOW);
            hash.Add(LINEWIDTH_RIM);
            hash.Add(LINEWIDTH_SEP);
            hash.Add(LINEWIDTH_IND);
            hash.Add(GRAYSCALE);
            hash.Add(ECHO_DATA);
            hash.Add(REPRINT_DATA);
            hash.Add(PRINT_INFILE_NAME);
            hash.Add(PRINT_COLOR_BREWER);

            return hash.ToHashCode();
        }

        public static bool operator ==(DistructConfigurationModel? left, DistructConfigurationModel? right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        public static bool operator !=(DistructConfigurationModel? left, DistructConfigurationModel? right)
        {
            return !(left == right);
        }
    }
}
