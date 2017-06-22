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

namespace DotSpatialGISManager.UCControls
{
    /// <summary>
    /// UCFileManagement.xaml 的交互逻辑
    /// </summary>
    public partial class UCTopologyAnalysis : UserControl
    {
        public UCTopologyAnalysis()
        {
            InitializeComponent();
            this.DataContext = this;
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

        }

        private void btnConnectivity_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
