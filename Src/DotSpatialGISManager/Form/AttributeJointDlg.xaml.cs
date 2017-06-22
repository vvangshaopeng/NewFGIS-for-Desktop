using DotSpatial.Symbology;
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
using DotSpatialGISManager.Enum;
using System.Data;
using System.ComponentModel;
using System.Collections.ObjectModel;
using DotSpatial.Data;

namespace DotSpatialGISManager
{
    /// <summary>
    /// AttributeJointDlg.xaml 的交互逻辑
    /// </summary>
    public partial class AttributeJointDlg : Window, INotifyPropertyChanged
    {
        private IFeatureLayer[] m_FeaLyrList = new IFeatureLayer[] { };
        private IFeatureLayer m_CurrentFeaLyr = null;
        private DataTable m_CurrentDtTable = null;

        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<string> _LayerFieldsList;
        public ObservableCollection<string> LayerFieldsList
        {
            get
            {
                return _LayerFieldsList;
            }
            set
            {
                if (_LayerFieldsList != value)
                {
                    _LayerFieldsList = value;
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("LayerFieldsList"));
                    }
                }
            }
        }

        private ObservableCollection<string> _JoinFieldsList;
        public ObservableCollection<string> JoinFieldsList
        {
            get
            {
                return _JoinFieldsList;
            }
            set
            {
                if (_JoinFieldsList != value)
                {
                    _JoinFieldsList = value;
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("JoinFieldsList"));
                    }
                }
            }
        }

        public AttributeJointDlg()
        {
            InitializeComponent();
            this.DataContext = this;
            m_FeaLyrList = MainWindow.m_DotMap.GetFeatureLayers();
            ObservableCollection<string> temp = new ObservableCollection<string>();
            foreach (var lyr in m_FeaLyrList)
            {
                temp.Add(lyr.LegendText);
            }
            this.cboLayer.ItemsSource = temp;
            if (this.cboLayer.Items.Count > 0)
                this.cboLayer.SelectedIndex = 0;
        }

        private void cboLayer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.cboLayer.SelectedIndex == -1) return;
            m_CurrentFeaLyr = m_FeaLyrList[this.cboLayer.SelectedIndex];
            ObservableCollection<string> temp = new ObservableCollection<string>();
            foreach (DataColumn col in m_CurrentFeaLyr.DataSet.DataTable.Columns)
            {
                temp.Add(col.ColumnName);
            }
            LayerFieldsList = temp;
            if (LayerFieldsList.Count > 0)
                this.cboField.SelectedIndex = 0;
        }

        private void btnSelectFilePath_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<string> temp = new ObservableCollection<string>();
            OpenFileDialog f = new OpenFileDialog();
            f.CheckFileExists = true;
            f.Title = "Select Excel or ShapeFile";
            f.Filter = "All Files|*.xlsx;*.xls;*.shp|Excel File(*.xlsx)|*.xlsx|Excel File(*.xls)|*.xls|ShapeFile(*.shp)|*.shp";
            if (f.ShowDialog() == true)
            {
                this.txtFilePath.Text = f.FileName;
                switch (System.IO.Path.GetExtension(f.FileName))
                {
                    case ".xlsx":
                    case ".xls":
                        Workbook workbook = new Workbook();
                        workbook.Open(f.FileName);
                        Cells cells = workbook.Worksheets[0].Cells;
                        m_CurrentDtTable = cells.ExportDataTable(0, 0, cells.MaxDataRow + 1, cells.MaxDataColumn + 1, true);
                        break;
                    case ".shp":
                        IFeatureSet pFeaSet = Shapefile.Open(f.FileName);
                        m_CurrentDtTable = pFeaSet.DataTable;
                        break;
                }
                if (m_CurrentDtTable != null)
                {
                    foreach (var col in m_CurrentDtTable.Columns)
                    {
                        temp.Add((col as DataColumn).ColumnName);
                    }
                    JoinFieldsList = temp;
                    if (JoinFieldsList.Count > 0)
                        this.cboJoinField.SelectedIndex = 0;
                }
            }
        }

        private void btnSelectSavePath_Click(object sender, RoutedEventArgs e)
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

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (this.cboLayer.Text == "")
            {
                MessageBox.Show("Please select a layer");
                return;
            }
            if (this.txtFilePath.Text.Trim() == "")
            {
                MessageBox.Show("Please select a remot file");
                return;
            }
            if (this.txtSavePath.Text.Trim() == "")
            {
                MessageBox.Show("Please select a save path");
                return;
            }
            if (this.cboField.Text == "" || this.cboJoinField.Text == "")
            {
                MessageBox.Show("Please select a join field");
                return;
            }
            //新建一个FeatureSet 几何与原图层一致
            IFeatureSet pResultSet = (m_CurrentFeaLyr as FeatureLayer).FeatureSet.CopyFeatures(false);
            pResultSet.DataTable = Common.CHelp.CopyDataTable(m_CurrentFeaLyr.DataSet.DataTable);
            DataTable SrcDT = pResultSet.DataTable;
            DataTable TarDt = m_CurrentDtTable.Copy();
            string SrcField = this.cboField.Text;
            string TarField = this.cboJoinField.Text;
            //添加字段
            List<string> FiledsName = new List<string>();
            foreach (DataColumn col in TarDt.Columns)
            {
                if (col.ColumnName == TarField) continue;
                if (SrcDT.Columns.Contains(col.ColumnName)) continue;
                DataColumn column = new DataColumn(col.ColumnName);
                SrcDT.Columns.Add(column);
                FiledsName.Add(col.ColumnName);
            }
            //遍历查找匹配值
            foreach (DataRow row in SrcDT.Rows)
            {
                DataRow[] resultRows = TarDt.Select(string.Format("{0} = '{1}'", SrcField, row[TarField]));
                //匹配找到的第一项
                foreach (DataRow resultRow in resultRows)
                {
                    foreach (string colname in FiledsName)
                    {
                        row[colname] = resultRow[colname];
                    }
                    break;
                }
            }
            //保存
            pResultSet.Name = System.IO.Path.GetFileNameWithoutExtension(this.txtSavePath.Text);
            pResultSet.SaveAs(this.txtSavePath.Text, true);
            //加载图层
            MainWindow.m_DotMap.Layers.Add(pResultSet);
            MessageBox.Show("Successfully");
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
