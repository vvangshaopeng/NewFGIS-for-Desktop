using DotSpatial.Controls;
using DotSpatial.Data;
using DotSpatial.Symbology;
using DotSpatialGISManager.Enum;
using GeoAPI.Geometries;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
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
    /// CreateFeatureDlg.xaml 的交互逻辑
    /// </summary>
    public partial class CreatePolylineDlg : Window
    {
        private IFeatureSet m_CurrentFeaset = null;
        private Dictionary<int, ILayer> m_DicLayer = null;
        private IFeatureSet m_ResultFeaset = null;
        private bool HasSaved = false;
        public ILayer m_PolylineLayer
        {
            get;
            private set;
        }

        private List<Coordinate> _CoordList;
        public List<Coordinate> CoordList
        {
            get
            {
                return _CoordList ?? (_CoordList = new List<Coordinate>());
            }
            set
            {
                _CoordList = value;
            }
        }

        private bool _IsFirstPoint = true;
        public bool IsFirstPoint
        {
            get
            {
                return _IsFirstPoint;
            }
            set
            {
                _IsFirstPoint = value;
            }
        }

        public CreatePolylineDlg()
        {
            InitializeComponent();
            this.Owner = MainWindow.m_MainWindow;
            MainWindow.m_AddFeaType = FeaType.Polyline;
            m_DicLayer = new Dictionary<int, ILayer>();
            var layers = MainWindow.m_DotMap.GetLineLayers();
            foreach (var layer in layers)
            {
                m_DicLayer.Add(this.cboLayer.Items.Count, layer);
                this.cboLayer.Items.Add(layer.LegendText);
            }
            if (this.cboLayer.Items.Count > 0)
            {
                this.cboLayer.SelectedIndex = 0;
            }
        }

        private void cboLayer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.cboLayer.SelectedIndex == -1) return;
            var layer = m_DicLayer[this.cboLayer.SelectedIndex];
            m_CurrentFeaset = (layer as FeatureLayer).FeatureSet;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (this.m_CurrentFeaset == null)
            {
                MessageBox.Show("Please select a target layer");
                return;
            }
            if (this.txtPath.Text == "")
            {
                MessageBox.Show("Please select save path");
                return;
            }
            this.groupAdd.IsEnabled = false;
            this.btnEnd.IsEnabled = true;
            //start 
            MainWindow.m_DotMap.Cursor = System.Windows.Forms.Cursors.Cross;
            //创建新的FeatureSet 复制要素
            m_ResultFeaset = new FeatureSet(FeatureType.Line);
            foreach (DataColumn column in m_CurrentFeaset.DataTable.Columns)
            {
                DataColumn col = new DataColumn(column.ColumnName, column.DataType);
                m_ResultFeaset.DataTable.Columns.Add(col);
            }
            foreach (var fea in m_CurrentFeaset.Features)
            {
                IFeature pFea = m_ResultFeaset.AddFeature(fea.Geometry);
                for (int i = 0; i < fea.DataRow.ItemArray.Count(); i++)
                {
                    pFea.DataRow[i] = fea.DataRow[i];
                }
            }
            m_ResultFeaset.Projection = MainWindow.m_DotMap.Projection;
            m_ResultFeaset.Name = m_CurrentFeaset.Name;
            m_PolylineLayer = MainWindow.m_DotMap.Layers.Add(m_ResultFeaset);
            m_PolylineLayer.LegendText = m_ResultFeaset.Name + "_copy";
        }

        private void btnEnd_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.m_DotMap.Cursor = System.Windows.Forms.Cursors.Default;
            if (m_ResultFeaset!=null)
            {
                m_ResultFeaset.SaveAs(this.txtPath.Text, true);
                MessageBox.Show("Save successfully,the layer has been added in map");
                HasSaved = true;
            }
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            UCControls.UCVectorDataEditing.m_CreatePolylineDlg = null;
            CoordList.Clear();
            MainWindow.m_AddFeaType = FeaType.None;
            if (!HasSaved)
            {
                MainWindow.m_DotMap.Layers.Remove(m_PolylineLayer as IMapLayer);
            }
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog f = new SaveFileDialog();
            f.AddExtension = true;
            f.Filter = "ShapeFile(*.shp)|*.shp";
            f.Title = "Select Save Path";
            if (f.ShowDialog() == true)
            {
                this.txtPath.Text = f.FileName;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (m_ResultFeaset != null)
            {
                if (MessageBox.Show("Save changes?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    m_ResultFeaset.SaveAs(this.txtPath.Text, true);
                    MessageBox.Show("Save successfully,the layer has been added in map");
                }
            }
            this.Close();
        }
    }
}
