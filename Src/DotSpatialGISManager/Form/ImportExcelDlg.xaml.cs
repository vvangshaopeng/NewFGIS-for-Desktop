using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Aspose.Cells;
using System.Data;
using DotSpatial.Data;
using GeoAPI.Geometries;
using NetTopologySuite;

namespace DotSpatialGISManager
{
    /// <summary>
    /// ImportExcelDlg.xaml 的交互逻辑
    /// </summary>
    public partial class ImportExcelDlg : Window
    {
        public ImportExcelDlg()
        {
            InitializeComponent();
        }

        public IFeatureSet ResultFeaSet
        {
            get;
            private set;
        }

        public DataTable m_DataTable
        {
            get;
            private set;
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog f = new OpenFileDialog();
            f.CheckFileExists = true;
            f.Title = "Select Excel File";
            f.Filter = @"Excel File(*.xlsx;*.xls)|*.xlsx;*.xls";
            if (f.ShowDialog() == true)
            {
                string path = f.FileName;
                this.txtPath.Text = path;
                Workbook workbook = new Workbook();
                workbook.Open(path);
                Cells cells = workbook.Worksheets[0].Cells;
                if (cells.MaxDataColumn < 0 || cells.MaxDataRow < 0)
                {
                    MessageBox.Show("This Excel is empty");
                    return;
                }
                if (cells.MaxColumn<1)
                {
                    MessageBox.Show("This Excel file should have 2 columns at least");
                    return;
                }
                //字段名列表
                List<string> FieldName = new List<string>();
                for (int j = 0; j < cells.MaxDataColumn + 1; j++)
                {
                    string strValue = cells[0, j].StringValue.Trim();
                    FieldName.Add(strValue);
                }
                m_DataTable = cells.ExportDataTable(0, 0, cells.MaxDataRow + 1, cells.MaxDataColumn + 1, true);
                //赋字段名
                for (int i = 0; i < FieldName.Count; i++)
                {
                    m_DataTable.Columns[i].ColumnName = FieldName[i];
                }
                //绑定combobox
                this.cboX.ItemsSource = FieldName;
                this.cboY.ItemsSource = FieldName;
                this.cboX.SelectedIndex = 0;
                this.cboY.SelectedIndex = 1;
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (this.txtSavePath.Text == "")
            {
                MessageBox.Show("Please select a save path");
                return;
            }
            if (this.cboX.Text == ""||this.cboY.Text == "")
            {
                MessageBox.Show("Please select x y field");
                return;
            }
            string xField = this.cboX.Text;
            string yField = this.cboY.Text;
            IFeatureSet pFeaSet = new FeatureSet(FeatureType.Point);
            //使用当前视图的空间参考
            pFeaSet.Projection = MainWindow.m_DotMap.Projection;
            pFeaSet.DataTable = m_DataTable.Clone();
            pFeaSet.Name = this.txtName.Text;
            for (int i = 0;i<m_DataTable.Rows.Count;i++)
            {
                string xValue = m_DataTable.Rows[i][xField].ToString();
                string yValue = m_DataTable.Rows[i][yField].ToString();
                double x, y;
                //尝试转换成double类型
                if (double.TryParse(xValue, out x)&&double.TryParse(yValue,out y))
                {
                    Coordinate coor = new Coordinate(x, y);
                    IPoint point = new NetTopologySuite.Geometries.Point(coor);
                    IFeature feature = pFeaSet.AddFeature(point);
                    feature.DataRow = m_DataTable.Rows[i];
                }
                else
                {
                    MessageBox.Show("The x or y value in row" + (i+1)+ "is not number");
                    return;
                }
            }
            pFeaSet.SaveAs(this.txtSavePath.Text,true);
            ResultFeaSet = pFeaSet;
            this.DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog f = new SaveFileDialog();
            f.AddExtension = true;
            f.Filter = "ShapeFile(*.shp)|*.shp";
            f.Title = "Select Save Path";
            if (f.ShowDialog() == true)
            {
                this.txtSavePath.Text = f.FileName;
            }
        }
    }
}
