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

namespace DotSpatialGISManager
{
    /// <summary>
    /// FayingSurfaceDlg.xaml 的交互逻辑
    /// </summary>
    public partial class SuspensionPointDlg : Window
    {
        private List<IMapLineLayer> m_Layers = new List<IMapLineLayer>();
        public SuspensionPointDlg()
        {
            InitializeComponent();
            m_Layers = MainWindow.m_DotMap.GetLineLayers().ToList();
            foreach (var layer in m_Layers)
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
                MessageBox.Show("Please select a polyline layer");
                return;
            }
            IFeatureLayer layer = m_Layers[this.cboLayer.SelectedIndex];
            IFeatureSet feaset = (layer as FeatureLayer).FeatureSet;
            ProgressBox p = new ProgressBox(0, 100, "Suspension point check progress");
            p.ShowPregress();
            p.SetProgressValue(0);
            p.SetProgressDescription("Checking suspension point...");
            Dictionary<int, List<GeoAPI.Geometries.IPoint>> AllPoints = new Dictionary<int, List<GeoAPI.Geometries.IPoint>>();//线图层所有端点
            for (int i = 0; i < feaset.Features.Count; i++)
            {
                IFeature pFea = feaset.Features[i];
                if (pFea.Geometry.GeometryType == "LineString")
                {
                    GeoAPI.Geometries.Coordinate coord1 = pFea.Geometry.Coordinates[0];
                    GeoAPI.Geometries.IPoint pPoint1 = new NetTopologySuite.Geometries.Point(coord1);
                    int count = pFea.Geometry.Coordinates.Count() - 1;
                    GeoAPI.Geometries.Coordinate coord2 = pFea.Geometry.Coordinates[count];
                    GeoAPI.Geometries.IPoint pPoint2 = new NetTopologySuite.Geometries.Point(coord2);
                    AllPoints.Add(pFea.Fid, new List<GeoAPI.Geometries.IPoint>() { pPoint1, pPoint2 });
                }
                else//多线
                {
                    for (int j = 0; j < pFea.Geometry.NumGeometries; j++)
                    {
                        var geometry = pFea.Geometry.GetGeometryN(j);
                        GeoAPI.Geometries.Coordinate coord1 = geometry.Coordinates[0];
                        GeoAPI.Geometries.IPoint pPoint1 = new NetTopologySuite.Geometries.Point(coord1);
                        int count = geometry.Coordinates.Count() - 1;
                        GeoAPI.Geometries.Coordinate coord2 = geometry.Coordinates[count];
                        GeoAPI.Geometries.IPoint pPoint2 = new NetTopologySuite.Geometries.Point(coord2);
                        if (AllPoints.ContainsKey(pFea.Fid))
                        {
                            if (!AllPoints[pFea.Fid].Contains(pPoint1))
                                AllPoints[pFea.Fid].Add(pPoint1);
                            if (!AllPoints[pFea.Fid].Contains(pPoint2))
                                AllPoints[pFea.Fid].Add(pPoint2);
                        }
                        else
                            AllPoints.Add(pFea.Fid, new List<GeoAPI.Geometries.IPoint>() { pPoint1, pPoint2 });
                    }
                }
            }
            List<GeoAPI.Geometries.IPoint> resultPoint = new List<GeoAPI.Geometries.IPoint>();
            double pi = Math.Round((double)(1.0 * 100 / feaset.Features.Count), 2);
            int number = 1;
            foreach (var value in AllPoints)
            {
                p.SetProgressValue(number * pi);
                p.SetProgressDescription2(string.Format("{0} feature(s) is(are) checked, the remaining {1} feature(s) is(are) being queried", number, feaset.Features.Count - number));
                number++;
                foreach (var point in value.Value)
                {
                    bool IsSuspension = true;
                    foreach (var fea in feaset.Features)
                    {
                        if (fea.Fid == value.Key)
                        {
                            continue;
                        }
                        //一旦相交，必不是悬点
                        if (fea.Geometry.Intersects(point))
                        {
                            IsSuspension = false;
                            break;
                        }
                    }
                    if (IsSuspension && !resultPoint.Contains(point))
                    {
                        resultPoint.Add(point);
                    }
                }
            }
            AllPoints.Clear();
            p.CloseProgress();
            if (resultPoint.Count == 0)
            {
                MessageBox.Show(string.Format("{0} has no suspension point.", layer.LegendText));
            }
            else
            {
                IFeatureSet pSet = new FeatureSet(FeatureType.Point);
                string[] Fields = new string[3] { "ID", "X", "Y" };
                foreach (string field in Fields)
                {
                    pSet.DataTable.Columns.Add(field);
                }
                int s = 0;
                foreach (var point in resultPoint)
                {
                    IFeature pFea = pSet.AddFeature(point);
                    pFea.DataRow[0] = s;
                    pFea.DataRow[1] = point.X;
                    pFea.DataRow[2] = point.Y;
                    s++;
                }
                pSet.Projection = MainWindow.m_DotMap.Projection;
                pSet.Name = m_Layers[this.cboLayer.SelectedIndex].LegendText + "_suspenion points";
                var feaLayer = MainWindow.m_DotMap.Layers.Add(pSet);
                PointSymbolizer symbol = new PointSymbolizer(System.Drawing.Color.Red, DotSpatial.Symbology.PointShape.Ellipse, 5);
                feaLayer.Symbolizer = symbol;
                MessageBox.Show(string.Format("{0} has {1} suspension point.", layer.LegendText, resultPoint.Count.ToString()));
            }
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
