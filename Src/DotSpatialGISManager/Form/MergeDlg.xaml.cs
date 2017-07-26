using DotSpatial.Controls;
using DotSpatial.Data;
using DotSpatial.Symbology;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DotSpatialGISManager
{
    /// <summary>
    /// MergeDlg.xaml 的交互逻辑
    /// </summary>
    public partial class MergeDlg : Window
    {
        private IFeatureLayer[] m_FeaLyrList = new IFeatureLayer[] { };
        private FeatureLayer m_CurrentFeaLyr = null;
        private FeatureSet m_InputFeaSet = null;
        private List<IMapLineLayer> m_Layers = new List<IMapLineLayer>();
        private IFeature lFeaM = null;

        public MergeDlg()
        {
            InitializeComponent();
            this.btnStartMerge.IsEnabled = false;
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
            m_CurrentFeaLyr = m_FeaLyrList[this.cboLayer.SelectedIndex] as FeatureLayer;
            m_InputFeaSet = (m_CurrentFeaLyr as FeatureLayer).FeatureSet as FeatureSet;
        }

        private void M_CurrentFeaLyr_SelectionChanged(object sender, EventArgs e)
        {
            if (m_CurrentFeaLyr.Selection.Count < 2) return;
            this.btnStartMerge.IsEnabled = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            UCControls.UCVectorDataEditing.m_MergeDlg = null;
        }
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            //若未选择图层
            if (this.cboLayer.Text == "")
            {
                MessageBox.Show("Please select a layer！");
                return;
            }
            this.btnOK.IsEnabled = false;
            //注册事件
            m_CurrentFeaLyr.SelectionChanged += M_CurrentFeaLyr_SelectionChanged;

            MainWindow.m_DotMap.FunctionMode = FunctionMode.Select;
        }

        private void btnStartMerge_Click(object sender, RoutedEventArgs e)
        {
            //注销事件
            m_CurrentFeaLyr.SelectionChanged -= M_CurrentFeaLyr_SelectionChanged;
            FeatureSet m_MergeFeaSet = new FeatureSet(m_InputFeaSet.FeatureType);
            m_MergeFeaSet = m_CurrentFeaLyr.Selection.ToFeatureSet();
            MergeFeature(m_MergeFeaSet);
            btnStartMerge.IsEnabled = false;
        }

        public void MergeFeature(FeatureSet m_MergeFeaSet)
        {
            if (m_MergeFeaSet.Features.Count < 2 || m_CurrentFeaLyr == null) return;

            //确保目标图层只选中编辑的那一个要素，因为后面会把选中要素移除
            //m_CurrentFeaLyr.UnSelectAll();
            //m_CurrentFeaLyr.Selection.Clear();

            //merge
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
            lFeaM = m_InputFeaSet.AddFeature(MergeFea.Geometry);
            m_CurrentFeaLyr.RemoveSelectedFeatures();

            MainWindow.m_DotMap.ResetBuffer();
            MainWindow.m_DotMap.Refresh();


            if (MessageBox.Show("Save edit?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                m_CurrentFeaLyr.FeatureSet.Save();
                MessageBox.Show("Save successfully!");
            }
            //移除图层重新加载，因为底层bug 移动节点之后选择要素会报错。
            MainWindow.m_AddFeaType = Enum.FeaType.None;
            MainWindow.m_DotMap.FunctionMode = FunctionMode.None;
            MainWindow.m_DotMap.Cursor = System.Windows.Forms.Cursors.Default;
            string shpPath = m_CurrentFeaLyr.FeatureSet.FilePath;
            string name = m_CurrentFeaLyr.LegendText;
            var symbol = m_CurrentFeaLyr.Symbolizer;
            var extent = m_CurrentFeaLyr.Extent;
            IFeatureSet s = Shapefile.Open(shpPath);
            MainWindow.m_DotMap.Layers.Remove(m_CurrentFeaLyr as IMapLayer);
            var result = MainWindow.m_DotMap.Layers.Add(s);
            result.Symbolizer = symbol;
            result.Projection = MainWindow.m_DotMap.Projection;
            result.LegendText = name;
            //result.Select((result as FeatureLayer).FeatureSet.Features[(result as FeatureLayer).FeatureSet.Features.Count - 1]);
            this.Close();
        }
    }
}