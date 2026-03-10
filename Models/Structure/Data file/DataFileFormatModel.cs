namespace GenotypeApplication.Models.Structure
{
    public class DataFileFormatModel : IEquatable<DataFileFormatModel>
    {
        #region Row Parameters (Header Rows)

        private bool _markerNames = false;
        public bool MarkerNames
        {
            get => _markerNames; set => _markerNames = value;
        }

        private bool _recessiveAlleles = false;
        public bool RecessiveAlleles
        {
            get => _recessiveAlleles; set => _recessiveAlleles = value;
        }

        private bool _mapDistances = false;
        public bool MapDistances
        {
            get => _mapDistances; set => _mapDistances = value;
        }

        #endregion

        #region Column Parameters (Metadata Columns)

        private bool _label = false;
        public bool Label
        {
            get => _label; set => _label = value;
        }

        private bool _popData = false;
        public bool PopData
        {
            get => _popData; set => _popData = value;
        }

        private bool _popFlag = false;
        public bool PopFlag
        {
            get => _popFlag; set => _popFlag = value;
        }

        private bool _locData = false;
        public bool LocData
        {
            get => _locData; set => _locData = value;
        }

        private bool _phenotype = false;
        public bool Phenotype
        {
            get => _phenotype; set => _phenotype = value;
        }

        private int _extraCols = 0;
        public int ExtraCols
        {
            get => _extraCols; set => _extraCols = value;
        }

        #endregion

        #region Data Parameters

        private int _numInds = 0;
        public int NumInds
        {
            get => _numInds; set => _numInds = value;
        }

        private int _numLoci = 0;
        public int NumLoci
        {
            get => _numLoci; set => _numLoci = value;
        }

        private int _ploidy = 2;
        public int Ploidy
        {
            get => _ploidy; set => _ploidy = value;
        }

        private int _missing = -9;
        public int Missing
        {
            get => _missing; set => _missing = value;
        }

        private bool _oneRowPerInd = false;
        public bool OneRowPerInd
        {
            get => _oneRowPerInd; set => _oneRowPerInd = value;
        }

        #endregion

        private bool _phaseInfo = false;
        public bool PHASEINFO
        {
            get => _phaseInfo; set => _phaseInfo = value;
        }

        private bool _notAmbiguous = false;
        public bool NOTAMBIGUOUS
        {
            get => _notAmbiguous; set => _notAmbiguous = value;
        }
        private int _notAmbiguousValue = -999;
        public int NotAmbiguousValue
        {
            get => _notAmbiguousValue; set => _notAmbiguousValue = value;
        }

        public bool Equals(DataFileFormatModel? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return MarkerNames == other.MarkerNames
                && RecessiveAlleles == other.RecessiveAlleles
                && MapDistances == other.MapDistances
                && Label == other.Label
                && PopData == other.PopData
                && PopFlag == other.PopFlag
                && LocData == other.LocData
                && Phenotype == other.Phenotype
                && ExtraCols == other.ExtraCols
                && NumInds == other.NumInds
                && NumLoci == other.NumLoci
                && Ploidy == other.Ploidy
                && Missing == other.Missing
                && OneRowPerInd == other.OneRowPerInd
                && PHASEINFO == other.PHASEINFO
                && NOTAMBIGUOUS == other.NOTAMBIGUOUS
                && NotAmbiguousValue == other.NotAmbiguousValue;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as DataFileFormatModel);
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(MarkerNames);
            hash.Add(RecessiveAlleles);
            hash.Add(MapDistances);
            hash.Add(Label);
            hash.Add(PopData);
            hash.Add(PopFlag);
            hash.Add(LocData);
            hash.Add(Phenotype);
            hash.Add(ExtraCols);
            hash.Add(NumInds);
            hash.Add(NumLoci);
            hash.Add(Ploidy);
            hash.Add(Missing);
            hash.Add(OneRowPerInd);
            hash.Add(PHASEINFO);
            hash.Add(NOTAMBIGUOUS);
            hash.Add(NotAmbiguousValue);
            return hash.ToHashCode();
        }

        public static bool operator ==(DataFileFormatModel? left, DataFileFormatModel? right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        public static bool operator !=(DataFileFormatModel? left, DataFileFormatModel? right)
        {
            return !(left == right);
        }
    }
}
