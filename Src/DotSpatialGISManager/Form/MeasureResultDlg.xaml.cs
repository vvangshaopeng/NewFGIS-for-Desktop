using DotSpatial.Controls;
using DotSpatial.Data;
using DotSpatial.Symbology;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using DotSpatial.Projections;

namespace DotSpatialGISManager
{
    /// <summary>
    /// MeasureResultDlg.xaml 的交互逻辑
    /// </summary>
    public partial class MeasureResultDlg : Window
    {
        public MeasureResultDlg()
        {
            InitializeComponent();
            //this.DataContext = this;
        }

        //private ObservableCollection<MeasureModel> _DataGridList;
        //public ObservableCollection<MeasureModel> DataGridList
        //{
        //    get
        //    {
        //        return _DataGridList;
        //    }
        //    set
        //    {
        //        if (_DataGridList!=value)
        //        {
        //            _DataGridList = value;
        //            if (this.PropertyChanged != null)
        //            {
        //                this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("DataGridList"));
        //            }
        //        }
        //    }
        //}

        public void Init()
        {
            List<ILayer> m_LayerList = MainWindow.m_DotMap.GetAllLayers();
            List<MeasureModel> DataGridList = new List<MeasureModel>();
            foreach (ILayer layer in m_LayerList)
            {
                if (layer is IFeatureLayer)
                {
                    IFeatureLayer pFeaLaeyr = (layer as IFeatureLayer);
                    ISelection selectFea = pFeaLaeyr.Selection;
                    IFeatureSet pFeaSet = selectFea.ToFeatureSet();
                    foreach (IFeature fea in pFeaSet.Features)
                    {
                        string id = fea.Fid.ToString();
                        if (pFeaSet.DataTable.Columns.Contains("Name"))
                            id = fea.Fid.ToString() + " " + fea.DataRow["Name"];
                        DataGridList.Add(new MeasureModel(id, pFeaLaeyr.DataSet.Name, fea.Geometry.Length.ToString("F4"), fea.Geometry.Area.ToString("F4"), fea.Geometry.GeometryType.ToString()));
                    }
                }
            }
            if (this.rgrvInfo.ItemsSource != null)
                this.rgrvInfo.ItemsSource = null;
            this.rgrvInfo.ItemsSource = DataGridList;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            UCControls.UCMapSurvying.m_MeasureResultDlg = null;
            MainWindow.m_DotMap.FunctionMode = FunctionMode.Pan;
        }
    }
}
