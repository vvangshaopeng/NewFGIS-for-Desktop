using DotSpatial.Controls;
using DotSpatial.Data;
using DotSpatial.Symbology;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DotSpatialGISManager
{
    /// <summary>
    /// MoveFeatureDlg.xaml 的交互逻辑
    /// </summary>
    public partial class MoveFeatureDlg : Window, INotifyPropertyChanged
    {
        private IFeatureLayer[] m_FeaLyrList = new IFeatureLayer[] { };
        private FeatureLayer m_CurrentFeaLyr = null;
        private FeatureSet m_InputFeaSet = null;
        private List<IMapLineLayer> m_Layers = new List<IMapLineLayer>();
        private IFeature selectFea = null;
        private IFeature lFeaM = null;

        private static MoveFeatureDlg _defaultIntance = null;

        public static MoveFeatureDlg GetInstance()
        {
            if (_defaultIntance == null)
            {
                _defaultIntance = new MoveFeatureDlg();
            }
            return _defaultIntance;
        }

        public event PropertyChangedEventHandler PropertyChanged;

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

        private Visibility _IsVisibility = Visibility.Collapsed;
        public Visibility IsVisibility
        {
            get
            {
                return _IsVisibility;
            }
            set
            {
                if (_IsVisibility != value)
                {
                    _IsVisibility = value;
                    if (PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(IsVisibility)));
                    }
                }
            }
        }


        public MoveFeatureDlg()
        {
            InitializeComponent();
            this.Owner = MainWindow.m_MainWindow;
            this.DataContext = this;
            _defaultIntance = this;
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
            m_CurrentFeaLyr = m_FeaLyrList[this.cboLayer.SelectedIndex] as FeatureLayer;
            m_InputFeaSet = (m_CurrentFeaLyr as FeatureLayer).FeatureSet as FeatureSet;
        }

        private void M_CurrentFeaLyr_SelectionChanged(object sender, EventArgs e)
        {
            if (m_CurrentFeaLyr.Selection.Count == 0) return;
            if (m_CurrentFeaLyr.Selection.Count > 1)
            {
                MessageBox.Show("Please select one feature");
                m_CurrentFeaLyr.UnSelectAll();
                return;
            }
            this.btnStartMoveFeature.IsEnabled = true;
            var pFeature = m_CurrentFeaLyr.Selection.ToFeatureList().FirstOrDefault(); ;
            ////保证FID 不发生变化
            //selectFea = pFeature.Copy();
            selectFea = (from u in (m_CurrentFeaLyr as FeatureLayer).FeatureSet.Features
                         where u.Geometry.Covers(pFeature.Geometry)
                         select u).FirstOrDefault();

            this.IsVisibility = Visibility.Visible;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            UCControls.UCVectorDataEditing.m_MoveFeatureDlg = null;
        }
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            //若未选择图层
            if (this.cboLayer.Text == "")
            {
                MessageBox.Show("Please select a layer！");
                return;
            }
            this.btnOK.IsEnabled = false;
            ////右键结束选择 写在mainwindow里
            //注册事件
            m_CurrentFeaLyr.SelectionChanged += M_CurrentFeaLyr_SelectionChanged;

            MainWindow.m_DotMap.FunctionMode = FunctionMode.Select;
        }

        private void btnStartMoveFeature_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Right button to move");
            //注销事件
            m_CurrentFeaLyr.SelectionChanged -= M_CurrentFeaLyr_SelectionChanged;
            MainWindow.m_AddFeaType = Enum.FeaType.MoveFeature;

            btnStartMoveFeature.IsEnabled = false;
        }

        public void MoveFeature(Coordinate coord)
        {
            if (selectFea == null || m_CurrentFeaLyr == null) return;

            //确保目标图层只选中编辑的那一个要素，因为后面会把选中要素移除
            m_CurrentFeaLyr.UnSelectAll();
            m_CurrentFeaLyr.Selection.Clear();
            m_CurrentFeaLyr.Select(selectFea);

            //move

            Coordinate moveCoord = new Coordinate();
            moveCoord.X = coord.X - selectFea.Geometry.Coordinates[0].X;
            moveCoord.Y = coord.Y - selectFea.Geometry.Coordinates[0].Y;
            if (selectFea.FeatureType == FeatureType.Point)
            {
                Coordinate resultCoord = new Coordinate();
                //move
                resultCoord.X = selectFea.Geometry.Coordinate.X + moveCoord.X;
                resultCoord.Y = selectFea.Geometry.Coordinate.Y + moveCoord.Y;
                IPoint pPoint = new NetTopologySuite.Geometries.Point(resultCoord);
                lFeaM = m_InputFeaSet.AddFeature(pPoint);
                for (int i = 0; i < selectFea.DataRow.ItemArray.Count(); i++)
                {
                    lFeaM.DataRow[i] = selectFea.DataRow[i];
                }
            }
            else if (selectFea.FeatureType == FeatureType.Line)
            {
                //move
                for (int i = 0; i < selectFea.Geometry.NumPoints; i++)
                {
                    Coordinate resultCoord = new Coordinate();
                    resultCoord.X = selectFea.Geometry.Coordinates[i].X + moveCoord.X;
                    resultCoord.Y = selectFea.Geometry.Coordinates[i].Y + moveCoord.Y;
                    CoordList.Add(resultCoord);
                }
                ILineString pLine = new LineString(CoordList.ToArray());

                lFeaM = m_InputFeaSet.AddFeature(pLine);
                for (int i = 0; i < selectFea.DataRow.ItemArray.Count(); i++)
                {
                    lFeaM.DataRow[i] = selectFea.DataRow[i];
                }
                CoordList.Clear();
            }
            else if (selectFea.FeatureType == FeatureType.Polygon)
            {
                //move
                for (int i = 0; i < selectFea.Geometry.NumPoints; i++)
                {
                    Coordinate resultCoord = new Coordinate();
                    resultCoord.X = selectFea.Geometry.Coordinates[i].X + moveCoord.X;
                    resultCoord.Y = selectFea.Geometry.Coordinates[i].Y + moveCoord.Y;
                    CoordList.Add(resultCoord);
                }
                ILinearRing LineRing = new LinearRing(CoordList.ToArray());
                IPolygon pPolygon = new NetTopologySuite.Geometries.Polygon(LineRing);
                lFeaM = m_InputFeaSet.AddFeature(pPolygon);
                for (int i = 0; i < selectFea.DataRow.ItemArray.Count(); i++)
                {
                    lFeaM.DataRow[i] = selectFea.DataRow[i];
                }
                CoordList.Clear();
            }
            //m_CurrentFeaLyr.FeatureSet.AddFeature(lFeaM.Geometry);
            m_CurrentFeaLyr.RemoveSelectedFeatures();

            MainWindow.m_DotMap.ResetBuffer();
            MainWindow.m_DotMap.Refresh();


            if (MessageBox.Show("Save edit?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                m_CurrentFeaLyr.FeatureSet.Save();
                MessageBox.Show("Save successfully!");
            }
            //移除图层重新加载，因为底层bug 移动节点之后选择要素会报错。
            MainWindow.m_AddFeaType = Enum.FeaType.None;
            MainWindow.m_DotMap.FunctionMode = FunctionMode.None;
            MainWindow.m_DotMap.Cursor = System.Windows.Forms.Cursors.Default;
            string shpPath = m_CurrentFeaLyr.FeatureSet.FilePath;
            string name = m_CurrentFeaLyr.LegendText;
            var symbol = m_CurrentFeaLyr.Symbolizer;
            var extent = m_CurrentFeaLyr.Extent;
            IFeatureSet s = Shapefile.Open(shpPath);
            MainWindow.m_DotMap.Layers.Remove(m_CurrentFeaLyr as IMapLayer);
            var result = MainWindow.m_DotMap.Layers.Add(s);
            result.Symbolizer = symbol;
            result.Projection = MainWindow.m_DotMap.Projection;
            result.LegendText = name;
            result.Select((result as FeatureLayer).FeatureSet.Features[(result as FeatureLayer).FeatureSet.Features.Count - 1]);
            this.Close();
        }
    }
}