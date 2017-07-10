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
using NetTopologySuite.Geometries;

namespace DotSpatialGISManager
{
    /// <summary>
    /// FayingSurfaceDlg.xaml 的交互逻辑
    /// </summary>
    public partial class MoveNodesDlg : Window, INotifyPropertyChanged
    {
        private IFeatureLayer[] m_FeaLyrList = new IFeatureLayer[] { };
        private FeatureLayer m_CurrentFeaLyr = null;
        private FeatureSet m_InputFeaSet = null;
        private List<IMapLineLayer> m_Layers = new List<IMapLineLayer>();
        private List<IPoint> Points = new List<IPoint>();
        private FeatureLayer AllPointLayer = null;
        private IFeature selectFea = null;
        private string TempPath = AppDomain.CurrentDomain.BaseDirectory + "Temp/" + Guid.NewGuid().ToString() + "temp.shp";

        private static MoveNodesDlg _defaultIntance = null;

        public static MoveNodesDlg GetInstance()
        {
            if (_defaultIntance == null)
            {
                _defaultIntance = new MoveNodesDlg();
            }
            return _defaultIntance;
        }

        private ObservableCollection<Nodes> _PointList;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Nodes> PointList
        {
            get
            {
                return _PointList;
            }
            set
            {
                if (_PointList != value)
                {
                    _PointList = value;
                    if (PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(PointList)));
                    }
                }
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


        public MoveNodesDlg()
        {
            InitializeComponent();
            this.btnStartEditNode.IsEnabled = false;
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

        private void M_CurrentFeaLyr_SelectionChanged(object sender, EventArgs e)
        {
            if (m_CurrentFeaLyr.Selection.Count == 0) return;
            if (m_CurrentFeaLyr.Selection.Count > 1)
            {
                MessageBox.Show("Please select one feature");
                m_CurrentFeaLyr.UnSelectAll();
                return;
            }
            this.btnStartEditNode.IsEnabled = true;
            var pFeature = m_CurrentFeaLyr.Selection.ToFeatureList().FirstOrDefault(); ;
            ////保证FID 不发生变化
            //selectFea = pFeature.Copy();
            selectFea = (from u in (m_CurrentFeaLyr as FeatureLayer).FeatureSet.Features
                         where u.Geometry.Covers(pFeature.Geometry)
                         select u).FirstOrDefault();
            int i = 1;
            List<Nodes> temp = new List<Nodes>();
            Points.Clear();
            foreach (var coor in selectFea.Geometry.Coordinates)
            {
                IPoint pPoint = new NetTopologySuite.Geometries.Point(coor);
                temp.Add(new Nodes { ID = i.ToString(), X = coor.X, Y = coor.Y });
                Points.Add(pPoint);
                i++;
            }
            PointList = new ObservableCollection<Nodes>(temp);
            this.IsVisibility = Visibility.Visible;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            UCControls.UCVectorDataEditing.m_MoveNodeDlg = null;
        }

        private void btnStartEditNode_Click(object sender, RoutedEventArgs e)
        {
            //显示选中要素的结点列表，并提示用户选择需编辑结点
            //if (btnStartEditNode.Content.ToString() == "Select editing node")
            //{
            MessageBox.Show("Please select the nodes that you want to edit,left button to select,right button to move");
            //注销事件
            m_CurrentFeaLyr.SelectionChanged -= M_CurrentFeaLyr_SelectionChanged;
            MainWindow.m_AddFeaType = Enum.FeaType.MovePoint;
            MainWindow.m_DotMap.FunctionMode = FunctionMode.Select;
            MainWindow.m_DotMap.Cursor = System.Windows.Forms.Cursors.Cross;
            if (m_CurrentFeaLyr.Selection.Count > 0)
                m_CurrentFeaLyr.ZoomToSelectedFeatures();
            //复制结点图层
            AllPointLayer = MainWindow.m_DotMap.Layers.Where(t => t.LegendText == "AllNodes").FirstOrDefault() as FeatureLayer;
            if (AllPointLayer == null)
            {
                FeatureSet PointSet = new FeatureSet(FeatureType.Point);
                for (int i = 0; i < Points.Count; i++)
                {
                    IFeature pFea = PointSet.AddFeature(Points[i]);
                    PointList[i].Feature = pFea;
                }
                PointSet.Name = "AllNodes";
                PointSet.Projection = MainWindow.m_DotMap.Projection;
                AllPointLayer = MainWindow.m_DotMap.Layers.Add(PointSet) as FeatureLayer;
                PointSymbolizer symbol = new PointSymbolizer(System.Drawing.Color.Red, DotSpatial.Symbology.PointShape.Ellipse, 5);
                AllPointLayer.Symbolizer = symbol;
                PointSet.SaveAs(TempPath, true);
            }
            btnStartEditNode.IsEnabled = false;
            //}
            //else//保存
            //{
            //    (m_CurrentFeaLyr as FeatureLayer).FeatureSet.Save();
            //    MessageBox.Show("Save successfully!");
            //    btnStartEditNode.Content = "Select editing node";
            //    MainWindow.m_AddFeaType = Enum.FeaType.None;
            //    MainWindow.m_DotMap.FunctionMode = FunctionMode.None;
            //    MainWindow.m_DotMap.Cursor = System.Windows.Forms.Cursors.Default;
            //    var layer = MainWindow.m_DotMap.Layers.Where(u => u.LegendText == "AllNodes").FirstOrDefault();
            //    if (layer != null)
            //        MainWindow.m_DotMap.Layers.Remove(layer);
            //    this.Close();
            //}
        }

        private void Datagrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.Datagrid.SelectedItem == null || AllPointLayer == null) return;
            Nodes node = this.Datagrid.SelectedItem as Nodes;
            AllPointLayer.UnSelectAll();
            AllPointLayer.Select(node.Feature);
            AllPointLayer.ZoomToSelectedFeatures();
        }

        public void MoveNode(Coordinate coord)
        {
            GeoAPI.Geometries.IPoint pPoint = new NetTopologySuite.Geometries.Point(coord);
            //确保目标图层只选中编辑的那一个要素，因为后面会把选中要素移除
            m_CurrentFeaLyr.UnSelectAll();
            m_CurrentFeaLyr.Selection.Clear();
            m_CurrentFeaLyr.Select(selectFea);
            if (AllPointLayer?.Selection.Count == 0 || Points?.Count == 0 || selectFea == null || m_CurrentFeaLyr == null) return;

            //移除点图层选中要素，并用pPoint替换选中点
            IFeature resFea = AllPointLayer.FeatureSet.AddFeature(pPoint);
            bool hasAdd = false;
            foreach (var fea in AllPointLayer.Selection.ToFeatureList())
            {
                for (int i = 0; i < Points.Count; i++)
                {
                    if (Points[i].Intersects(fea.Geometry))
                    {
                        Points[i] = pPoint;
                        hasAdd = true;
                        break;
                    }
                }
                if (hasAdd) break;
            }
            AllPointLayer.RemoveSelectedFeatures();
            //AllPointLayer.UnSelectAll();
            //AllPointLayer.Selection.Clear();
            AllPointLayer.FeatureSet.Save();

            List<Coordinate> temp = new List<Coordinate>();
            IFeature resultFeature = null;
            foreach (var point in Points)
            {
                temp.Add(new Coordinate(point.X, point.Y));
            }
            if (selectFea.FeatureType == FeatureType.Line)
            {
                LineString line = new LineString(temp.ToArray());
                try
                {
                    resultFeature = m_InputFeaSet.AddFeature(line);
                }
                catch
                {
                    resultFeature = m_InputFeaSet.AddFeature(line);
                }
                resultFeature.DataRow.BeginEdit();
                for (int i = 0; i < resultFeature.DataRow.ItemArray.Count(); i++)
                {
                    resultFeature.DataRow[i] = selectFea.DataRow[i];
                }
                resultFeature.DataRow.EndEdit();
            }
            else if (selectFea.FeatureType == FeatureType.Polygon)
            {
                ILinearRing LineRing = new LinearRing(temp.ToArray());
                NetTopologySuite.Geometries.Polygon pPolygon = new NetTopologySuite.Geometries.Polygon(LineRing);
                try
                {
                    resultFeature = m_InputFeaSet.AddFeature(pPolygon);
                }
                catch
                {
                    resultFeature = m_InputFeaSet.AddFeature(pPolygon);
                }
                resultFeature.DataRow.BeginEdit();
                for (int i = 0; i < resultFeature.DataRow.ItemArray.Count(); i++)
                {
                    resultFeature.DataRow[i] = selectFea.DataRow[i];
                }
                resultFeature.DataRow.EndEdit();
            }
            m_CurrentFeaLyr.RemoveSelectedFeatures();

            //this.selectFea = resultFeature;
            //this.ReBinding();

            //m_CurrentFeaLyr.Select(selectFea);
            MainWindow.m_DotMap.ResetBuffer();
            MainWindow.m_DotMap.Refresh();


            if (MessageBox.Show("Save edit?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                m_CurrentFeaLyr.FeatureSet.Save();
                MessageBox.Show("Save successfully!");
            }
            //移除图层重新加载，因为底层bug  移动节点之后选择要素会报错。
            MainWindow.m_AddFeaType = Enum.FeaType.None;
            MainWindow.m_DotMap.FunctionMode = FunctionMode.None;
            MainWindow.m_DotMap.Cursor = System.Windows.Forms.Cursors.Default;
            var layer = MainWindow.m_DotMap.Layers.Where(u => u.LegendText == "AllNodes").FirstOrDefault();
            if (layer != null)
                MainWindow.m_DotMap.Layers.Remove(layer);
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
            result.ZoomToSelectedFeatures();
            this.Close();
        }

        private void ReBinding()
        {
            List<Nodes> temp = new List<Nodes>();
            int i = 1;
            foreach (var fea in (AllPointLayer as FeatureLayer).FeatureSet.Features)
            {
                temp.Add(new Nodes { ID = i.ToString(), X = fea.Geometry.Coordinate.X, Y = fea.Geometry.Coordinate.Y, Feature = fea });
                i++;
            }
            PointList = new ObservableCollection<Nodes>(temp);
        }
    }
}
