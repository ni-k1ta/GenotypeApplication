using GenotypeApplication.Models.Structure.Data_file.Highlights;
using GenotypeApplication.MVVM.Converters;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GenotypeApplication.MVVM.Data_grid
{
    public static class DataGridHighlightBehavior
    {
        public static readonly DependencyProperty HighlightMapProperty =
            DependencyProperty.RegisterAttached("HighlightMap", typeof(HighlightMapModel), typeof(DataGridHighlightBehavior), new PropertyMetadata(null, OnHighlightInputChanged));

        public static HighlightMapModel? GetHighlightMap(DependencyObject obj) => (HighlightMapModel?)obj.GetValue(HighlightMapProperty);

        public static void SetHighlightMap(DependencyObject obj, HighlightMapModel? value) => obj.SetValue(HighlightMapProperty, value);


        public static readonly DependencyProperty DataSourceProperty =
            DependencyProperty.RegisterAttached("DataSource", typeof(DataTable), typeof(DataGridHighlightBehavior), new PropertyMetadata(null, OnHighlightInputChanged));

        public static DataTable? GetDataSource(DependencyObject obj) => (DataTable?)obj.GetValue(DataSourceProperty);

        public static void SetDataSource(DependencyObject obj, DataTable? value) => obj.SetValue(DataSourceProperty, value);

        private static void OnHighlightInputChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DataGrid dataGrid) return;

            if (!dataGrid.IsLoaded)
            {
                void onLoaded(object sender, RoutedEventArgs args)
                {
                    dataGrid.Loaded -= onLoaded;
                    var map = GetHighlightMap(dataGrid);
                    var data = GetDataSource(dataGrid);
                    UpdateCellStyle(dataGrid, map, data);
                    UpdateHeaders(dataGrid, map);
                    UpdateRowHeaders(dataGrid, map);
                }
                dataGrid.Loaded += onLoaded;
                return;
            }

            var map = GetHighlightMap(dataGrid);
            var data = GetDataSource(dataGrid);

            UpdateCellStyle(dataGrid, map, data);
            UpdateHeaders(dataGrid, map);
            UpdateRowHeaders(dataGrid, map);
        }

        private static void UpdateCellStyle(DataGrid dataGrid, HighlightMapModel? map, DataTable? data)
        {
            if (map == null || data == null)
            {
                dataGrid.CellStyle = null;
                return;
            }

            var converter = new CellHighlightConverter(map, data);

            var style = new Style(typeof(DataGridCell));

            var backgroundBinding = new MultiBinding
            {
                Converter = converter,
                ConverterParameter = "Background"
            };

            backgroundBinding.Bindings.Add(new Binding(".") { });

            backgroundBinding.Bindings.Add(new Binding(".")
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.Self)
            });

            var borderBinding = new MultiBinding
            {
                Converter = converter,
                ConverterParameter = "BorderBrush"
            };

            borderBinding.Bindings.Add(new Binding(".") { });

            borderBinding.Bindings.Add(new Binding(".")
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.Self)
            });

            style.Setters.Add(new Setter(DataGridCell.BackgroundProperty, backgroundBinding));
            style.Setters.Add(new Setter(DataGridCell.BorderBrushProperty, borderBinding));
            style.Setters.Add(new Setter(DataGridCell.BorderThicknessProperty, new Thickness(2)));

            dataGrid.CellStyle = style;
        }

        private static void UpdateHeaders(DataGrid dataGrid, HighlightMapModel? map)
        {
            if (map == null) return;

            // Заголовки столбцов
            foreach (var column in dataGrid.Columns)
            {
                int index = column.DisplayIndex;
                if (map.ColumnHeaders.TryGetValue(index, out var headerText))
                {
                    column.Header = headerText;
                }
            }
        }
        private static void UpdateRowHeaders(DataGrid dataGrid, HighlightMapModel? map)
        {
            if (map == null) return;

            var rowHeaderConverter = new RowHeaderConverter(map);

            var rowStyle = new Style(typeof(DataGridRow));

            var headerBinding = new Binding(".")
            {
                Converter = rowHeaderConverter
            };

            rowStyle.Setters.Add(new Setter(DataGridRow.HeaderProperty, headerBinding));

            dataGrid.RowStyle = rowStyle;
        }
    }
}
