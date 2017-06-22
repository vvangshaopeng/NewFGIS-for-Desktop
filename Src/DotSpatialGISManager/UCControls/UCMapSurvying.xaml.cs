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
    public partial class UCMapSurvying : UserControl
    {
        public static MeasureResultDlg m_MeasureResultDlg = null;
        public UCMapSurvying()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        //绑定图标路径
        #region
        public string MeasurePath
        {
            get
            {
                return PathHelper.ResourcePath + "06.surveying.png";
            }
        }

        #endregion

        private void btnMeasure_Click(object sender, RoutedEventArgs e)
        {
            if (m_MeasureResultDlg == null)
            {
                m_MeasureResultDlg = new MeasureResultDlg();
                m_MeasureResultDlg.Owner = MainWindow.m_MainWindow;
            }
            m_MeasureResultDlg.Init();
            m_MeasureResultDlg.Show();
            MainWindow.m_DotMap.FunctionMode = FunctionMode.Select;
        }
    }
}
