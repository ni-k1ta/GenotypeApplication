namespace GenotypeApplication.Models.Structure
{
    public class DataFileFormatModel
    {
        #region Row Parameters (Header Rows)

        private bool _markerNames = false;
        /// <summary>
        /// Первая строка содержит названия маркеров (локусов).
        /// </summary>
        public bool MarkerNames
        {
            get => _markerNames; set => _markerNames = value;
        }

        private bool _recessiveAlleles = false;
        /// <summary>
        /// Строка с указанием рецессивных аллелей для каждого локуса.
        /// </summary>
        public bool RecessiveAlleles
        {
            get => _recessiveAlleles; set => _recessiveAlleles = value;
        }

        private bool _mapDistances = false;
        /// <summary>
        /// Строка с расстояниями между соседними маркерами.
        /// </summary>
        public bool MapDistances
        {
            get => _mapDistances; set => _mapDistances = value;
        }

        #endregion

        #region Column Parameters (Metadata Columns)

        private bool _label = false;
        /// <summary>
        /// Первый столбец содержит идентификаторы (имена) индивидов.
        /// </summary>
        public bool Label
        {
            get => _label; set => _label = value;
        }

        private bool _popData = false;
        /// <summary>
        /// Столбец с индексом популяции для каждого индивида.
        /// </summary>
        public bool PopData
        {
            get => _popData; set => _popData = value;
        }

        private bool _popFlag = false;
        /// <summary>
        /// Столбец-флаг использования popinfo (0/1).
        /// </summary>
        public bool PopFlag
        {
            get => _popFlag; set => _popFlag = value;
        }

        private bool _locData = false;
        /// <summary>
        /// Столбец с локацией сэмплирования индивида.
        /// </summary>
        public bool LocData
        {
            get => _locData; set => _locData = value;
        }

        private bool _phenotype = false;
        /// <summary>
        /// Столбец с фенотипом индивида.
        /// </summary>
        public bool Phenotype
        {
            get => _phenotype; set => _phenotype = value;
        }

        private int _extraCols = 0;
        /// <summary>
        /// Количество дополнительных столбцов перед данными генотипов.
        /// </summary>
        public int ExtraCols
        {
            get => _extraCols; set => _extraCols = value;
        }

        #endregion

        #region Data Parameters

        private int _numInds = 0;
        /// <summary>
        /// Количество индивидов в файле.
        /// </summary>
        public int NumInds
        {
            get => _numInds; set => _numInds = value;
        }

        private int _numLoci = 0;
        /// <summary>
        /// Количество локусов (маркеров) в файле.
        /// </summary>
        public int NumLoci
        {
            get => _numLoci; set => _numLoci = value;
        }

        private int _ploidy = 2;
        /// <summary>
        /// Плоидность организма. По умолчанию 2 (диплоид).
        /// </summary>
        public int Ploidy
        {
            get => _ploidy; set => _ploidy = value;
        }

        private int _missing = -9;
        /// <summary>
        /// Значение для отсутствующих данных. По умолчанию -9.
        /// </summary>
        public int Missing
        {
            get => _missing; set => _missing = value;
        }

        private bool _oneRowPerInd = false;
        /// <summary>
        /// Данные каждого индивида в одной строке (все аллели подряд).
        /// </summary>
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
    }
}
