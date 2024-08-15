/*
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
*/
namespace AssetStatusInfo
{
    internal class RadioButtonColumn
    {
        private List<string> checkBoxColumnNames = new List<string>();
        private Dictionary<string, List<string>> tablesWithCheckboxColumns = new Dictionary<string, List<string>>();
        private Dictionary<string, int> selectedCell = new Dictionary<string, int>();
        //coloring 
        private int penThickness = 2; 
        private Brush borderColor = Brushes.Black;
        private Brush fillColor = Brushes.LightBlue;
        public Dictionary<string, int> SelectedCell
        {
            get => selectedCell;
        }
        public RadioButtonColumn()
        {

        }
        public RadioButtonColumn(DataGridViewCheckBoxColumn checkboxColumn, int PenThickness, Brush BorderColor, Brush FillColor)
        {
            checkBoxColumnNames.Add(checkboxColumn.Name);
            penThickness = PenThickness;
            borderColor = BorderColor;
            fillColor = FillColor;
        }
        public RadioButtonColumn(List<DataGridViewCheckBoxColumn> CheckBoxColumn, int PenThickness, Brush BorderColor, Brush FillColor)
        {
            CheckBoxColumn.ForEach(checkboxColumn => { checkBoxColumnNames.Add(checkboxColumn.Name); });
            penThickness = PenThickness;
            borderColor = BorderColor;
            fillColor = FillColor;
        }

        public void ConvertToRadioButtonColumn(DataGridView grid, DataGridViewCheckBoxColumn checkboxColumn)
        //single point of entry for adding all event listeners 
        {
            if (checkboxColumn.Name != "") //if checkboxColumnName is not specified, all checkBox columns become radio button columns
            {
                checkBoxColumnNames.Add(checkboxColumn.Name);
                selectedCell[checkboxColumn.Name] = -1;
                List<string>? test;
                tablesWithCheckboxColumns.TryGetValue(grid.Name, out test);
                if (test != null)
                {
                    tablesWithCheckboxColumns[grid.Name].Add(checkboxColumn.Name);
                }
                else
                {
                    tablesWithCheckboxColumns[grid.Name] = new List<string> { checkboxColumn.Name };
                }
            }
            grid.CellEndEdit += this.StateCheckOnCellEndEdit; 
            grid.CurrentCellDirtyStateChanged += this.DeselectPreviousOption;
            grid.CellPainting += this.RadioButtonPainter;
            grid.DataSourceChanged += Grid_DataSourceChanged;
            grid.CellMouseClick += Grid_CellMouseClick;
            grid.DataBindingComplete += Grid_DataSourceChanged;
        }

        private void Grid_CellMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
        //checks for click within the drawn circle
        {
            var valid = ValidateColumn(sender, e.ColumnIndex);
            if (valid.valid && e.ColumnIndex > -1 && e.RowIndex > -1)
            {
                int width = valid.grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Size.Width;
                int height = valid.grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Size.Height;
                int xCell = valid.grid.Rows[e.RowIndex].Cells[e.ColumnIndex].ContentBounds.X;
                int yCell = valid.grid.Rows[e.RowIndex].Cells[e.ColumnIndex].ContentBounds.Y;
                int xClick = e.Location.X;
                int yClick = e.Location.Y;
                int radius;
                if (width < height)
                {
                    radius = width;
                }
                else
                {
                    radius = height;
                }
                if(((xClick - xCell)*(xClick - xCell)+ (yClick - yCell)* (yClick - yCell)) < ((radius / 2)*(radius / 2)))
                {
                    valid.grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = true;
                    DeselectPreviousOption(sender, e);
                }
            }
        }
        private void Grid_DataSourceChanged(object? sender, EventArgs e)
        //if I'm inserting values into the grid I need to adjust the selected cell
        {
            var valid = ValidateColumn(sender, -1);
            if (valid.grid != null)
            {
                DataGridView s = valid.grid;
                List<int> checkBoxColumns = new List<int>();
                for (int i = 0; i < s.Columns.Count; i++)
                {
                    if (ValidateColumn(sender, i).valid)
                    {
                        if (selectedCell[s.Columns[i].Name] > -1)
                        {
                                selectedCell[s.Columns[i].Name] = -1; //if nothing is found 
                                for (int j = 0; j < s.RowCount; j++)
                                {
                                    if (s.Rows[j].Cells[i].Value == null ? false : (bool)s.Rows[j].Cells[i].Value == true)
                                    {
                                        selectedCell[s.Columns[i].Name] = j;
                                    }
                                }
                        }
                    }
                }
            }
        }

        public void RadioButtonPainter(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            var valid = ValidateColumn(sender, e.ColumnIndex);
            if (valid.valid && e.RowIndex != -1)
            {
                DataGridView s = valid.grid;
                e.Handled = true;
                //Rectangle b = new Rectangle();
                Pen pen = new Pen(borderColor, penThickness);
                Size size = e.CellBounds.Size;
                if (size.Width < size.Height)
                {
                    size.Width = size.Width - penThickness - 3 * penThickness;
                    size.Height = size.Width;
                }
                else
                {
                    size.Height = size.Height - penThickness - 3 * penThickness;
                    size.Width = size.Height;
                }
                Point point = new Point(e.CellBounds.X + (e.CellBounds.Width - size.Width) / 2, e.CellBounds.Y + (e.CellBounds.Height - size.Height) / 2); //fix misalignment with interior dot
                Rectangle rectangle = new Rectangle(point, size);
                e.PaintBackground(rectangle, false);
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.DrawEllipse(pen, rectangle);
                rectangle.X += 2 * penThickness;
                rectangle.Y += 2 * penThickness;
                rectangle.Width = size.Width - 4 * penThickness;
                rectangle.Height = size.Height - 4 * penThickness;
                bool cellValue = true;
                if (s.CurrentCell.RowIndex == e.RowIndex && s.CurrentCell.ColumnIndex == e.ColumnIndex) //no other way to check if it's dirty
                {
                    if (s.CurrentCell.Value == null) { cellValue = false; }
                    else { cellValue = (bool)s.CurrentCell.Value; }
                    if ((s.IsCurrentCellDirty == true && cellValue == false) || cellValue)
                    {
                        pen.Brush = fillColor;
                        e.Graphics.DrawEllipse(pen, rectangle);
                        e.Graphics.FillEllipse(fillColor, rectangle);
                    }
                    else
                    {
                        pen.Color = s.Rows[e.RowIndex].DefaultCellStyle.BackColor;
                    }
                }
                else if (e.Value != null)
                {
                    if ((bool)e.Value == true)
                    {
                        pen.Brush = fillColor;
                        e.Graphics.DrawEllipse(pen, rectangle);
                        e.Graphics.FillEllipse(fillColor, rectangle);
                    }
                    else
                    {
                        pen.Color = e.CellStyle.BackColor;
                    }
                }
            }
        }

        private (DataGridView? grid, bool valid) ValidateColumn(object? sender, int columnIndex, bool useCurrentCellColumnIndex=false)
        {
            DataGridView? s = null;
            if (sender != null)
            {
                if (sender.GetType() == typeof(DataGridView))
                {
                    s = (DataGridView)sender;
                    if(useCurrentCellColumnIndex)
                    {
                        columnIndex = s.CurrentCell.ColumnIndex;
                    }
                    else if(s.Columns.Count <= columnIndex || columnIndex < 0)
                    {
                        return (s, false);
                    }
                    if (s.Columns[columnIndex].CellType.Name == typeof(DataGridViewCheckBoxCell).Name && (checkBoxColumnNames.Count > 0 ? checkBoxColumnNames.Contains(s.Columns[columnIndex].Name) : true))
                    {
                        return (s, true);
                    }
                    //else{return (s, false);}
                }
            }
            return (s, false);
        }

        public void Deselect(DataGridView sender, string checkBoxColumnName)
        {
            if (selectedCell[checkBoxColumnName] > -1)
            {
                for(int i = 0; i < sender.Columns.Count; i++) 
                {
                    if (ValidateColumn(sender, i).valid)
                    {
                        sender.Rows[selectedCell[checkBoxColumnName]].Cells[i].Value = false;
                        selectedCell[checkBoxColumnName] = -1;
                    }
                }
            }
        }
        private void DeselectPreviousOption(object? sender, EventArgs e)
        //enforces single selection
        {
            var valid = ValidateColumn(sender, 0, true);
            
            if (valid.valid)
            {
                DataGridView s = valid.grid;
                if (selectedCell[s.Columns[s.CurrentCell.ColumnIndex].Name] != s.CurrentCell.RowIndex && selectedCell[s.Columns[s.CurrentCell.ColumnIndex].Name] > -1)
                {
                    Deselect(s, s.Columns[s.CurrentCell.ColumnIndex].Name);
                }
                selectedCell[s.Columns[s.CurrentCell.ColumnIndex].Name] = s.CurrentCell.RowIndex;
            }
        }

        private void StateCheckOnCellEndEdit(object? sender, DataGridViewCellEventArgs e)
        //prevents de-selection 
        {
            var valid = ValidateColumn(sender, e.ColumnIndex);
            if (valid.valid)
            {
                DataGridView s = valid.grid;
                if (e.RowIndex == selectedCell[s.Columns[s.CurrentCell.ColumnIndex].Name])
                {
                    if (s.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null)
                    {
                        if ((bool)s.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != true)
                        {
                            s.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = true;
                            //selectedCell[s.Columns[s.CurrentCell.ColumnIndex].Name] = -1;
                        }
                    }
                }
            }
        }
    }
}
