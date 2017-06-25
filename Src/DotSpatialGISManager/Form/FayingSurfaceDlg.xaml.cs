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

namespace DotSpatialGISManager
{
    /// <summary>
    /// FayingSurfaceDlg.xaml 的交互逻辑
    /// </summary>
    public partial class FayingSurfaceDlg : Window
    {
        private List<IMapPolygonLayer> m_Layers = new List<IMapPolygonLayer>();
        public FayingSurfaceDlg()
        {
            InitializeComponent();
            m_Layers = MainWindow.m_DotMap.GetPolygonLayers().ToList();
            foreach(var layer in m_Layers)
            {
                this.cboLayer.Items.Add(layer.LegendText);
            }
            if (this.cboLayer.Items.Count > 0)
                this.cboLayer.SelectedIndex = 0;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (this.cboLayer.Text == "")
            {
                MessageBox.Show("Please select a polygon layer");
                return;
            }
            IFeatureLayer layer = m_Layers[this.cboLayer.SelectedIndex];
            IFeatureSet feaset = (layer as FeatureLayer).FeatureSet;
            ProgressBox p = new ProgressBox(0, 100, "Faying surface check progress");
            p.ShowPregress();
            p.SetProgressValue(0);
            p.SetProgressDescription("Checking faying surface...");
            double pi = Math.Round((double)(1.0 * 100 / feaset.Features.Count));
            List<int> FidList = new List<int>();
            for(int i = 0;i<feaset.Features.Count;i++)
            {
                p.SetProgressValue(i * pi + pi);
                p.SetProgressDescription2(string.Format("{0} feature(s) is(are) checked, the remaining {1} feature(s) is(are) being queried", i + 1, feaset.Features.Count - i - 1));
                for(int j = i+1;j<feaset.Features.Count;j++)
                {
                    if (feaset.Features[i].Geometry.Overlaps(feaset.Features[j].Geometry))
                    {
                        if (!FidList.Contains(i))
                            FidList.Add(i);
                        if (!FidList.Contains(j))
                            FidList.Add(j);
                    }
                }
            }
            p.CloseProgress();
            if (FidList.Count == 0)
            {
                MessageBox.Show(string.Format("{0} has no faying surface.",layer.LegendText));
            }
            else
            {
                layer.UnSelectAll();
                MainWindow.m_DotMap.Refresh();
                layer.Select(FidList);
                MessageBox.Show(string.Format("{0} has {1} faying surfaces.", layer.LegendText, FidList.Count.ToString()));
            }
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
