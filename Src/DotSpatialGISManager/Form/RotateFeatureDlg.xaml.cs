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
    /// RotateFeatureDlg.xaml 的交互逻辑
    /// </summary>
    public partial class RotateFeatureDlg : Window, INotifyPropertyChanged
    {
        private IFeatureLayer[] m_FeaLyrList = new IFeatureLayer[] { };
        private FeatureLayer m_CurrentFeaLyr = null;
        private FeatureSet m_InputFeaSet = null;
        private List<IMapLineLayer> m_Layers = new List<IMapLineLayer>();
        private IFeature selectFea = null;
        private IFeature lFeaM = null;

        private static RotateFeatureDlg _defaultIntance = null;

        public static RotateFeatureDlg GetInstance()
        {
            if (_defaultIntance == null)
            {
                _defaultIntance = new RotateFeatureDlg();
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


        public RotateFeatureDlg()
        {
            InitializeComponent();
            this.btnStartRotateFeature.IsEnabled = false;
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
            this.btnStartRotateFeature.IsEnabled = true;
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
            UCControls.UCVectorDataEditing.m_RotateFeatureDlg = null;
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

        private void btnStartRotateFeature_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Right button to rotate");
            //注销事件
            m_CurrentFeaLyr.SelectionChanged -= M_CurrentFeaLyr_SelectionChanged;
            MainWindow.m_AddFeaType = Enum.FeaType.RotateFeature;

            btnStartRotateFeature.IsEnabled = false;
        }

        public void RotateFeature(Coordinate coord)
        {
            if (selectFea == null || m_CurrentFeaLyr == null) return;

            //确保目标图层只选中编辑的那一个要素，因为后面会把选中要素移除
            m_CurrentFeaLyr.UnSelectAll();
            m_CurrentFeaLyr.Selection.Clear();
            m_CurrentFeaLyr.Select(selectFea);

            //rotate
            Coordinate center = new Coordinate();
            center.X = 0;
            center.Y = 0;
            for (int i = 0; i < selectFea.Geometry.NumPoints; i++)
            {
                center.X += selectFea.Geometry.Coordinates[i].X;
                center.Y += selectFea.Geometry.Coordinates[i].Y;
            }
            center.X = center.X / selectFea.Geometry.NumPoints;
            center.Y = center.Y / selectFea.Geometry.NumPoints;
            double dX = coord.X - center.X;
            double dY = coord.Y - center.Y;
            double ranRadian = Math.Atan(dY / dX);
            double ranAngle = 0;
            ranAngle = ranRadian * 180 / Math.PI;
            if (dX >= 0 && dY >= 0)
            {
                ranAngle = 270 + ranAngle;
            }
            else if (dX < 0)
            {
                ranAngle = 90 + ranAngle;
            }
            else if (dX >= 0 && dY < 0)
            {
                ranAngle = 360 + ranAngle;
            }
            ranRadian = ranAngle * Math.PI / 180;
            selectFea.Rotate(center, ranRadian);
            IFeature rotateFea = m_InputFeaSet.AddFeature(selectFea.Geometry);
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
            //result.Select((result as FeatureLayer).FeatureSet.Features[(result as FeatureLayer).FeatureSet.Features.Count - 1]);
            this.Close();
        }
    }
}