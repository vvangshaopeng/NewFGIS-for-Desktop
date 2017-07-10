using DotSpatial.Data;
using DotSpatial.Symbology;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Data;
using DotSpatial.Controls;
using Microsoft.Win32;

namespace DotSpatialGISManager
{
    /// <summary>
    /// FieldCalculatorDlg.xaml 的交互逻辑
    /// </summary>
    public partial class MergeDlg : Window, INotifyPropertyChanged
    {
        private IFeatureLayer[] m_FeaLyrList = new IFeatureLayer[] { };
        private IFeatureLayer m_CurrentFeaLyr = null;
        private IFeatureSet m_MergeFeaSet = null;
        private IFeatureSet m_InputFeaSet = null;
        private IFeatureSet m_ResultFeaset = null;

        #region 绑定

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public MergeDlg()
        {
            InitializeComponent();
            this.Owner = MainWindow.m_MainWindow;
            this.DataContext = this;

            //获取视图中图层列表
            m_FeaLyrList = MainWindow.m_DotMap.GetFeatureLayers();
            foreach (ILayer layer in m_FeaLyrList)
            {
                if (layer is IFeatureLayer)
                    this.cboLayer.Items.Add((layer as FeatureLayer).Name);
            }
            if (this.cboLayer.Items.Count > 0)
                this.cboLayer.SelectedIndex = 0;
        }



        private void cboLayer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.cboLayer.SelectedIndex == -1) return;
            m_CurrentFeaLyr = m_FeaLyrList[this.cboLayer.SelectedIndex];
            m_InputFeaSet = (m_CurrentFeaLyr as FeatureLayer).FeatureSet;
            MainWindow.m_DotMap.FunctionMode = FunctionMode.Select;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            //create a new featureset
            m_ResultFeaset = new FeatureSet(m_InputFeaSet.FeatureType);
            foreach (DataColumn column in m_InputFeaSet.DataTable.Columns)
            {
                DataColumn col = new DataColumn(column.ColumnName, column.DataType);
                m_ResultFeaset.DataTable.Columns.Add(col);
            }

            //merge
            m_MergeFeaSet = new FeatureSet(m_InputFeaSet.FeatureType);
            m_MergeFeaSet = m_CurrentFeaLyr.Selection.ToFeatureSet();
            if (m_MergeFeaSet.Features.Count < 2)
            {
                MessageBox.Show("Please select at least two features！");
                return;
            }
            IFeature MergeFea = m_MergeFeaSet.GetFeature(0);
            for (int i = 0; i < m_MergeFeaSet.Features.Count; i++)
            {
                var fea = m_MergeFeaSet.GetFeature(i);
                MergeFea = MergeFea.Union(fea.Geometry);
                if (MergeFea == null)
                {
                    break;
                }
            }
            IFeature lFeaM = m_ResultFeaset.AddFeature(MergeFea.Geometry);
            for (int i = 0; i < m_MergeFeaSet.Features[0].DataRow.ItemArray.Count(); i++)
            {
                lFeaM.DataRow[i] = m_MergeFeaSet.Features[0].DataRow[i];
            }

            m_CurrentFeaLyr.InvertSelection();
            IFeatureSet fs = m_CurrentFeaLyr.Selection.ToFeatureSet();
            foreach (var fea in fs.Features)
            {
                IFeature lFea = m_ResultFeaset.AddFeature(fea.Geometry);
                for (int i = 0; i < fea.DataRow.ItemArray.Count(); i++)
                {
                    lFea.DataRow[i] = fea.DataRow[i];
                }
            }
            m_ResultFeaset.InitializeVertices();
            MainWindow.m_DotMap.ResetBuffer();

            m_ResultFeaset.Projection = MainWindow.m_DotMap.Projection;
            m_ResultFeaset.Name = m_InputFeaSet.Name;
            m_CurrentFeaLyr = MainWindow.m_DotMap.Layers.Add(m_ResultFeaset);
            m_CurrentFeaLyr.LegendText = m_ResultFeaset.Name + "_copy";

            MainWindow.m_DotMap.Refresh();
            IFeatureLayer[] m_FeaLyrList = MainWindow.m_DotMap.GetFeatureLayers();
            for (int i = 0; i < MainWindow.m_DotMap.Layers.Count; i++)
            {
                m_FeaLyrList[i].ClearSelection();
            }
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
                this.txtPath.Text = f.FileName;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            UCControls.UCVectorDataEditing.m_MergeDlg = null;
        }
    }
}
