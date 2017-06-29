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
using NetTopologySuite.Geometries;
using GeoAPI.Geometries;

namespace DotSpatialGISManager
{
    /// <summary>
    /// FayingSurfaceDlg.xaml 的交互逻辑
    /// </summary>
    public partial class SelfIntersectionDlg : Window
    {
        private List<IMapLineLayer> m_Layers = new List<IMapLineLayer>();
        public SelfIntersectionDlg()
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
            ProgressBox p = new ProgressBox(0, 100, "Self intersection check progress");
            p.ShowPregress();
            p.SetProgressValue(0);
            p.SetProgressDescription("Checking Self intersection...");
            double pi = Math.Round((double)(1.0 * 100 / feaset.Features.Count),2);
            List<int> FidList = new List<int>();
            //DateTime dt = DateTime.Now;
            for (int i = 0; i < feaset.Features.Count; i++)
            {
                p.SetProgressValue(i * pi + pi);
                p.SetProgressDescription2(string.Format("{0} feature(s) is(are) checked, the remaining {1} feature(s) is(are) being queried", i + 1, feaset.Features.Count - i - 1));
                var LineList = GetLinesFromGeometry(feaset.Features[i].Geometry);
                //var LineList = GetAllLine(feaset.Features[i].Geometry);
                bool IsCrossed = false;
                for (int j = 0; j < LineList.Count; j++)
                {
                    for (int k = j+1; k < LineList.Count; k++)
                    {
                        if (LineList[j].Crosses(LineList[k]))
                        {
                            FidList.Add(feaset.Features[i].Fid);
                            IsCrossed = true;
                            break;
                        }
                    }
                    if (IsCrossed)
                        break;
                }
            }
            //DateTime dt2 = DateTime.Now;
            //var span = dt2 - dt;
            //MessageBox.Show(span.ToString());
            p.CloseProgress();
            if (FidList.Count == 0)
            {
                MessageBox.Show(string.Format("{0} has no Self intersection.", layer.LegendText));
            }
            else
            {
                layer.UnSelectAll();
                MainWindow.m_DotMap.Refresh();
                layer.Select(FidList);
                MessageBox.Show(string.Format("{0} has {1} Self intersection", layer.LegendText, FidList.Count.ToString()));
            }
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 把先要素根据一定规律拆成若干段，便于检查自相交
        /// </summary>
        private List<LineString> GetLinesFromGeometry(GeoAPI.Geometries.IGeometry polyline)
        {
            List<LineString> resultLineList = new List<LineString>();//确定为不自相交的折线的几何
            if (polyline.GeometryType != "LineString" && polyline.GeometryType!= "MultiLineString")
            {
                return resultLineList;
            }
            for(int s = 0;s<polyline.NumGeometries;s++)
            {
                var partLine = polyline.GetGeometryN(s);
                int count = partLine.Coordinates.Count();
                List<LineString> tempLineList = new List<LineString>();
                for (int i = 0; i < count; i++)
                {
                    if (i + 1 < count)
                    {
                        List<Coordinate> temp = new List<Coordinate>() { partLine.Coordinates[i], partLine.Coordinates[i + 1] };
                        LineString line = new LineString(temp.ToArray());
                        tempLineList.Add(line);
                    }
                }
                if (tempLineList.Count < 1) return resultLineList;
                LineString referenceLine = tempLineList[0];//参照线
                Coordinate start = referenceLine.Coordinates[0];
                Coordinate end = referenceLine.Coordinates[1];
                List<Coordinate> tempCoorList = new List<Coordinate>() { start, end };
                for (int i = 0; i < tempLineList.Count; i++)
                {
                    if (i + 1 < tempLineList.Count)
                    {
                        double xr = referenceLine.Coordinates[1].X - referenceLine.Coordinates[0].X;
                        double yr = referenceLine.Coordinates[1].Y - referenceLine.Coordinates[0].Y;
                        double x1 = tempLineList[i].Coordinates[1].X - tempLineList[i].Coordinates[0].X;
                        double y1 = tempLineList[i].Coordinates[1].Y - tempLineList[i].Coordinates[0].Y;
                        double x2 = tempLineList[i + 1].Coordinates[1].X - tempLineList[i + 1].Coordinates[0].X;
                        double y2 = tempLineList[i + 1].Coordinates[1].Y - tempLineList[i + 1].Coordinates[0].Y;
                        if ((x1 == 0 && y1 == 0) || (xr == 0 && yr == 0) || (x2 == 0 && y2 == 0))
                        {
                            end = tempLineList[i + 1].Coordinates[1];
                            tempCoorList.Add(end);
                            referenceLine = new LineString(new List<Coordinate>() { start, end }.ToArray());
                            continue;
                        }
                        //余弦值>0视为同方向
                        if ((double)(1.0 * (x1 * x2 + y1 * y2) / (Math.Sqrt(x1 * x1 + y1 * y1) * Math.Sqrt(x2 * x2 + y2 * y2))) > 0 &&
                            (double)(1.0 * (xr * x2 + yr * y2) / (Math.Sqrt(xr * xr + yr * yr) * Math.Sqrt(x2 * x2 + y2 * y2))) > 0)
                        {
                            end = tempLineList[i + 1].Coordinates[1];
                            tempCoorList.Add(end);
                            referenceLine = new LineString(new List<Coordinate>() { start, end }.ToArray());
                        }
                        else
                        {
                            resultLineList.Add(new LineString(tempCoorList.ToArray()));
                            referenceLine = tempLineList[i + 1];
                            start = referenceLine.Coordinates[0];
                            end = referenceLine.Coordinates[1];
                            tempCoorList = new List<Coordinate>() { start, end };
                        }
                    }
                    else
                    {
                        resultLineList.Add(new LineString(tempCoorList.ToArray()));
                    }
                }
            }
            return resultLineList;
        }

        private List<LineString> GetAllLine(GeoAPI.Geometries.IGeometry polyline)
        {
            List<LineString> resultLineList = new List<LineString>();
            if (polyline.GeometryType != "LineString") return resultLineList;
            int count = polyline.Coordinates.Count();
            for (int i = 0; i < count; i++)
            {
                if (i + 1 < count)
                {
                    List<Coordinate> temp = new List<Coordinate>() { polyline.Coordinates[i], polyline.Coordinates[i + 1] };
                    LineString line = new LineString(temp.ToArray());
                    resultLineList.Add(line);
                }
            }
            return resultLineList;
        }
    }
}
