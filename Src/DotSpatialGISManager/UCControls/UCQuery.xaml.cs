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
    public partial class UCQuery : UserControl
    {
        public static SQLQueryDlg m_SQLQueryDlg = null;
        public static LocationQueryDlg m_LocationQueryDlg = null;
        public UCQuery()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        //绑定图标路径
        #region
        public string AttributeQueryPath
        {
            get
            {
                return PathHelper.ResourcePath + "07.attribute query.png";
            }
        }

        public string LocationQueryPath
        {
            get
            {
                return PathHelper.ResourcePath + "07.location query.png";
            }
        }
        #endregion

        private void btnLocationQuery_Click(object sender, RoutedEventArgs e)
        {
            if (m_LocationQueryDlg == null)
            {
                m_LocationQueryDlg = new LocationQueryDlg();
                m_LocationQueryDlg.Show();
            }
        }

        private void btnAttributeQuery_Click(object sender, RoutedEventArgs e)
        {
            if (m_SQLQueryDlg == null)
            {
                SQLQueryDlg f = new SQLQueryDlg();
                f.Show();
            }
        }
    }
}
