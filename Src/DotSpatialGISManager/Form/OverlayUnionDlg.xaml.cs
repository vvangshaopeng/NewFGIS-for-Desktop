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
using DotSpatial.Analysis;

namespace DotSpatialGISManager
{
    /// <summary>
    /// OverlayUnionDlg.xaml 的交互逻辑
    /// </summary>
    public partial class OverlayUnionDlg : Window
    {
        private List<ILayer> m_LayerList = null;
        private IFeatureSet m_SourceFeaSet = null;
        private IFeatureSet m_TargetFeaSet = null;

        public OverlayUnionDlg()
        {
            InitializeComponent();
            //获取视图中图层列表
            m_LayerList = MainWindow.m_DotMap.GetAllLayers();
            foreach (ILayer layer in m_LayerList)
            {
                if (layer is IFeatureLayer)
                    this.cboSourceLayer.Items.Add((layer as FeatureLayer).Name);
                this.cboTargetLayer.Items.Add((layer as FeatureLayer).Name);
            }
            if (this.cboSourceLayer.Items.Count > 0)
                this.cboSourceLayer.SelectedIndex = 0;
            if (this.cboTargetLayer.Items.Count > 0)
                this.cboTargetLayer.SelectedIndex = 0;
        }

        private void cboSourceLayer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.cboSourceLayer.SelectedIndex == -1) return;
            m_SourceFeaSet = (m_LayerList[this.cboSourceLayer.SelectedIndex] as FeatureLayer).FeatureSet;
        }

        private void cboTargetLayer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.cboTargetLayer.SelectedIndex == -1) return;
            m_TargetFeaSet = (m_LayerList[this.cboTargetLayer.SelectedIndex] as FeatureLayer).FeatureSet;
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
            if (m_SourceFeaSet == null)
            {
                MessageBox.Show("Please select a source layer");
                return;
            }
            if (m_TargetFeaSet == null)
            {
                MessageBox.Show("Please select a target layer");
                return;
            }
            if (this.txtSavePath.Text.Trim() == "")
            {
                MessageBox.Show("Please select a save path");
                return;
            }

            var m_OutputFeaSet = Overlay.AppendFeatures(m_TargetFeaSet as FeatureSet, m_SourceFeaSet as FeatureSet);
            //保存
            m_OutputFeaSet.Name = System.IO.Path.GetFileNameWithoutExtension(this.txtSavePath.Text);
            m_OutputFeaSet.SaveAs(this.txtSavePath.Text, true);
            m_OutputFeaSet.Projection = MainWindow.m_DotMap.Projection;

            MainWindow.m_DotMap.Layers.Add(m_OutputFeaSet);
            MessageBox.Show("Successfully");
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}