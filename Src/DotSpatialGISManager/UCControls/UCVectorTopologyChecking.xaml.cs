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
    public partial class UCVectorTopologyChecking : UserControl
    {
        public UCVectorTopologyChecking()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        //绑定图标路径
        #region
        public string SuspensionPath
        {
            get
            {
                return PathHelper.ResourcePath + "03.suspension.png";
            }
        }

        public string SelfIntersectionPath
        {
            get
            {
                return PathHelper.ResourcePath + "03.self intersection.png";
            }
        }

        public string FayingSurfacePath
        {
            get
            {
                return PathHelper.ResourcePath + "03.faying surface.png";
            }
        }
        #endregion
        /// <summary>
        /// 导出数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSuspension_Click(object sender, RoutedEventArgs e)
        {
            SuspensionPointDlg f = new SuspensionPointDlg();
            f.ShowDialog();
        }
        /// <summary>
        /// 导出数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSelfIntersection_Click(object sender, RoutedEventArgs e)
        {
            SelfIntersectionDlg f = new SelfIntersectionDlg();
            f.ShowDialog();
        }
        /// <summary>
        /// 重叠面检查
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFayingSurface_Click(object sender, RoutedEventArgs e)
        {
            FayingSurfaceDlg f = new FayingSurfaceDlg();
            f.ShowDialog();
        }

    }
}
