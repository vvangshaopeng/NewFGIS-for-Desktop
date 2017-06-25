using DotSpatial.Controls;
using DotSpatial.Data;
using DotSpatial.Symbology;
using DotSpatialGISManager.Enum;
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
using System.ComponentModel;
using System.Collections.ObjectModel;
using GeoAPI.Geometries;
using System.Collections;
using Common;

namespace DotSpatialGISManager
{
    /// <summary>
    /// SQLQueryDlg.xaml 的交互逻辑
    /// </summary>
    public partial class LocationQueryDlg : Window, INotifyPropertyChanged
    {
        private SelectionMethod m_SelectionMethod;
        private SpatialMethod m_SpatialMethod;
        private bool _UsingSelectedFeas;
        private string _SelectedFeasInfo;
        private string m_ResourcePath = AppDomain.CurrentDomain.BaseDirectory + "../Images/";
        private bool hasQueried = false;

        private ObservableCollection<LocationLayerModel> _TargetLayers;
        public ObservableCollection<LocationLayerModel> TargetLayers
        {
            get
            {
                return _TargetLayers ?? (_TargetLayers = new ObservableCollection<LocationLayerModel>());
            }
            set
            {
                if (_TargetLayers != value)
                {
                    _TargetLayers = value;
                    hasQueried = false;
                }
            }
        }

        private ObservableCollection<LocationLayerModel> _SourceLayers;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<LocationLayerModel> SourceLayers
        {
            get
            {
                return _SourceLayers ?? (_SourceLayers = new ObservableCollection<LocationLayerModel>());
            }
            set
            {
                if (_SourceLayers != value)
                {
                    _SourceLayers = value;
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("SourceLayer"));
                    }
                    hasQueried = false;
                }
            }
        }

        private LocationLayerModel _SelectedSourceLayer;
        public LocationLayerModel SelectedSourceLayer
        {
            get
            {
                return _SelectedSourceLayer;
            }
            set
            {
                if (_SelectedSourceLayer != value)
                {
                    _SelectedSourceLayer = value;
                    if (_SelectedSourceLayer != null && _SelectedSourceLayer.Layer != null)
                        this.SelectedFeasInfo = _SelectedSourceLayer.Layer.Selection.Count + " feature(s) selected";
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("SelectedSourceLayer"));
                    }
                    hasQueried = false;
                }
            }
        }

        public bool UsingSelectedFeas
        {
            get
            {
                return _UsingSelectedFeas;
            }
            set
            {
                if (_UsingSelectedFeas != value)
                {
                    _UsingSelectedFeas = value;
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("UsingSelectedFeas"));
                    }
                    hasQueried = false;
                }
            }
        }

        public string SelectedFeasInfo
        {
            get
            {
                return _SelectedFeasInfo;
            }
            set
            {
                if (_SelectedFeasInfo != value)
                {
                    _SelectedFeasInfo = value;
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("SelectedFeasInfo"));
                    }
                }
            }
        }

        public LocationQueryDlg()
        {
            InitializeComponent();
            this.Owner = MainWindow.m_MainWindow;
            this.DataContext = this;
            this.cboMethod.Items.Add("select features from");
            this.cboMethod.Items.Add("add to the currently features in");
            this.cboMethod.Items.Add("remove from the currently selected features in");
            this.cboMethod.Items.Add("select from the currently selected features in");
            this.cboMethod.SelectedIndex = 0;
            this.cboSelectionmethod.Items.Add("intersect the source layer feature");
            this.cboSelectionmethod.Items.Add("contain the source layer feature");
            this.cboSelectionmethod.Items.Add("are within the source layer featur");
            this.cboSelectionmethod.SelectedIndex = 0;
            InitBinding();
        }

        public void ResetSourceLayers()
        {
            SourceLayers.Clear();
            SelectedSourceLayer = null;
            List<LocationLayerModel> list = (from u in TargetLayers
                                             where u.IsChecked == false
                                             select u).ToList();
            list.ForEach(x => SourceLayers.Add(x));
            if (SourceLayers.Count > 0)
                SelectedSourceLayer = SourceLayers[0];
        }

        private void InitBinding()
        {
            var m_FealyrList = MainWindow.m_DotMap.GetFeatureLayers();
            ObservableCollection<LocationLayerModel> temp = new ObservableCollection<LocationLayerModel>();
            foreach (FeatureLayer layer in m_FealyrList)
            {
                LocationLayerModel model = new LocationLayerModel();
                model.ImagePath = GetImagePath(layer.FeatureSet.FeatureType);
                model.IsChecked = false;
                model.Layer = layer;
                model.LayerName = layer.LegendText;
                temp.Add(model);
            }
            TargetLayers = temp;
            List<LocationLayerModel> list = (from u in TargetLayers
                                             where u.IsChecked == false
                                             select u).ToList();
            list.ForEach(x => SourceLayers.Add(x));
            if (SourceLayers.Count > 0)
                SelectedSourceLayer = SourceLayers[0];
        }

        private string GetImagePath(FeatureType s)
        {
            switch (s)
            {
                case FeatureType.Point:
                case FeatureType.MultiPoint:
                    return m_ResourcePath + "point.png";
                case FeatureType.Line:
                    return m_ResourcePath + "polyline.png";
                case FeatureType.Polygon:
                    return m_ResourcePath + "polygon.png";
            }
            return null;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            UCControls.UCQuery.m_LocationQueryDlg = null;
        }

        private void cboMethod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.cboMethod.SelectedIndex == -1) return;
            hasQueried = false;
            switch (this.cboMethod.SelectedIndex)
            {
                case 0:
                    m_SelectionMethod = SelectionMethod.Select;
                    break;
                case 1:
                    m_SelectionMethod = SelectionMethod.Add;
                    break;
                case 2:
                    m_SelectionMethod = SelectionMethod.RemoveFromSelected;
                    break;
                case 3:
                    m_SelectionMethod = SelectionMethod.SelectFromSelected;
                    break;
            }
        }

        private void cboSelectionmethod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.cboSelectionmethod.SelectedIndex == -1) return;
            hasQueried = false;
            switch (this.cboSelectionmethod.SelectedIndex)
            {
                case 0:
                    m_SpatialMethod = SpatialMethod.Intersect;
                    break;
                case 1:
                    m_SpatialMethod = SpatialMethod.Contains;
                    break;
                case 2:
                    m_SpatialMethod = SpatialMethod.Within;
                    break;
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            btnApply_Click(null, null);
            this.Close();
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            if (hasQueried) return;
            var TargetLayerList = (from u in TargetLayers
                                   where u.IsChecked
                                   select u).ToList();
            if (TargetLayerList.Count == 0)
            {
                MessageBox.Show("Please select at least one target layer");
                return;
            }
            if (SelectedSourceLayer == null)
            {
                MessageBox.Show("Please select a source layer");
                return;
            }
            ProgressBox p = new ProgressBox(0, 100, "Location query progress");
            p.ShowPregress();
            p.SetProgressValue(0);
            IFeatureSet srcFeaset;
            if (this.UsingSelectedFeas)
                srcFeaset = SelectedSourceLayer.Layer.Selection.ToFeatureSet();
            else
                srcFeaset = SelectedSourceLayer.Layer.FeatureSet;
            Dictionary<IFeatureLayer, List<int>> DicFidList = new Dictionary<IFeatureLayer, List<int>>();
            switch (m_SpatialMethod)
            {
                case SpatialMethod.Intersect:
                    foreach (var layer in TargetLayerList)
                    {
                        List<int> FidList = new List<int>();
                        var tarFeaList = GetTargetFeaList(layer.Layer);
                        int tarCount = tarFeaList.Count;
                        double pi = Math.Round((double)(100 * 1.0 / tarCount), 2);
                        p.SetProgressDescription("Querying " + layer.Layer.LegendText);
                        for (int i = 0; i < tarCount; i++)
                        {
                            p.SetProgressValue(pi * (i + 1));
                            p.SetProgressDescription2(string.Format("{0} feature(s) is(are) queried, the remaining {1} feature(s) is(are) being queried", i + 1, tarCount - i - 1));
                            foreach (var fea in srcFeaset.Features)
                            {
                                if (tarFeaList[i].Geometry.Intersects(fea.Geometry))
                                {
                                    FidList.Add(tarFeaList[i].Fid);
                                    break;
                                }
                            }
                        }
                        DicFidList.Add(layer.Layer, FidList);
                    }
                    break;
                case SpatialMethod.Contains:
                    foreach (var layer in TargetLayerList)
                    {
                        List<int> FidList = new List<int>();
                        var tarFeaList = GetTargetFeaList(layer.Layer);
                        int tarCount = tarFeaList.Count;
                        double pi = Math.Round((double)(100 * 1.0 / tarCount), 2);
                        p.SetProgressDescription("Querying " + layer.Layer.LegendText);
                        for (int i = 0; i < tarCount; i++)
                        {
                            p.SetProgressValue(pi * (i + 1));
                            p.SetProgressDescription2(string.Format("{0} feature(s) is(are) queried, the remaining {1} feature(s) is(are) being queried", i + 1, tarCount - i - 1));
                            foreach (var fea in srcFeaset.Features)
                            {
                                if (tarFeaList[i].Geometry.Contains(fea.Geometry))
                                {
                                    FidList.Add(tarFeaList[i].Fid);
                                    break;
                                }
                            }
                        }
                        DicFidList.Add(layer.Layer, FidList);
                    }
                    break;
                case SpatialMethod.Within:
                    foreach (var layer in TargetLayerList)
                    {
                        List<int> FidList = new List<int>();
                        var tarFeaList = GetTargetFeaList(layer.Layer);
                        int tarCount = tarFeaList.Count;
                        double pi = Math.Round((double)(100 * 1.0 / tarCount), 2);
                        p.SetProgressDescription("Querying " + layer.Layer.LegendText);
                        for (int i = 0; i < tarCount; i++)
                        {
                            p.SetProgressValue(pi * (i + 1));
                            p.SetProgressDescription2(string.Format("{0} feature(s) is(are) queried, the remaining {1} feature(s) is(are) being queried", i + 1, tarCount - i - 1));
                            foreach (var fea in srcFeaset.Features)
                            {
                                if (tarFeaList[i].Geometry.Within(fea.Geometry))
                                {
                                    FidList.Add(tarFeaList[i].Fid);
                                    break;
                                }
                            }
                        }
                        DicFidList.Add(layer.Layer, FidList);
                    }
                    break;
            }
            //select features
            foreach (var value in DicFidList)
            {
                if (value.Value.Count > 0)
                {
                    switch (m_SelectionMethod)
                    {
                        case SelectionMethod.Add:
                            value.Key.Select(value.Value);
                            break;
                        case SelectionMethod.RemoveFromSelected:
                            value.Key.UnSelect(value.Value);
                            break;
                        case SelectionMethod.SelectFromSelected:
                        case SelectionMethod.Select:
                            value.Key.UnSelectAll();
                            value.Key.Select(value.Value);
                            break;
                    }
                    MainWindow.m_DotMap.Refresh();
                }
            }
            p.CloseProgress();
            hasQueried = true;
        }

        private List<IFeature> GetTargetFeaList(FeatureLayer layer)
        {
            switch (m_SelectionMethod)
            {
                case SelectionMethod.Select:
                case SelectionMethod.Add:
                default:
                    return layer.FeatureSet.Features.ToList();
                case SelectionMethod.RemoveFromSelected:
                case SelectionMethod.SelectFromSelected:
                    var selection = layer.Selection.ToFeatureList();
                    List<IFeature> list = new List<IFeature>();
                    foreach (var s in selection)
                    {
                        list.Add(s);
                    }
                    return list;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (SelectedSourceLayer == null || SelectedSourceLayer.Layer == null) return;
            this.SelectedFeasInfo = _SelectedSourceLayer.Layer.Selection.Count + " feature(s) selected";
        }
    }
}
