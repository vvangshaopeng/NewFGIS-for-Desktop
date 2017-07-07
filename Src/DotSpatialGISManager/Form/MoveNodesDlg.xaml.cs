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

namespace DotSpatialGISManager
{
    /// <summary>
    /// FayingSurfaceDlg.xaml 的交互逻辑
    /// </summary>
    public partial class MoveNodesDlg : Window, INotifyPropertyChanged
    {
        private IFeatureLayer[] m_FeaLyrList = new IFeatureLayer[] { };
        public IFeatureLayer m_CurrentFeaLyr = null;
        private IFeatureSet m_InputFeaSet = null;
        private List<IMapLineLayer> m_Layers = new List<IMapLineLayer>();
        public List<IPoint> Points = new List<IPoint>();
        public IFeatureLayer AllPointLayer = null;
        public IFeature selectFea = null;

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
            m_CurrentFeaLyr = m_FeaLyrList[this.cboLayer.SelectedIndex];
            m_InputFeaSet = (m_CurrentFeaLyr as FeatureLayer).FeatureSet;
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
            IFeature pFeature = m_CurrentFeaLyr.Selection.ToFeatureList()[0];
            selectFea = (from u in (m_CurrentFeaLyr as FeatureLayer).FeatureSet.Features
                         where u.Geometry.Intersects(pFeature.Geometry)
                         select u).FirstOrDefault();
            int i = 1;
            List<Nodes> temp = new List<Nodes>();
            Points.Clear();
            foreach (var coor in pFeature.Geometry.Coordinates)
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
            if (btnStartEditNode.Content.ToString() == "Select editing node")
            {
                MessageBox.Show("Please select the nodes thet you want to edit!");

                MainWindow.m_AddFeaType = Enum.FeaType.None;
                MainWindow.m_DotMap.FunctionMode = FunctionMode.Select;
                MainWindow.m_DotMap.Cursor = System.Windows.Forms.Cursors.Default;

                m_CurrentFeaLyr.ZoomToSelectedFeatures();
                //复制结点图层
                FeatureSet PointSet = new FeatureSet(FeatureType.Point);
                for (int i = 0; i < Points.Count; i++)
                {
                    IFeature pFea = PointSet.AddFeature(Points[i]);
                    PointList[i].Feature = pFea;
                }
                PointSet.Name = "AllNodes";
                PointSet.Projection = MainWindow.m_DotMap.Projection;
                AllPointLayer = MainWindow.m_DotMap.Layers.Add(PointSet);
                PointSymbolizer symbol = new PointSymbolizer(System.Drawing.Color.Red, DotSpatial.Symbology.PointShape.Ellipse, 5);
                AllPointLayer.Symbolizer = symbol;

                //切换按钮显示文本
                btnStartEditNode.Content = "Start editing node";
            }

            //编辑结点
            else if (btnStartEditNode.Content.ToString() == "Start editing node")
            {
                MainWindow.m_AddFeaType = Enum.FeaType.MovePoint;
                MainWindow.m_DotMap.FunctionMode = FunctionMode.None;
                MainWindow.m_DotMap.Cursor = System.Windows.Forms.Cursors.Cross;
                btnStartEditNode.Content = "Select editing node";
            }
        }

        private void Datagrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.Datagrid.SelectedItem == null || AllPointLayer == null) return;
            Nodes node = this.Datagrid.SelectedItem as Nodes;
            AllPointLayer.UnSelectAll();
            AllPointLayer.Select(node.Feature);
            AllPointLayer.ZoomToSelectedFeatures();
        }
    }
}
