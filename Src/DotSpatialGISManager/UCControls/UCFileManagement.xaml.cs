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
    public partial class UCFileManagement : UserControl
    {
        public UCFileManagement()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        //绑定图标路径
        #region
        public string ImportDataPath
        {
            get
            {
                return PathHelper.ResourcePath + "01.import data.png";
            }
        }

        public string ExportDataPath
        {
            get
            {
                return PathHelper.ResourcePath + "01.export data.png";
            }
        }

        public string FormatDataPath
        {
            get
            {
                return PathHelper.ResourcePath + "01.format change.png";
            }
        }

        public string AddLayerPath
        {
            get
            {
                return PathHelper.ResourcePath + "01.add layer.png";
            }
        }
        #endregion

        /// <summary>
        /// 添加图层
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddLayer_Click(object sender, RoutedEventArgs e)
        {
            var layers = MainWindow.m_DotMap.AddLayers();
            MainWindow.m_DotMap.ZoomToMaxExtent();
        }

        /// <summary>
        /// 导入数据（Excel）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnImprotData_Click(object sender, RoutedEventArgs e)
        {
            ImportExcelDlg f = new ImportExcelDlg();
            if (f.ShowDialog() == true)
            {
                IFeatureSet pFeaSet = f.ResultFeaSet;
                IMapFeatureLayer layer = MainWindow.m_DotMap.Layers.Add(pFeaSet);
                //底层有bug 这里再赋值一次 否则无法显示属性表
                layer.DataSet.DataTable = f.m_DataTable;
                MainWindow.m_DotMap.ZoomToMaxExtent();
            }
        }

        /// <summary>
        /// 导出数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExportData_Click(object sender, RoutedEventArgs e)
        {
            ExportShpDlg f = new ExportShpDlg();
            f.ShowDialog();
        }
    }
}
