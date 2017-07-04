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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Common.Helper;

using DotSpatial.Data;
using Microsoft.Win32;
using DotSpatial.Controls;
using DotSpatial.Symbology;
using DotSpatialGISManager.Enum;

namespace DotSpatialGISManager.UCControls
{
    /// <summary>
    /// UCFileManagement.xaml 的交互逻辑
    /// </summary>
    public partial class UCTopologyAnalysis : UserControl
    {
        private IFeatureSet m_ResultFeaset = null;
        public int ClickIndex = 0;
        public static UCTopologyAnalysis m_UCTopologyAnalysis = null;
        public ILayer m_PointLayer
        {
            get;
            private set;
        }

        public static UCTopologyAnalysis GetIntance()
        {
            if (m_UCTopologyAnalysis == null)
            {
                m_UCTopologyAnalysis = new UCTopologyAnalysis();
            }
            return m_UCTopologyAnalysis;
        }

        public UCTopologyAnalysis()
        {
            InitializeComponent();
            this.DataContext = this;
            m_UCTopologyAnalysis = this;
        }

        //绑定图标路径
        #region
        public string StartEndPath
        {
            get
            {
                return PathHelper.ResourcePath + "10.set start_end point.png";
            }
        }

        public string ConnectivityPath
        {
            get
            {
                return PathHelper.ResourcePath + "10.Connectivity analysis.png";
            }
        }
        #endregion

        private void btnStartEnd_Click(object sender, RoutedEventArgs e)
        {
            //start 
            MainWindow.m_DotMap.Cursor = System.Windows.Forms.Cursors.Cross;
            //创建新的FeatureSet 
            m_ResultFeaset = new FeatureSet(FeatureType.Point);
            m_ResultFeaset.Projection = MainWindow.m_DotMap.Projection;
            m_ResultFeaset.Name = "start_end";
            m_PointLayer = MainWindow.m_DotMap.Layers.Add(m_ResultFeaset);
            m_PointLayer.LegendText = m_ResultFeaset.Name;
            MainWindow.m_AddFeaType = FeaType.UCPoint;
        }

        private void btnConnectivity_Click(object sender, RoutedEventArgs e)
        {
            ConnectivityAnalysisDlg f = new ConnectivityAnalysisDlg(m_PointLayer);
            f.ShowDialog();
        }
    }
}
