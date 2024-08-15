using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AssetStatusInfo
{
    public delegate void ButtonAction<Object, EventArgs>();
    internal class SearchGridDecorator
    {
        public DataGridView searchableGrid;
        public DataGridView searchGrid;
        public Button? searchButton;
        public Button? searchClearButton;
        public Action actionsOnSearch;
        public Func<DataTable> resetSearchableGridData;
        //magnifying glass painter
        private int penThickness = 2;
        private SolidBrush borderColor = new SolidBrush(Color.LightGray);
        //non-searchable field info
        private Color nonSearchableBackgroundColor = Color.Gray;
        private bool nonSearchableHeadersInvisible = true;
        public List<string> nonSearchableFields = new List<string>();

        public SearchGridDecorator(DataGridView SearchableGrid, DataGridView SearchGrid, Button SearchButton, Button SearchClearButton, Func<DataTable> ResetSearchableGridData) : this(SearchableGrid, SearchGrid, SearchButton, ResetSearchableGridData)
        {
            searchClearButton = SearchClearButton;
            searchClearButton.Click += searchClearButton_Click;
        }
        public SearchGridDecorator(DataGridView SearchableGrid, DataGridView SearchGrid, Button SearchButton, Func<DataTable> ResetSearchableGridData) : this(SearchableGrid, SearchGrid, ResetSearchableGridData)
        {
            searchButton = SearchButton;
            searchButton.Click += searchButton_Click;
        }

        public SearchGridDecorator(DataGridView SearchableGrid, DataGridView SearchGrid, Func<DataTable> ResetSearchableGridData)
        {
            searchableGrid = SearchableGrid;
            searchableGrid.ColumnWidthChanged += MatchColumnWidth;
            searchableGrid.DataContextChanged += MatchColumnOrder;
            resetSearchableGridData = ResetSearchableGridData;

            searchGrid = SearchGrid;
            searchGrid.CellPainting += SearchGrid_PaintMagnifyingGlass;
            searchGrid.KeyUp += SearchGrid_KeyUp;
            searchGrid.EnableHeadersVisualStyles = false;
            actionsOnSearch += executeSearch;
        }

        public void AddSearchAction(Action SearchAction)
        {
            actionsOnSearch += SearchAction;
        }

        public void RemoveSearchAction(Action SearchAction)
        {
            //original
            if (actionsOnSearch == null ? false : actionsOnSearch.GetInvocationList().Contains(SearchAction))
            {
                if (SearchAction != null)
                {
                    actionsOnSearch -= SearchAction;
                }
            }
        }
        public void ChangeMagnifyingGlass(int PenThickness, SolidBrush BorderColor)
        {
            penThickness = PenThickness;
            borderColor = BorderColor;
        }

        private void MatchColumnOrder(object? sender, EventArgs e)
        {
            MatchColumnOrder();
        }
        public void MatchColumnOrder()
        {
            int missing = 0;
            foreach (DataGridViewColumn column in searchableGrid.Columns)
            {
                if (searchGrid.Columns[column.Name] != null)
                {
                    searchGrid.Columns[column.Name].DisplayIndex = column.DisplayIndex - missing;
                }
                else
                {
                    missing++;
                }
            }
        }

        private void MatchColumnWidth(object? sender, DataGridViewColumnEventArgs e)
        {
            MatchColumnWidth();
        }
        public void MatchColumnWidth()
        {
            var visibleColumns = from DataGridViewColumn c in searchableGrid.Columns
                                 where c.Visible
                                 select c;
            foreach (DataGridViewColumn column in visibleColumns)
            {
                if (searchGrid.Columns[column.Name] != null)
                {
                    searchGrid.Columns[column.Name].Width = column.Width;
                }
            }
        }
        public void FormatNonSearchableFields()
        {
            foreach (string columnName in nonSearchableFields)
            {
                if (searchGrid.Columns.Contains(columnName))
                {
                    searchGrid.Columns[columnName].DefaultCellStyle.BackColor = nonSearchableBackgroundColor;
                    searchGrid.Columns[columnName].HeaderCell.Style.BackColor = nonSearchableBackgroundColor;
                    if (nonSearchableHeadersInvisible) { searchGrid.Columns[columnName].HeaderText = ""; }
                    searchGrid.Columns[columnName].ReadOnly = true;
                }
            }
        }
        public DataSet GridDataSetter(DataSet dataSet, InputConfigurations configs, string tableName = null)
        {
            if (tableName == null)
            {
                tableName = searchGrid.Name;
            }
            return GridDataSetter(searchGrid, dataSet, configs, tableName);
        }
        public static DataSet GridDataSetter(DataGridView grid, DataSet? dataSet, InputConfigurations configs, string tableName = null)
        {
            //should I make an overload without dataSet?
            if (tableName == null)
            {
                tableName = grid.Name;
            }
            if (dataSet == null)
            {
                dataSet = new DataSet();
            }
            DataTable manifest = dataSet.Tables.Add(tableName);
            foreach (KeyValuePair<String, InputConfiguration> kvp in configs.configurations)
            {
                manifest.Columns.Add(kvp.Key, kvp.Value.typeOf);
            }
            grid.DataSource = manifest;
            return dataSet;
        }
        private void SearchGrid_PaintMagnifyingGlass(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            DataGridView? s = null;
            if (sender != null)
            {
                if (sender.GetType() == typeof(DataGridView) && e.RowIndex != -1)
                {
                    s = (DataGridView)sender;
                    if (!s.Rows[e.RowIndex].Cells[e.ColumnIndex].ReadOnly)
                    {
                        Size size = e.CellBounds.Size;
                        if (size.Width < size.Height)
                        {
                            size.Width = size.Width - 4 * penThickness;
                            size.Width = (int)(.65 * size.Width);
                            size.Height = size.Width;
                        }
                        else
                        {
                            size.Height = size.Height - 4 * penThickness;
                            size.Height = (int)(.65 * size.Height);
                            size.Width = size.Height;
                        }
                        Point point = new Point(e.CellBounds.X + (e.CellBounds.Width - (int)(1.9 * size.Width)), e.CellBounds.Y + (e.CellBounds.Height - (int)(1.9 * size.Height))); //fix misalignment with interior do
                        Rectangle rectangle = new Rectangle(point, size);
                        Point centerPoint = new Point(rectangle.X + (int)(.9*rectangle.Width), rectangle.Y + (int)(.9 * rectangle.Height));
                        Point bottomRight = new Point(rectangle.X + (int)(1.3 * rectangle.Width), rectangle.Y + (int)(1.3 * rectangle.Height));
                        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        e.Paint(rectangle, DataGridViewPaintParts.All);
                        Pen pen = new Pen(borderColor, penThickness);
                        e.Graphics.DrawEllipse(pen, rectangle);
                        pen.Width = 4;
                        e.Graphics.DrawLine(pen, centerPoint, bottomRight);
                        e.Handled = true;
                    }
                }
            }
        }
        private void SearchGrid_KeyUp(object? sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                DataGridView? s = null;
                if (sender != null)
                {
                    s = (DataGridView)sender;
                    s.EndEdit();
                }
                actionsOnSearch();
            }
        }

        private void searchButton_Click(object? sender, EventArgs e)
        {
            actionsOnSearch();
        }

        private void executeSearch()
        {
            DataTable searchableGridDataTable = (DataTable)searchableGrid.DataSource;
            string queryFilter = "";
            foreach (DataGridViewCell cell in searchGrid.Rows[0].Cells)
            {
                if (cell.Value != null && cell.Value != DBNull.Value)
                {
                    if (cell.Value.ToString() != "")
                    {
                        queryFilter += $"{cell.OwningColumn.Name} LIKE '{cell.Value}' AND ";
                    }
                }
            }
            if (queryFilter.Length > 5)
            {
                queryFilter = queryFilter.Substring(0, queryFilter.Length - 5);
                DataTable a = new DataTable();
                a = searchableGridDataTable.Copy();
                a.Clear();
                var b = searchableGridDataTable.Select(queryFilter);
                foreach (var item in b)
                {
                    a.ImportRow(item);
                }
                searchableGrid.DataSource = a;
            }
        }
        private void searchClearButton_Click(object? sender, EventArgs e)
        {
            foreach (DataGridViewCell cell in searchGrid.Rows[0].Cells)
            {
                cell.Value = DBNull.Value;
            }
            searchableGrid.DataSource = resetSearchableGridData();
        }
    }
}
