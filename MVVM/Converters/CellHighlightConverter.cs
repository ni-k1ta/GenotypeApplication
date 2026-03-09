using GenotypeApplication.Models.Structure.Data_file.Highlights;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace GenotypeApplication.MVVM.Converters
{
    public class CellHighlightConverter : IMultiValueConverter
    {
        private readonly HighlightMapModel _map;

        private readonly SolidColorBrush? _missingBorderBrush;
        private readonly SolidColorBrush? _notAmbiguousBorderBrush;

        private readonly Brush[,] _backgroundMatrix;
        private readonly Brush[,] _borderMatrix;
        private readonly int _totalRows;
        private readonly int _totalCols;

        private readonly Dictionary<DataGridCell, Brush> _borderResultCache = new();

        public CellHighlightConverter(HighlightMapModel map, DataTable? data)
        {
            _map = map;

            if (map.MissingValue.HasValue)
            {
                _missingBorderBrush = new SolidColorBrush(map.MissingCellBorderColor);
                _missingBorderBrush.Freeze();
            }

            if (map.NotAmbiguousValue.HasValue)
            {
                _notAmbiguousBorderBrush = new SolidColorBrush(map.NotAmbiguousCellBorderColor);
                _notAmbiguousBorderBrush.Freeze();
            }

            _totalRows = data?.Rows.Count ?? 0;
            _totalCols = data?.Columns.Count ?? 0;

            _backgroundMatrix = new Brush[_totalRows, _totalCols];
            _borderMatrix = new Brush[_totalRows, _totalCols];

            PrecomputeMatrices(data);
        }
        private void PrecomputeMatrices(DataTable? data)
        {
            for (int row = 0; row < _totalRows; row++)
            {
                for (int col = 0; col < _totalCols; col++)
                {
                    _backgroundMatrix[row, col] = Brushes.Transparent;
                    _borderMatrix[row, col] = Brushes.Transparent;
                }
            }

            if (_map.DataBlock != null)
            {
                var brush = GetRegionBrush(_map.DataBlock);
                FillRegion(_backgroundMatrix, _map.DataBlock, brush);
            }

            foreach (var region in _map.ColumnRegions)
            {
                var brush = GetRegionBrush(region);
                FillRegion(_backgroundMatrix, region, brush);
            }

            foreach (var region in _map.RowRegions)
            {
                var brush = GetRegionBrush(region);
                FillRegion(_backgroundMatrix, region, brush);
            }

            foreach (var region in _map.PhaseInfoRegions)
            {
                var brush = GetRegionBrush(region);
                FillRegion(_backgroundMatrix, region, brush);
            }

            if (data != null)
            {
                PrecomputeBorders(data);
            }
        }
        private void FillRegion(Brush[,] matrix, HighlightRegionModel region, Brush brush)
        {
            int startRow = Math.Max(0, region.StartRow);
            int endRow = Math.Min(_totalRows - 1, region.EndRow);
            int startCol = Math.Max(0, region.StartCol);
            int endCol = Math.Min(_totalCols - 1, region.EndCol);

            for (int row = startRow; row <= endRow; row++)
            {
                for (int col = startCol; col <= endCol; col++)
                {
                    matrix[row, col] = brush;
                }
            }
        }
        private void PrecomputeBorders(DataTable rawData)
        {
            for (int row = 0; row < _totalRows; row++)
            {
                var dataRow = rawData.Rows[row];

                for (int col = 0; col < _totalCols; col++)
                {
                    var value = dataRow[col]?.ToString();
                    if (value == null) continue;

                    bool isMissing = _map.MissingValue.HasValue
                        && IsMatch(value, _map.MissingValue.Value);

                    bool isNotAmbiguous = _map.NotAmbiguousValue.HasValue
                        && IsMatch(value, _map.NotAmbiguousValue.Value);

                    if (isMissing)
                    {
                        _borderMatrix[row, col] = _missingBorderBrush ?? Brushes.Transparent;
                    }

                    if (isNotAmbiguous)
                    {
                        _borderMatrix[row, col] = _notAmbiguousBorderBrush ?? Brushes.Transparent;
                    }
                }
            }
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) return DependencyProperty.UnsetValue;
            if (values[0] is not DataRowView) return DependencyProperty.UnsetValue;
            if (values[1] is not DataGridCell cell) return DependencyProperty.UnsetValue;

            var mode = parameter as string;

            if (mode == "Background")
            {
                int col = cell.Column?.DisplayIndex ?? -1;
                int row = GetRowIndex(cell);

                if (row < 0 || col < 0) return DependencyProperty.UnsetValue;
                if (row >= _totalRows || col >= _totalCols) return DependencyProperty.UnsetValue;

                _borderResultCache[cell] = _borderMatrix[row, col];

                return _backgroundMatrix[row, col];
            }

            if (mode == "BorderBrush")
            {
                if (_borderResultCache.TryGetValue(cell, out var cached))
                {
                    _borderResultCache.Remove(cell);
                    return cached;
                }

                int col = cell.Column?.DisplayIndex ?? -1;
                int row = GetRowIndex(cell);

                if (row < 0 || col < 0) return Brushes.Transparent;
                if (row >= _totalRows || col >= _totalCols) return Brushes.Transparent;

                return _borderMatrix[row, col];
            }

            return DependencyProperty.UnsetValue;
        }

        private static int GetRowIndex(DataGridCell cell)
        {
            var row = DataGridRow.GetRowContainingElement(cell);
            if (row == null) return -1;
            return row.GetIndex();
        }

        private readonly Dictionary<Color, Brush> _brushCache = new();
        private Brush GetRegionBrush(HighlightRegionModel region)
        {
            return GetOrCreateBrush(region.Color, alpha: 40);
        }
        private Brush GetOrCreateBrush(Color color, byte alpha)
        {
            var adjustedColor = Color.FromArgb(alpha, color.R, color.G, color.B);

            if (_brushCache.TryGetValue(adjustedColor, out var cached))
                return cached;

            var brush = new SolidColorBrush(adjustedColor);
            brush.Freeze();
            _brushCache[adjustedColor] = brush;
            return brush;
        }

        private static bool IsMatch(string cellText, int targetValue)
        {
            if (int.TryParse(cellText.Trim(), out int parsed))
                return parsed == targetValue;

            return cellText.Trim() == targetValue.ToString();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
