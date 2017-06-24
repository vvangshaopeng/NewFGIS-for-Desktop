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
using DotSpatial.Analysis;

namespace DotSpatialGISManager.UCControls
{
    /// <summary>
    /// UCOverlay.xaml 的交互逻辑
    /// </summary>
    public partial class UCOverlay : UserControl
    {
        public UCOverlay()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        //绑定图标路径
        #region
        public string UnionPath
        {
            get
            {
                return PathHelper.ResourcePath + "08.union.png";
            }
        }

        public string IntersectPath
        {
            get
            {
                return PathHelper.ResourcePath + "08.intersect.png";
            }
        }
        #endregion

        private void btnUnion_Click(object sender, RoutedEventArgs e)
        {
            OverlayUnionDlg f = new OverlayUnionDlg();
            f.ShowDialog();
        }

        private void btnIntersect_Click(object sender, RoutedEventArgs e)
        {
            OverlayIntersectDlg f = new OverlayIntersectDlg();
            f.ShowDialog();
        }
    }
}
