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
    /// BufferAnalysisDlg.xaml 的交互逻辑
    /// </summary>
    public partial class BufferAnalysisDlg : Window
    {
        private IFeatureLayer[] m_FeaLyrList = new IFeatureLayer[] { };
        private IFeatureLayer m_CurrentFeaLyr = null;
        private IFeatureSet m_InputFeaSet = null;
        private IFeatureSet m_OutputFeaSet = new FeatureSet(FeatureType.Polygon);

        private string[] m_bufferMethodArr = { "All features", "Selected features" };

        private string bufferMethod = "All features";

        private bool ifDissolve;

        ICancelProgressHandler cancelProgressHandler = null;
        public BufferAnalysisDlg()
        {
            InitializeComponent();
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
        }

        private void btnSelectOutputPath_Click(object sender, RoutedEventArgs e)
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

        private void cboMethod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<string> m_bufferMethodList = new List<string>(m_bufferMethodArr);
            if (this.cboMethod.SelectedIndex == -1) return;
            bufferMethod = m_bufferMethodList[this.cboMethod.SelectedIndex];
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (this.cboLayer.Text == "")
            {
                MessageBox.Show("Please select a input layer");
                return;
            }
            if (this.txtSavePath.Text.Trim() == "")
            {
                MessageBox.Show("Please select a save path");
                return;
            }

            if (bufferMethod == "Selected features")
            {
                FeatureSet fs = null;
                fs = m_CurrentFeaLyr.Selection.ToFeatureSet();
                m_InputFeaSet = fs as IFeatureSet;
            }

            double bufferDistance = 0;
            try
            {
                bufferDistance = Convert.ToDouble(this.txtBufferDis.Text);
            }
            catch
            {
                MessageBox.Show("input a number");
            }

            DotSpatial.Analysis.Buffer.AddBuffer(m_InputFeaSet, bufferDistance, m_OutputFeaSet, cancelProgressHandler);

            //dissolve
            if (ifDissolve == true)
            {
                m_OutputFeaSet.DataTable.Columns.Add("id");
                m_OutputFeaSet = FeatureSetExt.Dissolve(m_OutputFeaSet, m_OutputFeaSet.DataTable.Columns["id"].ColumnName);
                for(int i = 0;i<m_OutputFeaSet.Features.Count;i++)
                {
                    m_OutputFeaSet.Features[i].DataRow["id"] = i.ToString();
                }
            }
            else
            {
                m_OutputFeaSet.CopyTableSchema(m_InputFeaSet.DataTable);
                if (m_OutputFeaSet.Features.Count == m_InputFeaSet.Features.Count)
                {
                    for (int i = 0; i < m_OutputFeaSet.Features.Count; i++)
                    {
                        for (int j = 0; j < m_OutputFeaSet.DataTable.Columns.Count; j++)
                        {
                            m_OutputFeaSet.Features[i].DataRow[j] = m_InputFeaSet.Features[i].DataRow[j];
                        }
                    }
                }

            }
            //保存
            m_OutputFeaSet.Name = System.IO.Path.GetFileNameWithoutExtension(this.txtSavePath.Text);
            m_OutputFeaSet.SaveAs(this.txtSavePath.Text, true);
            m_OutputFeaSet.Projection = MainWindow.m_DotMap.Projection;
            //加载图层
            MainWindow.m_DotMap.Layers.Add(m_OutputFeaSet);
            MessageBox.Show("Successfully");
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ifDissolve = true;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ifDissolve = false;
        }
    }
}