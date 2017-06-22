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
using System.Windows.Controls.Primitives;

namespace DotSpatialGISManager.UCControls
{
    /// <summary>
    /// UCFileManagement.xaml 的交互逻辑
    /// </summary>
    public partial class UCLayerConnection : UserControl
    {
        public UCLayerConnection()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        //绑定图标路径
        #region
        public string SpatialJointPath
        {
            get
            {
                return PathHelper.ResourcePath + "05.location .png";
            }
        }

        public string AttributeJointPath
        {
            get
            {
                return PathHelper.ResourcePath + "05.attribute.png";
            }
        }

        #endregion

        private void btnSpatialJoint_Click(object sender, RoutedEventArgs e)
        {
            SpatialJointDlg f = new SpatialJointDlg();
            f.ShowDialog();
        }

        private void btnAttributeJoint_Click(object sender, RoutedEventArgs e)
        {
            AttributeJointDlg f = new AttributeJointDlg();
            f.ShowDialog();
        }
    }
}
