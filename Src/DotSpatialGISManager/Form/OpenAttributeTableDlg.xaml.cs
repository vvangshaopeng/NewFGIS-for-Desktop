using DotSpatial.Controls;
using DotSpatial.Data;
using DotSpatial.Symbology;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
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

namespace DotSpatialGISManager
{
    /// <summary>
    /// SetScaleDlg.xaml 的交互逻辑
    /// </summary>
    public partial class OpenAttributeTableDlg : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private IMapFeatureLayer[] m_FealyrList = null;
        private bool m_HasChanged = false;

        private List<string> _layerlist;
        public List<string> LayerList
        {
            get
            {
                return _layerlist;
            }
            set
            {
                _layerlist = value;
            }
        }

        private DataTable _layerdatatable;
        public DataTable LayerDataTable
        {
            get
            {
                return _layerdatatable;
            }
            set
            {
                if (_layerdatatable != value)
                {
                    _layerdatatable = value;
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("LayerDataTable"));
                    }
                }

            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            MainWindow.m_OpenAttributeTableDlg = null;
            if (m_HasChanged)
            {
                if (MessageBox.Show("Save changes?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    btnSaveEdit_Click(null, null);
                }
            }
        }

        public OpenAttributeTableDlg()
        {
            InitializeComponent();
            this.DataContext = this;
            this.Owner = MainWindow.m_MainWindow;
            m_FealyrList = MainWindow.m_DotMap.GetFeatureLayers();
            List<string> LyrNameList = new List<string>();
            foreach (var layer in m_FealyrList)
            {
                LyrNameList.Add(layer.LegendText);
            }
            LayerList = LyrNameList;
            if (m_FealyrList.Count() > 0)
                this.cboLayer.SelectedIndex = 0;
        }

        private void cboLayer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.cboLayer.SelectedIndex == -1 || m_FealyrList == null) return;
            DataTable dt = m_FealyrList[this.cboLayer.SelectedIndex].DataSet.DataTable;
            DataTable copydt = new DataTable();
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                copydt.Columns.Add(dt.Columns[i].ColumnName);
            }
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = copydt.NewRow();
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    row[j] = dt.Rows[i][j].ToString();
                }
                copydt.Rows.Add(row);
            }
            LayerDataTable = copydt;
        }

        private void btnStartEdit_Click(object sender, RoutedEventArgs e)
        {
            this.dataGrid.IsReadOnly = false;
            this.btnStopEdit.IsEnabled = true;
            this.btnStartEdit.IsEnabled = false;
            m_HasChanged = true;
        }

        private void btnStopEdit_Click(object sender, RoutedEventArgs e)
        {
            this.dataGrid.IsReadOnly = true;
            this.btnStopEdit.IsEnabled = false;
            this.btnStartEdit.IsEnabled = true;
        }

        private void btnSaveEdit_Click(object sender, RoutedEventArgs e)
        {
            if (this.cboLayer.SelectedIndex >= 0)
            {
                m_FealyrList[this.cboLayer.SelectedIndex].DataSet.DataTable = LayerDataTable;
                (m_FealyrList[this.cboLayer.SelectedIndex] as FeatureLayer).FeatureSet.Save();
                MessageBox.Show("Save successfully!");
                m_HasChanged = false;
            }
        }

        private void btnFieldCalculator_Click(object sender, RoutedEventArgs e)
        {
            if (this.cboLayer.SelectedIndex >= 0)
            {
                FieldCalculatorDlg f = new FieldCalculatorDlg(m_FealyrList[this.cboLayer.SelectedIndex] as IFeatureLayer);
                f.Show();
            }
        }
    }
}
