using DotSpatial.Controls;
using DotSpatial.Data;
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
using Common;
using System.Drawing;
using System.Data;
using System.Collections.ObjectModel;
using System.ComponentModel;
using GeoAPI.Geometries;
using Microsoft.Win32;

namespace DotSpatialGISManager
{
    /// <summary>
    /// FayingSurfaceDlg.xaml 的交互逻辑
    /// </summary>
    public partial class RotateFeatureDlg : Window, INotifyPropertyChanged
    {
        private IFeatureLayer[] m_FeaLyrList = new IFeatureLayer[] { };
        private IFeatureLayer m_CurrentFeaLyr = null;
        private IFeatureSet m_InputFeaSet = null;
        private IFeatureSet m_ResultFeaset = null;
        private IFeature selectFea = null;

        #region 绑定

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public RotateFeatureDlg()
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
            //若未选择图层
            if (this.cboLayer.Text == "")
            {
                MessageBox.Show("Please select a layer！");
                return;
            }
            if (m_CurrentFeaLyr.Selection.Count == 0 || m_CurrentFeaLyr.Selection.Count > 1)
            {
                MessageBox.Show("Please select one feature");
                m_CurrentFeaLyr.UnSelectAll();
                return;
            }
            IFeature pFeature = m_CurrentFeaLyr.Selection.ToFeatureList()[0];
            selectFea = (from u in (m_CurrentFeaLyr as FeatureLayer).FeatureSet.Features
                         where u.Geometry.Intersects(pFeature.Geometry)
                         select u).FirstOrDefault();

            //create a new featureset
            m_ResultFeaset = new FeatureSet(m_InputFeaSet.FeatureType);
            foreach (DataColumn column in m_InputFeaSet.DataTable.Columns)
            {
                DataColumn col = new DataColumn(column.ColumnName, column.DataType);
                m_ResultFeaset.DataTable.Columns.Add(col);
            }

            //rotate
            double ranAngle = 0;
            try
            {
                ranAngle = Convert.ToDouble(this.txtAngle.Text);
            }
            catch
            {
                MessageBox.Show("input a number");
            }
            selectFea.Rotate(selectFea.Geometry.Coordinates[0], ranAngle);
            IFeature lFeaM = m_ResultFeaset.AddFeature(selectFea.Geometry);
            for (int i = 0; i < selectFea.DataRow.ItemArray.Count(); i++)
            {
                lFeaM.DataRow[i] = selectFea.DataRow[i];
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
            UCControls.UCVectorDataEditing.m_RotateFeatureDlg = null;
        }


    }
}
