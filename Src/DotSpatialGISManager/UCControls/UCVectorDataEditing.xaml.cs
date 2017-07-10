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
    public partial class UCVectorDataEditing : UserControl
    {
        public static CreatePointDlg m_CreatePointDlg = null;
        public static CreatePolylineDlg m_CreatePolylineDlg = null;
        public static CreatePolygonDlg m_CreatePolygonDlg = null;
        public static DeleteFeatureDlg m_DeleteFeatureDlg = null;
        public static ToggleButton m_btnSelect = null;
        public static MoveNodesDlg m_MoveNodeDlg = null;
        public static MoveFeatureDlg m_MoveFeatureDlg = null;
        public static RotateFeatureDlg m_RotateFeatureDlg = null;
        public static MergeDlg m_MergeDlg = null;

        public UCVectorDataEditing()
        {
            InitializeComponent();
            this.DataContext = this;
            m_btnSelect = this.btnSelectFeature;

            disableButton();
        }

        //绑定图标路径
        #region

        public string SelectFeaturePath
        {
            get
            {
                return PathHelper.ResourcePath + "01.select feature.png";
            }
        }

        public string StartEditingPath
        {
            get
            {
                return PathHelper.ResourcePath + "02.start editing.png";
            }
        }

        public string CreatePointPath
        {
            get
            {
                return PathHelper.ResourcePath + "02.create point.png";
            }
        }

        public string CreatePolylinPath
        {
            get
            {
                return PathHelper.ResourcePath + "02.create polyline.png";
            }
        }

        public string CreatePolygonPath
        {
            get
            {
                return PathHelper.ResourcePath + "02.create polygon.png";
            }
        }

        public string DeleteFeaturePath
        {
            get
            {
                return PathHelper.ResourcePath + "02.delete feature.png";
            }
        }

        public string MoveFeaturePath
        {
            get
            {
                return PathHelper.ResourcePath + "02.move feature.png";
            }
        }

        public string RotateFeaturePath
        {
            get
            {
                return PathHelper.ResourcePath + "02.rotate feature.png";
            }
        }

        public string MoveNodePath
        {
            get
            {
                return PathHelper.ResourcePath + "02.move node.png";
            }
        }

        public string MergeFeaturePath
        {
            get
            {
                return PathHelper.ResourcePath + "02.feature combination.png";
            }
        }

        public string SplitFeaturePath
        {
            get
            {
                return PathHelper.ResourcePath + "02.feature depart.png";
            }
        }

        public string SaveEditsPath
        {
            get
            {
                return PathHelper.ResourcePath + "02.save edits.png";
            }
        }
        #endregion

        private void btnSelectFeature_Click(object sender, RoutedEventArgs e)
        {
            if (this.btnSelectFeature.IsChecked == true)
            {
                MainWindow.m_DotMap.FunctionMode = FunctionMode.Select;
            }
            else
            {
                MainWindow.m_DotMap.FunctionMode = FunctionMode.None;
            }
        }

        private void btnDeleteFeature_Click(object sender, RoutedEventArgs e)
        {
            if (m_DeleteFeatureDlg == null)
            {
                m_DeleteFeatureDlg = new DeleteFeatureDlg();
                m_DeleteFeatureDlg.Show();
            }
        }

        private void btnCreatePoint_Click(object sender, RoutedEventArgs e)
        {
            if (m_CreatePointDlg== null)
            {
                m_CreatePointDlg = new CreatePointDlg();
                m_CreatePointDlg.Show();
            }
        }

        private void btnCreatePolyline_Click(object sender, RoutedEventArgs e)
        {
            if (m_CreatePolylineDlg == null)
            {
                m_CreatePolylineDlg = new CreatePolylineDlg();
                m_CreatePolylineDlg.Show();
            }
        }

        private void btnCreatePolygon_Click(object sender, RoutedEventArgs e)
        {
            if (m_CreatePolygonDlg == null)
            {
                m_CreatePolygonDlg = new CreatePolygonDlg();
                m_CreatePolygonDlg.Show();
            }
        }

        private void btnStartEdit_Click(object sender, RoutedEventArgs e)
        {
            enableButton();
        }

        private void btnMoveFeature_Click(object sender, RoutedEventArgs e)
        {
            if (m_MoveFeatureDlg == null)
            {
                m_MoveFeatureDlg = new MoveFeatureDlg();
                m_MoveFeatureDlg.Show();
            }
        }

        private void btnRotateFeature_Click(object sender, RoutedEventArgs e)
        {
            if (m_RotateFeatureDlg == null)
            {
                m_RotateFeatureDlg = new RotateFeatureDlg();
                m_RotateFeatureDlg.Show();
            }
        }

        private void btnMoveNodes_Click(object sender, RoutedEventArgs e)
        {
            if (m_MoveNodeDlg == null)
            {
                m_MoveNodeDlg = new MoveNodesDlg();
                m_MoveNodeDlg.Show();
            }
        }

        private void btnMergeFeature_Click(object sender, RoutedEventArgs e)
        {
            if (m_MergeDlg == null)
            {
                m_MergeDlg = new MergeDlg();
                m_MergeDlg.Show();
            }
        }

        private void btnSplitFeature_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSaveEdits_Click(object sender, RoutedEventArgs e)
        {
            disableButton();
        }

        private void disableButton()
        {
            btnDeleteFeature.IsEnabled = false;
            btnDeleteFeature.Opacity = 0.5;

            btnCreatePoint.IsEnabled = false;
            btnCreatePoint.Opacity = 0.5;

            btnCreatePolyline.IsEnabled = false;
            btnCreatePolyline.Opacity = 0.5;

            btnCreatePolygon.IsEnabled = false;
            btnCreatePolygon.Opacity = 0.5;

            btnMoveFeature.IsEnabled = false;
            btnMoveFeature.Opacity = 0.5;

            btnRotateFeature.IsEnabled = false;
            btnRotateFeature.Opacity = 0.5;

            btnMoveNodes.IsEnabled = false;
            btnMoveNodes.Opacity = 0.5;

            btnMergeFeature.IsEnabled = false;
            btnMergeFeature.Opacity = 0.5;

            btnSplitFeature.IsEnabled = false;
            btnSplitFeature.Opacity = 0.5;

            btnSaveEdits.IsEnabled = false;
            btnSaveEdits.Opacity = 0.5;
        }
        private void enableButton()
        {
            btnDeleteFeature.IsEnabled = true;
            btnDeleteFeature.Opacity = 1;

            btnCreatePoint.IsEnabled = true;
            btnCreatePoint.Opacity = 1;

            btnCreatePolyline.IsEnabled = true;
            btnCreatePolyline.Opacity = 1;

            btnCreatePolygon.IsEnabled = true;
            btnCreatePolygon.Opacity = 1;

            btnMoveFeature.IsEnabled = true;
            btnMoveFeature.Opacity = 1;

            btnRotateFeature.IsEnabled = true;
            btnRotateFeature.Opacity = 1;

            btnMoveNodes.IsEnabled = true;
            btnMoveNodes.Opacity = 1;

            btnMergeFeature.IsEnabled = true;
            btnMergeFeature.Opacity = 1;

            btnSplitFeature.IsEnabled = true;
            btnSplitFeature.Opacity = 1;

            btnSaveEdits.IsEnabled = true;
            btnSaveEdits.Opacity = 1;
        }
    }
}
