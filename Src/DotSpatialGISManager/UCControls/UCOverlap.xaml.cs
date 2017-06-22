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
    public partial class UCOverlap : UserControl
    {
        public UCOverlap()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        //绑定图标路径
        #region
        public string SumPath
        {
            get
            {
                return PathHelper.ResourcePath + "08.sum.png";
            }
        }

        public string IntersectionPath
        {
            get
            {
                return PathHelper.ResourcePath + "08.intersection.png";
            }
        }
        #endregion

        private void btnSum_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnIntersection_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
