using GenotypeApplication.MVVM.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GenotypeApplication.Models.Distruct
{
    public class ClusterColorItem : ViewModelBase
    {
        private int _clusterIndex;
        private string _colorName = string.Empty;
        private double _grayscaleValue;

        public int ClusterIndex
        {
            get => _clusterIndex;
            set { SetField(ref _clusterIndex, value); }
        }

        public string ColorName
        {
            get => _colorName;
            set { SetField(ref _colorName, value); }
        }

        public double GrayscaleValue
        {
            get => _grayscaleValue;
            set 
            {
                if (value > 1.0 || value < 0.0) return;
                SetField(ref _grayscaleValue, value);
            }
        }
    }
}
