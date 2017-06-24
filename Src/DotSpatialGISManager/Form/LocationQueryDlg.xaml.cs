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
        private string m_ResourcePath = AppDomain.CurrentDomain.BaseDirectory + "../Resources/";

        private ObservableCollection<LocationLayerModel> _TargetLayers;
        public ObservableCollection<LocationLayerModel> TargetLayers
        {
            get
            {
                return _TargetLayers??(_TargetLayers = new ObservableCollection<LocationLayerModel>());
            }
            set
            {
                _TargetLayers = value;
            }
        }

        private ObservableCollection<LocationLayerModel> _SourceLayers;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<LocationLayerModel> SourceLayers
        {
            get
            {
                return _SourceLayers??(_SourceLayers = new ObservableCollection<LocationLayerModel>());
            }
            set
            {
                if (_SourceLayers != value)
                {
                    _SourceLayers = value;
                    if (this.PropertyChanged!=null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("SourceLayer"));
                    }
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
                    if (_SelectedSourceLayer!=null && _SelectedSourceLayer.Layer!=null)
                        this.SelectedFeasInfo = _SelectedSourceLayer.Layer.Selection.Count + " feature(s) selected";
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("SelectedSourceLayer"));
                    }
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
                if (_UsingSelectedFeas!=value)
                {
                    _UsingSelectedFeas = value;
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("UsingSelectedFeas"));
                    }
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
            switch(s)
            {
                case FeatureType.Point:
                case FeatureType.MultiPoint:
                    return m_ResourcePath + "point.png";
                case FeatureType.Line:
                    return m_ResourcePath + "polylin.png";
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
            switch(this.cboMethod.SelectedIndex)
            {
                case 0:
                    m_SelectionMethod = SelectionMethod.Select;
                    break;
                case 1:
                    m_SelectionMethod = SelectionMethod.Add;
                    break;
                case 2:
                    m_SelectionMethod = SelectionMethod.Remove;
                    break;
                case 3:
                    m_SelectionMethod = SelectionMethod.RemoveFromSelected;
                    break;
            }
        }

        private void cboSelectionmethod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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
            var TargetLayerList = (from u in TargetLayers
                                   where u.IsChecked
                                   select u).ToList();
            if (TargetLayers.Count == 0)
            {
                MessageBox.Show("Please select at least one target layer");
                return;
            }
            if (SelectedSourceLayer == null)
            {
                MessageBox.Show("Please select a source layer");
                return;
            }
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            var TargetLayerList = (from u in TargetLayers
                                   where u.IsChecked
                                   select u).ToList();
            if (TargetLayers.Count == 0)
            {
                MessageBox.Show("Please select at least one target layer");
                return;
            }
            if (SelectedSourceLayer == null)
            {
                MessageBox.Show("Please select a source layer");
                return;
            }
            //Union Geometry
            ProgressBox p = new ProgressBox(0, 100, "Location query progress");
            p.ShowPregress();
            p.SetProgressValue(0);
            p.SetProgressDescription("Unioning geometry...");
            IGeometry resultGeo = null;
            IFeatureSet srcFeaset = null;
            if (this.UsingSelectedFeas)
                srcFeaset = SelectedSourceLayer.Layer.Selection.ToFeatureSet();
            else
                srcFeaset = SelectedSourceLayer.Layer.FeatureSet;
            foreach (var fea in srcFeaset.Features)
            {
                if (resultGeo == null)
                    resultGeo = fea.Geometry;
                else
                    resultGeo = resultGeo.Union(fea.Geometry);
            }
            p.SetProgressValue(10);
            switch(m_SpatialMethod)
            {
                case SpatialMethod.Intersect:
                    foreach(var layer in TargetLayerList)
                    {
                        List<int> FidList = new List<int>();
                        var tarFeaList = layer.Layer.FeatureSet.Features;
                        int tarCount = tarFeaList.Count;
                        double pi = Math.Round((double)(100 * 1.0 / tarCount),2);
                        p.SetProgressDescription("Querying "+layer.Layer.LegendText);
                        for(int i = 0;i< tarCount; i++)
                        {
                            p.SetProgressValue(10 + pi * (i + 1));
                            p.SetProgressDescription2(string.Format("{0} feature(s) is(are) queried, the remaining {1} feature(s) is(are) being queried",i+1,tarCount-i-1));
                            if (tarFeaList[i].Intersection(resultGeo)!=null)
                            {
                                FidList.Add(tarFeaList[i].Fid);
                            }
                        }
                        layer.Layer.Select(FidList);
                    }
                    break;
                case SpatialMethod.Contains:
                    break;
                case SpatialMethod.Within:
                    break;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
