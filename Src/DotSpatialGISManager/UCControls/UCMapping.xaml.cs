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
    public partial class UCMapping : UserControl
    {
        public UCMapping()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        //绑定图标路径
        #region
        public string MappingPath
        {
            get
            {
                return PathHelper.ResourcePath + "11.mapping.png";
            }
        }
        public string UploadMapPath
        {
            get
            {
                return PathHelper.ResourcePath + "11.upload map.png";
            }
        }


        #endregion

        private void btnMapping_Click(object sender, RoutedEventArgs e)
        {
            LayoutForm f = new LayoutForm();
            f.MapControl = MainWindow.m_DotMap;
            f.Show();
        }

        private void btnUploadMap_Click(object sender, RoutedEventArgs e)
        {
            UploadMapDlg f = new UploadMapDlg();
            f.Show();
        }
    }
}
