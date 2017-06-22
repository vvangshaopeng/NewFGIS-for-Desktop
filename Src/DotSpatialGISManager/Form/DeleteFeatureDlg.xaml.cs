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
    public partial class DeleteFeatureDlg : Window
    {
        private List<IMapFeatureLayer> m_LayerList = new List<IMapFeatureLayer>();
        private bool m_HasChanged = false;
        public DeleteFeatureDlg()
        {
            InitializeComponent();
            this.Owner = MainWindow.m_MainWindow;
            MainWindow.m_DotMap.FunctionMode = FunctionMode.Select;
            //获取视图中图层列表
            m_LayerList = MainWindow.m_DotMap.GetFeatureLayers().ToList();
            foreach (ILayer layer in m_LayerList)
                this.cboLayer.Items.Add(layer.LegendText);
            if (this.cboLayer.Items.Count > 0)
                this.cboLayer.SelectedIndex = 0;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (this.cboLayer.SelectedIndex == -1 || this.m_LayerList.Count <= this.cboLayer.SelectedIndex) return;
            (m_LayerList[this.cboLayer.SelectedIndex] as FeatureLayer).RemoveSelectedFeatures();
            (m_LayerList[this.cboLayer.SelectedIndex] as FeatureLayer).UnSelectAll();
            MainWindow.m_DotMap.ResetBuffer();
            MessageBox.Show("Delete features successful");
            m_HasChanged = true;
            this.btnSave.IsEnabled = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 全选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.cboLayer.SelectedIndex == -1 || this.m_LayerList.Count <= this.cboLayer.SelectedIndex) return;
            (m_LayerList[this.cboLayer.SelectedIndex] as FeatureLayer).SelectAll();
        }

        /// <summary>
        /// 反选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (this.cboLayer.SelectedIndex == -1 || this.m_LayerList.Count <= this.cboLayer.SelectedIndex) return;
            (m_LayerList[this.cboLayer.SelectedIndex] as FeatureLayer).UnSelectAll();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            UCControls.UCVectorDataEditing.m_DeleteFeatureDlg = null;
            if (m_HasChanged)
            {
                if (MessageBox.Show("Save changes?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    btnSave_Click(null, null);
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (m_HasChanged)
            {
                try
                {
                    foreach (var layer in m_LayerList)
                    {
                        (layer as FeatureLayer).FeatureSet.Save();
                        MessageBox.Show("Save successfully");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Save failed," + ex.Message);
                }
                m_HasChanged = false;
                this.btnSave.IsEnabled = false;
            }
        }

        private void cboLayer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.cboLayer.SelectedIndex == -1) return;
            IFeatureLayer pLayer = m_LayerList[this.cboLayer.SelectedIndex] as FeatureLayer;
            pLayer.SelectionChanged += PLayer_SelectionChanged;
            if (pLayer.Selection.Count > 0)
                this.btnOK.IsEnabled = true;
            else
                this.btnOK.IsEnabled = false;
        }

        private void PLayer_SelectionChanged(object sender, EventArgs e)
        {
            if (sender is FeatureLayer)
            {
                if ((sender as FeatureLayer).Selection.Count>0)
                {
                    this.btnOK.IsEnabled = true;
                }
                else
                {
                    this.btnOK.IsEnabled = false;
                }
            }
        }
    }
}
