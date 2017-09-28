using DotSpatial.Controls;
using DotSpatial.Data;
using DotSpatial.Symbology;
using DotSpatialGISManager.Enum;
using GeoAPI.Geometries;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
    /// CreateFeatureDlg.xaml 的交互逻辑
    /// </summary>
    public partial class ConnectivityAnalysisDlg : Window
    {
        private IFeatureSet m_CurrentFeaset = null;
        private Dictionary<int, ILayer> m_DicLayer = null;
        private IFeatureSet m_ResultFeaset = null;
        private bool HasSaved = false;
        private IFeatureSet m_PointFeaSet;
        public ILayer m_PolylineLayer
        {
            get;
            private set;
        }

        private List<Coordinate> _CoordList;
        public List<Coordinate> CoordList
        {
            get
            {
                return _CoordList ?? (_CoordList = new List<Coordinate>());
            }
            set
            {
                _CoordList = value;
            }
        }

        private bool _IsFirstPoint = true;
        public bool IsFirstPoint
        {
            get
            {
                return _IsFirstPoint;
            }
            set
            {
                _IsFirstPoint = value;
            }
        }

        public ConnectivityAnalysisDlg(ILayer pLayer)
        {
            InitializeComponent();
            this.Owner = MainWindow.m_MainWindow;
            m_PointFeaSet = (pLayer as FeatureLayer).FeatureSet;
            m_DicLayer = new Dictionary<int, ILayer>();
            var layers = MainWindow.m_DotMap.GetLineLayers();
            foreach (var layer in layers)
            {
                m_DicLayer.Add(this.cboLayer.Items.Count, layer);
                this.cboLayer.Items.Add(layer.LegendText);
            }
            if (this.cboLayer.Items.Count > 0)
            {
                this.cboLayer.SelectedIndex = 0;
            }
        }

        private void cboLayer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.cboLayer.SelectedIndex == -1) return;
            var layer = m_DicLayer[this.cboLayer.SelectedIndex];
            m_CurrentFeaset = (layer as FeatureLayer).FeatureSet;
        }


        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            double bufferDistance;
            if (!double.TryParse(this.txtbuffer.Text, out bufferDistance))
            {
                MessageBox.Show("Input error!");
                return;
            }
            if (m_PointFeaSet.Features.Count != 2) return;
            //如果是经纬度坐标，转换缓冲距离为米
            if (m_CurrentFeaset.Features[1].Geometry.Coordinates[0].Y < 90)
            {
                bufferDistance = bufferDistance/111;
            }
            Dictionary<int,IFeature> StartLines = new Dictionary<int,IFeature>();
            Dictionary<int,IFeature> EndLines = new Dictionary<int,IFeature>();
            //生成起终点缓冲区
            IGeometry pStartCircle = m_PointFeaSet.Features[0].Geometry.Buffer(bufferDistance);
            IGeometry pEndCircle = m_PointFeaSet.Features[1].Geometry.Buffer(bufferDistance);

            List<int> FidList = new List<int>();
            //求交
            foreach (var line in m_CurrentFeaset.Features)
            {
                if (line.Geometry.Intersects(pStartCircle))
                {
                    StartLines.Add(line.Fid, line);
                    FidList.Add(line.Fid);
                }
                if (line.Geometry.Intersects(pEndCircle))
                    EndLines.Add(line.Fid,line);
            }
            //验证连通性

            //若起终点线非直接相交，验证间接相交性
            if (StartLines.Count == 0 || EndLines.Count == 0)
            {
                MessageBox.Show("Buffer distance may be too small!");
                return;
            }
            while (StartLines.Count > 0)
            {
                if (IsIntersect(StartLines,EndLines))
                {
                    MessageBox.Show("Connective!");
                    return;
                }
                Dictionary<int,IFeature> temp = new Dictionary<int,IFeature>();
                int i = 0;
                foreach (var start in StartLines)
                {
                    foreach (var fea in m_CurrentFeaset.Features)
                    {
                        if (fea.Geometry.Intersects(start.Value.Geometry))
                        {
                            if (!temp.ContainsKey(fea.Fid) && !FidList.Contains(fea.Fid))
                            {
                                temp.Add(fea.Fid, fea);
                                FidList.Add(fea.Fid);
                            }
                        }
                    }
                    if(i == StartLines.Count-1)
                    {
                        StartLines = temp;
                    }
                    i++;
                }
            }
            MessageBox.Show("Not connective!");
            this.Close();
        }

        private bool IsIntersect(Dictionary<int,IFeature> start, Dictionary<int,IFeature> end)
        {
            foreach (var geo_start in start)
            {
                foreach (var geo_end in end)
                {
                    if (geo_start.Value.Geometry.Intersects(geo_end.Value.Geometry))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
