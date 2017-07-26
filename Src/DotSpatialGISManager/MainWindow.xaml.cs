using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPF.Themes;
using DotSpatial.Controls;
using DotSpatial.Data;
using Common.Helper;
using System.Windows.Controls.Primitives;
using DotSpatialGISManager.UCControls;
using DotSpatialGISManager.Enum;
using GeoAPI.Geometries;
using DotSpatial.Symbology;
using NetTopologySuite.Geometries;
using System.Data;

namespace DotSpatialGISManager
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //全局静态控件
        public static Map m_DotMap = null;
        public static Legend m_DotLegend = null;
        public static SetScaleDlg m_SetScaleDlg = null;
        public static ZoomToCoorDlg m_ZoomToCoorDlg = null;
        public static MainWindow m_MainWindow = null;
        public static OpenAttributeTableDlg m_OpenAttributeTableDlg = null;
        public static FeaType m_AddFeaType = FeaType.None;

        ////添加一个结点编辑要用到的辅助变量
        //public static int isMoveNodes = 0;
        public string ZoomInImage
        {
            get
            {
                return PathHelper.ResourcePath + "01.zoom in.png";
            }
        }

        public string ZoomOutImage
        {
            get
            {
                return PathHelper.ResourcePath + "01.zoom out.png";
            }
        }

        public string FullExtentImage
        {
            get
            {
                return PathHelper.ResourcePath + "01.full extent.png";
            }
        }

        public string PanImage
        {
            get
            {
                return PathHelper.ResourcePath + "01.pan.png";
            }
        }

        public string IdentifyImage
        {
            get
            {
                return PathHelper.ResourcePath + "01.information.png";
            }
        }

        public string SetScaleImage
        {
            get
            {
                return PathHelper.ResourcePath + "01.set scale.png";
            }
        }

        public string SelectImage
        {
            get
            {
                return PathHelper.ResourcePath + "02.select.png";
            }
        }

        public string CenterLocationImage
        {
            get
            {
                return PathHelper.ResourcePath + "01.center location.png";
            }
        }

        public string AttributeImage
        {
            get
            {
                return PathHelper.ResourcePath + "01.open attribute table.png";
            }
        }

        /// <summary>
        /// 构造
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            m_MainWindow = this;
            //默认主题
            Application.Current.ApplyTheme("Office2010Blue");
            //默认窗口最大化
            //this.WindowState = WindowState.Maximized;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //初始化控件
            m_DotMap = new Map();
            m_DotLegend = new Legend();
            m_DotMap.Legend = m_DotLegend;
            this.LegendHost.Child = m_DotLegend;
            this.MapHost.Child = m_DotMap;
            //默认漫游
            m_DotMap.FunctionMode = FunctionMode.Pan;
            m_DotMap.ZoomOutFartherThanMaxExtent = true;
            m_DotMap.IsZoomedToMaxExtent = false;
            InitEvent();
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        private void InitEvent()
        {
            m_DotMap.GeoMouseMove += M_DotMap_GeoMouseMove;
            m_DotMap.ViewExtentsChanged += M_DotMap_ViewExtentsChanged;
            m_DotMap.MouseUp += M_DotMap_MouseUp;
            m_DotMap.FunctionModeChanged += M_DotMap_FunctionModeChanged;
            m_DotMap.MouseDown += M_DotMap_MouseDown;
        }

        private void M_DotMap_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (m_AddFeaType != FeaType.None)
            {

                Coordinate coord = m_DotMap.PixelToProj(e.Location);//点击的屏幕未知转换成坐标系中的点
                switch (m_AddFeaType)
                {
                    case FeaType.Point:
                        {
                            CreatePointDlg f = UCVectorDataEditing.m_CreatePointDlg;
                            if (f == null) return;
                            var layer = f.m_PointLayer;
                            IFeatureSet PointF = (layer as FeatureLayer).FeatureSet;
                            GeoAPI.Geometries.IPoint pPoint = new NetTopologySuite.Geometries.Point(coord);
                            IFeature currentFeature = PointF.AddFeature(pPoint);
                            PointF.InitializeVertices();
                            m_DotMap.ResetBuffer();
                        }
                        break;
                    case FeaType.Polyline:
                        {
                            CreatePolylineDlg f = UCVectorDataEditing.m_CreatePolylineDlg;
                            if (f == null) return;
                            var layer = f.m_PolylineLayer;
                            IFeatureSet LineF = (layer as FeatureLayer).FeatureSet;
                            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                            {
                                if (f.IsFirstPoint)
                                {
                                    //一开始就要加入至少两个点
                                    f.CoordList.Add(coord);
                                    f.CoordList.Add(coord);
                                    LineString line = new LineString(f.CoordList.ToArray());
                                    IFeature lineFeature = LineF.AddFeature(line);
                                    f.IsFirstPoint = false;
                                }
                                else
                                {
                                    LineF.Features.RemoveAt(LineF.Features.Count - 1);
                                    if (f.CoordList[0] == f.CoordList[1])
                                        f.CoordList.RemoveAt(1);
                                    f.CoordList.Add(coord);
                                    LineString line = new LineString(f.CoordList.ToArray());
                                    IFeature lineFeature = LineF.AddFeature(line);
                                    m_DotMap.ResetBuffer();
                                }
                            }
                            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
                            {
                                LineF.InitializeVertices();
                                f.IsFirstPoint = true;
                                f.CoordList.Clear();
                                m_DotMap.ResetBuffer();
                            }
                        }
                        break;
                    case FeaType.Polygon:
                        {
                            CreatePolygonDlg f = UCVectorDataEditing.m_CreatePolygonDlg;
                            if (f == null) return;
                            var layer = f.m_PolygonLayer;
                            IFeatureSet PolygonF = (layer as FeatureLayer).FeatureSet;
                            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                            {
                                if (f.IsFirstPoint)
                                {
                                    for (int i = 0; i < 4; i++)
                                        f.CoordList.Add(coord);
                                    ILinearRing LineRing = new LinearRing(f.CoordList.ToArray());
                                    NetTopologySuite.Geometries.Polygon pPolygon = new NetTopologySuite.Geometries.Polygon(LineRing);
                                    IFeature polygonFeature = PolygonF.AddFeature(pPolygon);
                                    f.IsFirstPoint = false;
                                }
                                else
                                {
                                    PolygonF.Features.RemoveAt(PolygonF.Features.Count - 1);
                                    if (f.CoordList[0] == f.CoordList[1])
                                        f.CoordList.RemoveAt(1);
                                    //组成面的点必须形成一个闭环 因此要先把最新加入的点去掉，加入绘制点之后再加入第一个点
                                    f.CoordList.RemoveAt(f.CoordList.Count - 1);
                                    f.CoordList.Add(coord);
                                    f.CoordList.Add(f.CoordList[0]);
                                    ILinearRing LineRing = new LinearRing(f.CoordList.ToArray());
                                    NetTopologySuite.Geometries.Polygon pPolygon = new NetTopologySuite.Geometries.Polygon(LineRing);
                                    IFeature lineFeature = PolygonF.AddFeature(pPolygon);
                                    m_DotMap.ResetBuffer();
                                }
                            }
                            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
                            {
                                PolygonF.InitializeVertices();
                                f.IsFirstPoint = true;
                                f.CoordList.Clear();
                                m_DotMap.ResetBuffer();
                            }
                        }
                        break;
                    case FeaType.UCPoint:
                        {
                            bool ShouldAdd = true;
                            UCTopologyAnalysis uc = UCTopologyAnalysis.GetIntance();
                            if (uc.ClickIndex >= 1)
                            {
                                m_AddFeaType = FeaType.None;
                                uc.ClickIndex = 0;
                                ShouldAdd = false;
                                m_DotMap.Cursor = System.Windows.Forms.Cursors.Default;
                            }
                            var layer = uc.m_PointLayer;
                            IFeatureSet PointF = (layer as FeatureLayer).FeatureSet;
                            GeoAPI.Geometries.IPoint pPoint = new NetTopologySuite.Geometries.Point(coord);
                            IFeature currentFeature = PointF.AddFeature(pPoint);
                            m_DotMap.ResetBuffer();
                            if (ShouldAdd)
                                uc.ClickIndex++;
                        }
                        break;
                    case FeaType.MovePoint:
                        {
                            if (e.Button == System.Windows.Forms.MouseButtons.Right)
                            {
                                var defaultIntance = MoveNodesDlg.GetInstance();
                                if (defaultIntance != null)
                                {
                                    defaultIntance.MoveNode(coord);
                                }
                            }
                        }
                        break;
                    case FeaType.MoveFeature:
                        {
                            if (e.Button == System.Windows.Forms.MouseButtons.Right)
                            {
                                var defaultIntance = MoveFeatureDlg.GetInstance();
                                if (defaultIntance != null)
                                {
                                    defaultIntance.MoveFeature(coord);
                                }
                            }
                        }
                        break;
                    case FeaType.RotateFeature:
                        {
                            if (e.Button == System.Windows.Forms.MouseButtons.Right)
                            {
                                var defaultIntance = RotateFeatureDlg.GetInstance();
                                if (defaultIntance != null)
                                {
                                    defaultIntance.RotateFeature(coord);
                                }
                            }
                        break;
                        }
                }
            }
        }

        private void M_DotMap_FunctionModeChanged(object sender, EventArgs e)
        {
            this.btnZoomIn.IsChecked = false;
            this.btnZoomOut.IsChecked = false;
            this.btnPan.IsChecked = false;
            this.btnIDentify.IsChecked = false;
            this.btnSelect.IsChecked = false;
            UCVectorDataEditing.m_btnSelect.IsChecked = false;
            switch (m_DotMap.FunctionMode)
            {
                case FunctionMode.Info:
                    btnIDentify.IsChecked = true;
                    break;
                case FunctionMode.Pan:
                    btnPan.IsChecked = true;
                    break;
                case FunctionMode.Select:
                    btnSelect.IsChecked = true;
                    UCVectorDataEditing.m_btnSelect.IsChecked = true;
                    break;
                case FunctionMode.ZoomIn:
                    btnZoomIn.IsChecked = true;
                    break;
                case FunctionMode.ZoomOut:
                    btnZoomOut.IsChecked = true;
                    break;
            }
        }

        private void M_DotMap_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (m_DotMap.FunctionMode == FunctionMode.Select && UCMapSurvying.m_MeasureResultDlg != null)
            {
                UCMapSurvying.m_MeasureResultDlg.Init();
            }
        }

        private void M_DotMap_ViewExtentsChanged(object sender, ExtentArgs e)
        {
            if (m_SetScaleDlg != null)
            {
                m_SetScaleDlg.Init();
            }
        }

        private void M_DotMap_GeoMouseMove(object sender, GeoMouseArgs e)
        {
            this.tbkCoor.Text = string.Format("X：{0}，Y：{1}", e.GeographicLocation.X.ToString("F6"), e.GeographicLocation.Y.ToString("F6"));
        }

        //private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (e.AddedItems.Count > 0)
        //    {
        //        string theme = e.AddedItems[0].ToString();
        //        // Application Level
        //        Application.Current.ApplyTheme(theme);
        //    }
        //}

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as Button).Name)
            {
                case "btnFull":
                    m_DotMap.ZoomToMaxExtent();
                    break;
                case "btnSetScale":
                    if (m_SetScaleDlg == null)
                    {
                        m_SetScaleDlg = new SetScaleDlg();
                        m_SetScaleDlg.Owner = this;
                        m_SetScaleDlg.Init();
                        m_SetScaleDlg.Show();
                    }
                    break;
                case "btnCenter":
                    if (m_ZoomToCoorDlg == null)
                    {
                        m_ZoomToCoorDlg = new ZoomToCoorDlg();
                        m_ZoomToCoorDlg.Show();
                    }
                    break;
                case "btnAttribute":
                    if (m_OpenAttributeTableDlg == null)
                    {
                        m_OpenAttributeTableDlg = new OpenAttributeTableDlg();
                        m_OpenAttributeTableDlg.Show();
                    }
                    break;
            }
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as ToggleButton).Name)
            {
                case "btnZoomIn":
                    m_DotMap.FunctionMode = FunctionMode.ZoomIn;
                    break;
                case "btnZoomOut":
                    //缩小似乎有bug
                    m_DotMap.FunctionMode = FunctionMode.ZoomOut;
                    m_DotMap.ZoomOutFartherThanMaxExtent = true;
                    m_DotMap.IsZoomedToMaxExtent = false;
                    break;
                case "btnPan":
                    m_DotMap.FunctionMode = FunctionMode.Pan;
                    break;
                case "btnIDentify":
                    m_DotMap.FunctionMode = FunctionMode.Info;
                    break;
                case "btnSelect":
                    m_DotMap.FunctionMode = FunctionMode.Select;
                    break;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            App.Current.Shutdown();
        }
    }
}
