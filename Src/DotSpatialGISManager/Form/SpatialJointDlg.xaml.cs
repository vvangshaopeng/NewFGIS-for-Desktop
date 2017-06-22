using DotSpatial.Data;
using DotSpatial.Symbology;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    /// SpatialJointDlg.xaml 的交互逻辑
    /// </summary>
    public partial class SpatialJointDlg : Window
    {
        private IFeatureLayer[] m_FeaLyrList = new IFeatureLayer[] { };
        private IFeatureLayer m_CurrentFeaLyr = null;
        private IFeatureSet m_RemoteFeaSet = null;

        public SpatialJointDlg()
        {
            InitializeComponent();
            m_FeaLyrList = MainWindow.m_DotMap.GetFeatureLayers();
            List<string> temp = new List<string>();
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
        }

        private void btnSelectFilePath_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog f = new OpenFileDialog();
            f.CheckFileExists = true;
            f.Title = "Select ShapeFile";
            f.Filter = "ShapeFile(*.shp)|*.shp";
            if (f.ShowDialog() == true)
            {
                this.txtFilePath.Text = f.FileName;
                m_RemoteFeaSet = Shapefile.Open(f.FileName);
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
            //新建一个FeatureSet 几何与原图层一致
            IFeatureSet pResultSet = (m_CurrentFeaLyr as FeatureLayer).FeatureSet.CopyFeatures(false);
            pResultSet.DataTable = Common.CHelp.CopyDataTable(m_CurrentFeaLyr.DataSet.DataTable);
            DataTable SrcDT = pResultSet.DataTable;
            DataTable TarDt = m_RemoteFeaSet.DataTable.Copy();
            //添加字段
            List<string> FiledsName = new List<string>();
            foreach (DataColumn col in TarDt.Columns)
            {
                if (SrcDT.Columns.Contains(col.ColumnName)) continue;
                DataColumn column = new DataColumn(col.ColumnName);
                SrcDT.Columns.Add(column);
                FiledsName.Add(col.ColumnName);
            }
            //相交查询
            int i = 0;
            foreach (var LayerFea in pResultSet.Features)
            {
                foreach (var remoteFea in m_RemoteFeaSet.Features)
                {
                    if (LayerFea.Geometry.IsEmpty) continue;
                    //找到第一个匹配项即返回
                    if (LayerFea.Geometry.Intersects(remoteFea.Geometry))
                    {
                        foreach (string colname in FiledsName)
                            SrcDT.Rows[i][colname]= remoteFea.DataRow[colname];
                        break;
                    }
                }
                i++;
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
