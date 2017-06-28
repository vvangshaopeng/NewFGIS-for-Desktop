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
using Common;

namespace DotSpatialGISManager
{
    /// <summary>
    /// OverlayIntersectDlg.xaml 的交互逻辑
    /// </summary>
    public partial class OverlayIntersectDlg : Window
    {
        private List<ILayer> m_LayerList = null;
        private IFeatureSet m_SourceFeaSet = null;
        private IFeatureSet m_TargetFeaSet = null;

        public OverlayIntersectDlg()
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

            var m_OutputFeaSet = IntersectFeatures(m_TargetFeaSet, m_SourceFeaSet);
            if (m_OutputFeaSet != null)
            {
                m_OutputFeaSet.Name = System.IO.Path.GetFileNameWithoutExtension(this.txtSavePath.Text);
                m_OutputFeaSet.SaveAs(this.txtSavePath.Text, true);
                m_OutputFeaSet.Projection = MainWindow.m_DotMap.Projection;

                MainWindow.m_DotMap.Layers.Add(m_OutputFeaSet);
                MessageBox.Show("Successfully");
            }
            else
            {
                MessageBox.Show("Failed");
            }
            this.Close();
        }

        private FeatureSet IntersectFeatures(IFeatureSet targetFeatures, IFeatureSet sourceFeatures)
        {
            if (targetFeatures == null || sourceFeatures == null)
            {
                return null;
            }
            FeatureSet resultFeatures = new FeatureSet(); // the resulting featureset
            resultFeatures.CopyTableSchema(targetFeatures); // set up the data table in the new feature set

            ProgressBox p = new ProgressBox(0, 100, "Self intersection check progress");
            p.ShowPregress();
            p.SetProgressValue(0);
            p.SetProgressDescription("Overlay Intersect...");
            double pi = Math.Round((double)(1.0 * 100 / targetFeatures.Features.Count), 2);

            for (int i = 0; i < targetFeatures.Features.Count ; i++)
            {
                p.SetProgressValue(i * pi + pi);
                p.SetProgressDescription2(string.Format("{0} feature(s) is(are) checked, the remaining {1} feature(s) is(are) being queried", i + 1, targetFeatures.Features.Count - i - 1));
                var tf = targetFeatures.GetFeature(i); // get the full undifferenced feature
                for (int j = 0; j < sourceFeatures.Features.Count ; j++)
                {
                    var sf = sourceFeatures.GetFeature(j);
                    if (sf.Geometry.Intersects(tf.Geometry))
                    {
                        tf = tf.Intersection(sf.Geometry); // clip off any pieces of SF that overlap FR
                    }
                    if (tf == null)
                    {
                        break;
                    }
                }
                if (tf != null)
                {
                    resultFeatures.AddFeature(tf.Geometry).CopyAttributes(targetFeatures.GetFeature(i));
                }
            }
            p.CloseProgress();
            return resultFeatures;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}