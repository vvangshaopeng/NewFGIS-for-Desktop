using DotSpatial.Symbology;
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
using DotSpatial.Controls;
using DotSpatial.Data;
using Microsoft.Win32;

namespace DotSpatialGISManager
{
    /// <summary>
    /// ExportShpDlg.xaml 的交互逻辑
    /// </summary>
    public partial class ExportShpDlg : Window
    {
        private List<ILayer> m_LayerList = null;
        private IFeatureSet m_CurrentFeaSet
        {
            get;
            set;
        }
        public ExportShpDlg()
        {
            InitializeComponent();
            //获取视图中图层列表
            m_LayerList = MainWindow.m_DotMap.GetAllLayers();
            foreach (ILayer layer in m_LayerList)
            {
                if (layer is IFeatureLayer)
                    this.cboLayer.Items.Add(layer.LegendText);
            }
            if (this.cboLayer.Items.Count > 0)
                this.cboLayer.SelectedIndex = 0;
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

        private void cboLayer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.cboLayer.SelectedIndex == -1) return;
            m_CurrentFeaSet = (m_LayerList[this.cboLayer.SelectedIndex] as FeatureLayer).FeatureSet;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (m_CurrentFeaSet == null)
            {
                MessageBox.Show("Please select a layer");
                return;
            }
            m_CurrentFeaSet.SaveAs(this.txtPath.Text,true);
            MessageBox.Show("Save successfully!");
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
