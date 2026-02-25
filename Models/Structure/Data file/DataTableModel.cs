using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenotypeApplication.Models.Structure.Data_file
{
    /// <summary>
    /// Обёртка над DataTable для удобной работы с данными файла.
    /// Предоставляет методы для доступа к строкам, столбцам и ячейкам.
    /// </summary>
    public class DataTableModel
    {
        private readonly DataTable _dataTable;

        public DataTableModel(DataTable dataTable)
        {
            _dataTable = dataTable ?? throw new ArgumentNullException("Data table from data file can't be null!");
        }

        /// <summary>
        /// Исходный DataTable для биндинга к DataGrid.
        /// </summary>
        public DataTable RawData => _dataTable;
        /// <summary>
        /// Количество строк в таблице.
        /// </summary>
        public int RowsCount => _dataTable.Rows.Count;
        /// <summary>
        /// Количество столбцов в таблице.
        /// </summary>
        public int ColumnCount => _dataTable.Columns.Count;
        /// <summary>
        /// Проверяет, пуста ли таблица.
        /// </summary>
        public bool IsEmpty => RowsCount == 0 || ColumnCount == 0;


        public string[] GetColumn(int columnIndex, int startRowIndex)
        {
            if (columnIndex < 0 || columnIndex >= ColumnCount)
                return Array.Empty<string>();
            if (startRowIndex < 0 || startRowIndex >= RowsCount)
                return Array.Empty<string>();

            return _dataTable.Rows
                .Cast<DataRow>()
                .Skip(startRowIndex)
                .Select(row => row[columnIndex]?.ToString() ?? string.Empty)
                .ToArray();
        }
        public string[] GetColumnRows(int columnIndex, int startRowIndex, int rowCount)
        {
            if (columnIndex < 0 || columnIndex >= ColumnCount)
                return Array.Empty<string>();

            if (startRowIndex < 0 || startRowIndex >= RowsCount)
                return Array.Empty<string>();

            if (rowCount <= 0)
                return Array.Empty<string>();

            return _dataTable.Rows
                .Cast<DataRow>()
                .Skip(startRowIndex)
                .Take(rowCount)                // ← ограничение количества строк
                .Select(row => row[columnIndex]?.ToString() ?? string.Empty)
                .ToArray();
        }
        public string[] GetRow(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= RowsCount)
                return Array.Empty<string>();

            return _dataTable.Rows[rowIndex].ItemArray
                .Select(item => item?.ToString() ?? string.Empty)
                .ToArray();
        }
        public string GetCellValue(int columnIndex, int rowIndex)
        {
            if (columnIndex < 0 || columnIndex >= ColumnCount)
                return string.Empty;

            if (rowIndex < 0 || rowIndex >= RowsCount)
                return string.Empty;

            return _dataTable.Rows[rowIndex][columnIndex]?.ToString() ?? string.Empty;
        }
    }
}
