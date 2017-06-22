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
    public partial class UCBufferAnalysis : UserControl
    {
        public UCBufferAnalysis()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        //绑定图标路径
        #region
        public string PointBufferPath
        {
            get
            {
                return PathHelper.ResourcePath + "09.point buffer.png";
            }
        }

        public string PolylineBufferPath
        {
            get
            {
                return PathHelper.ResourcePath + "09.line buffer.png";
            }
        }

        public string PolygonBufferPath
        {
            get
            {
                return PathHelper.ResourcePath + "09.polygon buffer.png";
            }
        }
        #endregion
        /// <summary>
        /// 导出数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPointBuffer_Click(object sender, RoutedEventArgs e)
        {

        }
        /// <summary>
        /// 导出数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPolylineBuffer_Click(object sender, RoutedEventArgs e)
        {

        }
        /// <summary>
        /// 导出数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPolygonBuffer_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}
